using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tempus.Core.Abstractions;
using Tempus.Core.Configuration;
using Tempus.Core.Conversion;
using Tempus.Core.Internal;

namespace Tempus.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static TempusBuilder AddTempus(
        this IServiceCollection services,
        Action<TempusOptions>? configure = null)
    {
        TempusOptions options = new();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton(options.Dst);
        services.AddSingleton(options.Timezone);
        services.AddSingleton(options.Display);

        services.TryAddSingleton<ITimezoneResolver, BundledTimezoneResolver>();
        services.TryAddSingleton<IDstResolver, DstResolver>();
        services.TryAddSingleton<ITempusClock, SystemTempusClock>();

        // Wire ambient accessor so extension methods work without DI at call sites
        services.AddSingleton<IAmbientAccessorInitializer, AmbientAccessorInitializer>();

        return new TempusBuilder(services);
    }

    /// <summary>
    /// Resolves the ambient accessor initializer so that <c>DateTimeOffset</c> / <c>DateTime</c>
    /// extension methods (<c>ToZone</c>, <c>ToRelativeString</c>, etc.) work throughout the app.
    /// Call this once after the <see cref="IServiceProvider"/> is built — e.g. in
    /// <c>UseTempusTimezone()</c> or at the start of your app's request pipeline.
    /// </summary>
    public static void EnsureTempusInitialized(this IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.GetRequiredService<IAmbientAccessorInitializer>();
    }

    /// <summary>
    /// Replaces the system clock with a controllable fake — call this in test setup.
    /// </summary>
    public static TempusBuilder UseFakeClock(this TempusBuilder builder, ITempusClock clock)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.RemoveAll<ITempusClock>();
        builder.Services.AddSingleton(clock);
        return builder;
    }
}
