using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tempus.AspNetCore.Context;
using Tempus.AspNetCore.DependencyInjection;
using Tempus.Core.DependencyInjection;
using Tempus.Testing.Clocks;

namespace Tempus.AspNetCore.Tests;

public class TempusUserContextTests
{
    private static ITempusUserContextFactory BuildFactory(DateTimeOffset now)
    {
        var clock = new FakeClock(now);
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        var builder = services.AddTempus();
        builder.UseFakeClock(clock);
        services.AddTempusAspNetCore();
        return services.BuildServiceProvider().GetRequiredService<ITempusUserContextFactory>();
    }

    [Fact]
    public void Create_SetsTimeZoneId()
    {
        var factory = BuildFactory(DateTimeOffset.UtcNow);
        var ctx = factory.Create("America/New_York");
        ctx.TimeZoneId.Should().Be("America/New_York");
    }

    [Fact]
    public void Create_SetsTimeZoneInfo()
    {
        var factory = BuildFactory(DateTimeOffset.UtcNow);
        var ctx = factory.Create("America/New_York");
        ctx.TimeZone.Should().NotBeNull();
    }

    [Fact]
    public void NowForUser_ReflectsOffsetOfTimeZone()
    {
        var utc = new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var factory = BuildFactory(utc);
        var ctx = factory.Create("America/New_York"); // UTC-4 in summer (EDT)

        // 12:00 UTC = 08:00 EDT
        ctx.NowForUser.Hour.Should().Be(8);
    }

    [Fact]
    public void ToUserTime_ConvertsUtcToLocalOffset()
    {
        var factory = BuildFactory(DateTimeOffset.UtcNow);
        var ctx = factory.Create("Europe/London");

        var utc = new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero);
        var local = ctx.ToUserTime(utc);

        // BST = UTC+1, so 10:00 UTC = 11:00 BST
        local.Hour.Should().Be(11);
    }

    [Fact]
    public void ToUtc_ConvertsLocalToUtc()
    {
        var factory = BuildFactory(DateTimeOffset.UtcNow);
        var ctx = factory.Create("America/New_York");

        // During EDT (UTC-4): 14:00 local = 18:00 UTC
        var local = new DateTime(2026, 7, 1, 14, 0, 0);
        var utc = ctx.ToUtc(local);

        utc.UtcDateTime.Hour.Should().Be(18);
    }

    [Fact]
    public void Format_UsesDefaultFormat()
    {
        var factory = BuildFactory(DateTimeOffset.UtcNow);
        var ctx = factory.Create("Etc/UTC");

        var utc = new DateTimeOffset(2026, 6, 15, 9, 30, 0, TimeSpan.Zero);
        var formatted = ctx.Format(utc);

        formatted.Should().StartWith("2026-06-15");
    }

    [Fact]
    public void Format_UsesCustomFormat()
    {
        var factory = BuildFactory(DateTimeOffset.UtcNow);
        var ctx = factory.Create("Etc/UTC");

        var utc = new DateTimeOffset(2026, 6, 15, 9, 30, 0, TimeSpan.Zero);
        var formatted = ctx.Format(utc, "yyyy/MM/dd");

        formatted.Should().Be("2026/06/15");
    }

    [Fact]
    public void TodayForUser_ReturnsLocalDate()
    {
        // 23:30 UTC = 19:30 EDT (UTC-4) — still same UTC day, different local day
        var utc = new DateTimeOffset(2026, 6, 15, 23, 30, 0, TimeSpan.Zero);
        var factory = BuildFactory(utc);
        var ctx = factory.Create("America/New_York");

        ctx.TodayForUser.Should().Be(new DateOnly(2026, 6, 15));
    }
}
