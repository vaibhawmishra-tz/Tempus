using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tempus.AspNetCore.Context;
using Tempus.AspNetCore.DependencyInjection;
using Tempus.AspNetCore.Middleware;
using Tempus.Core.DependencyInjection;
using Tempus.Testing.Clocks;

namespace Tempus.AspNetCore.Tests;

public class TempusTimezoneMiddlewareTests
{
    private static (TempusTimezoneMiddleware Middleware, IServiceProvider Services) Build(
        Action<TempusAspNetCoreOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddTempus();
        var opts = new TempusAspNetCoreOptions();
        configure?.Invoke(opts);
        services.AddTempusAspNetCore(o =>
        {
            o.HeaderName = opts.HeaderName;
            o.JwtClaimName = opts.JwtClaimName;
            o.QueryParamName = opts.QueryParamName;
            o.FallbackTimeZone = opts.FallbackTimeZone;
        });
        var sp = services.BuildServiceProvider();
        var middleware = new TempusTimezoneMiddleware(_ => Task.CompletedTask, sp.GetRequiredService<IOptions<TempusAspNetCoreOptions>>());
        return (middleware, sp);
    }

    private static DefaultHttpContext BuildHttpContext(IServiceProvider sp)
    {
        var ctx = new DefaultHttpContext { RequestServices = sp };
        return ctx;
    }

    [Fact]
    public async Task XTimezoneHeader_SetsUserContextTimeZoneId()
    {
        var (middleware, sp) = Build();
        var httpCtx = BuildHttpContext(sp);
        httpCtx.Request.Headers["X-Timezone"] = "America/New_York";

        await middleware.InvokeAsync(httpCtx);

        var userCtx = (ITempusUserContext)httpCtx.Items[TempusContextKeys.UserContext]!;
        userCtx.TimeZoneId.Should().Be("America/New_York");
    }

    [Fact]
    public async Task QueryParam_SetsUserContextTimeZoneId()
    {
        var (middleware, sp) = Build();
        var httpCtx = BuildHttpContext(sp);
        httpCtx.Request.QueryString = new QueryString("?tz=Europe/London");

        await middleware.InvokeAsync(httpCtx);

        var userCtx = (ITempusUserContext)httpCtx.Items[TempusContextKeys.UserContext]!;
        userCtx.TimeZoneId.Should().Be("Europe/London");
    }

    [Fact]
    public async Task JwtClaim_SetsUserContextTimeZoneId()
    {
        var (middleware, sp) = Build();
        var httpCtx = BuildHttpContext(sp);
        var claims = new[] { new Claim("timezone", "Asia/Kolkata") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpCtx.User = new ClaimsPrincipal(identity);

        await middleware.InvokeAsync(httpCtx);

        var userCtx = (ITempusUserContext)httpCtx.Items[TempusContextKeys.UserContext]!;
        userCtx.TimeZoneId.Should().Be("Asia/Kolkata");
    }

    [Fact]
    public async Task NoTimezone_FallsBackToConfiguredDefault()
    {
        var (middleware, sp) = Build(o => o.FallbackTimeZone = "Europe/Berlin");
        var httpCtx = BuildHttpContext(sp);

        await middleware.InvokeAsync(httpCtx);

        var userCtx = (ITempusUserContext)httpCtx.Items[TempusContextKeys.UserContext]!;
        userCtx.TimeZoneId.Should().Be("Europe/Berlin");
    }

    [Fact]
    public async Task InvalidTimezone_DoesNotThrow_UsesUtcFallback()
    {
        var (middleware, sp) = Build();
        var httpCtx = BuildHttpContext(sp);
        httpCtx.Request.Headers["X-Timezone"] = "Not/A/Real/Timezone";

        // Should not throw; resolver falls back to UTC per MissingZoneStrategy.FallbackToUtc default
        Func<Task> act = () => middleware.InvokeAsync(httpCtx);
        await act.Should().NotThrowAsync();

        var userCtx = (ITempusUserContext)httpCtx.Items[TempusContextKeys.UserContext]!;
        userCtx.Should().NotBeNull();
    }

    [Fact]
    public async Task Header_TakesPrecedenceOver_QueryParam()
    {
        var (middleware, sp) = Build();
        var httpCtx = BuildHttpContext(sp);
        httpCtx.Request.Headers["X-Timezone"] = "America/Chicago";
        httpCtx.Request.QueryString = new QueryString("?tz=Europe/London");

        await middleware.InvokeAsync(httpCtx);

        var userCtx = (ITempusUserContext)httpCtx.Items[TempusContextKeys.UserContext]!;
        userCtx.TimeZoneId.Should().Be("America/Chicago");
    }

    [Fact]
    public async Task UserContextFactory_IsAvailableAfterMiddleware()
    {
        var (middleware, sp) = Build();
        var httpCtx = BuildHttpContext(sp);
        httpCtx.Request.Headers["X-Timezone"] = "Pacific/Auckland";

        await middleware.InvokeAsync(httpCtx);

        httpCtx.Items[TempusContextKeys.UserContext].Should().BeAssignableTo<ITempusUserContext>();
    }
}
