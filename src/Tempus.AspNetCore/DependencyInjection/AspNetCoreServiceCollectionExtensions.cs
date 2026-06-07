using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Tempus.AspNetCore.Context;
using Tempus.AspNetCore.Json;
using Tempus.AspNetCore.Middleware;
using Tempus.Core.Abstractions;

namespace Tempus.AspNetCore.DependencyInjection;

public static class AspNetCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers Tempus ASP.NET Core services: timezone middleware support, per-request user context,
    /// and the ITempusUserContextFactory. Requires AddTempus() to be called first.
    /// </summary>
    public static IServiceCollection AddTempusAspNetCore(
        this IServiceCollection services,
        Action<TempusAspNetCoreOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
            services.Configure(configure);
        else
            services.AddOptions<TempusAspNetCoreOptions>();

        services.AddHttpContextAccessor();

        services.TryAddSingleton<ITempusUserContextFactory>(sp =>
            new TempusUserContextFactory(
                sp.GetRequiredService<ITempusClock>(),
                sp.GetRequiredService<ITimezoneResolver>()));

        // Resolve the per-request context from HttpContext.Items (populated by the middleware).
        // Falls back to UTC when called outside a real HTTP request (e.g. background services, tests).
        services.TryAddScoped<ITempusUserContext>(sp =>
        {
            var accessor = sp.GetRequiredService<IHttpContextAccessor>();
            if (accessor.HttpContext?.Items[TempusContextKeys.UserContext] is ITempusUserContext ctx)
                return ctx;
            return sp.GetRequiredService<ITempusUserContextFactory>().Create("UTC");
        });

        return services;
    }

    /// <summary>
    /// Registers Tempus JSON converters into ASP.NET Core MVC's JSON pipeline.
    /// <list type="bullet">
    ///   <item><see cref="Json.UtcDateTimeJsonConverter"/> is applied globally — all
    ///   <see cref="DateTimeOffset"/> values serialise as UTC ISO 8601.</item>
    ///   <item><see cref="Json.UserLocalTimeJsonConverter"/> and
    ///   <see cref="Json.RelativeTimeJsonConverter"/> activate per-property via
    ///   <c>[UserLocalTime]</c> and <c>[RelativeTime]</c> attributes.</item>
    /// </list>
    /// Also ensures core Tempus ASP.NET Core services are registered (equivalent to calling
    /// <c>AddTempusAspNetCore()</c> with default options) so that <c>UseTempusTimezone()</c>
    /// works without a separate explicit call.
    /// </summary>
    public static IMvcBuilder AddTempusJson(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Ensure the context factory and middleware options are registered.
        // TryAdd* semantics mean this is a no-op if AddTempusAspNetCore() was already called.
        builder.Services.AddTempusAspNetCore();
        builder.Services.TryAddSingleton<UserLocalTimeJsonConverter>(sp =>
            new UserLocalTimeJsonConverter(sp.GetRequiredService<IHttpContextAccessor>()));
        builder.Services.TryAddSingleton<RelativeTimeJsonConverter>(sp =>
            new RelativeTimeJsonConverter(sp.GetRequiredService<IHttpContextAccessor>()));

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigureOptions<JsonOptions>, TempusJsonOptionsSetup>(sp =>
                new TempusJsonOptionsSetup(sp.GetRequiredService<IHttpContextAccessor>())));

        return builder;
    }
}
