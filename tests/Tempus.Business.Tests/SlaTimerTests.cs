using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tempus.Business.Abstractions;
using Tempus.Business.DependencyInjection;
using Tempus.Business.Models;
using Tempus.Core.Abstractions;
using Tempus.Core.DependencyInjection;
using Tempus.Testing.Clocks;

namespace Tempus.Business.Tests;

public class SlaTimerTests
{
    private static (ISlaTimerFactory factory, FakeClock clock) BuildFactory(DateTimeOffset now)
    {
        var clock = new FakeClock(now);
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        var builder = services.AddTempus();
        builder.UseFakeClock(clock);
        services.AddBusinessCalendar(opts =>
            opts.BusinessHours = BusinessHours.NineToFive("UTC"));
        services.AddSlaTimer();
        var factory = services.BuildServiceProvider().GetRequiredService<ISlaTimerFactory>();
        return (factory, clock);
    }

    [Fact]
    public void ElapsedBusinessTime_TwoHoursIn_ReturnsTwoHours()
    {
        var start = new DateTimeOffset(2026, 5, 25, 9, 0, 0, TimeSpan.Zero); // Monday 09:00 UTC
        var (factory, _) = BuildFactory(start.AddHours(2));

        var sla = factory.Create(start, TimeSpan.FromHours(4));

        sla.ElapsedBusinessTime.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void IsBreached_WhenElapsedIsLessThanTarget_ReturnsFalse()
    {
        var start = new DateTimeOffset(2026, 5, 25, 9, 0, 0, TimeSpan.Zero);
        var (factory, _) = BuildFactory(start.AddHours(2));

        var sla = factory.Create(start, TimeSpan.FromHours(4));

        sla.IsBreached.Should().BeFalse();
    }

    [Fact]
    public void RemainingBusinessTime_TwoHoursIn_ReturnsTwoHours()
    {
        var start = new DateTimeOffset(2026, 5, 25, 9, 0, 0, TimeSpan.Zero);
        var (factory, _) = BuildFactory(start.AddHours(2));

        var sla = factory.Create(start, TimeSpan.FromHours(4));

        sla.RemainingBusinessTime.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void IsBreached_WhenTargetExceeded_ReturnsTrue()
    {
        var start = new DateTimeOffset(2026, 5, 25, 9, 0, 0, TimeSpan.Zero);
        var (factory, _) = BuildFactory(start.AddHours(5)); // 5h elapsed, 4h target

        var sla = factory.Create(start, TimeSpan.FromHours(4));

        sla.IsBreached.Should().BeTrue();
    }

    [Fact]
    public void BreachesAt_SameDay_IsStartPlusTargetHours()
    {
        var start = new DateTimeOffset(2026, 5, 25, 9, 0, 0, TimeSpan.Zero);
        var (factory, _) = BuildFactory(start);

        var sla = factory.Create(start, TimeSpan.FromHours(4));

        sla.BreachesAt.Should().Be(new DateTimeOffset(2026, 5, 25, 13, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void BreachesAt_SpansWeekend_SkipsNonBusinessDays()
    {
        // Friday 15:00 UTC (May 22 2026) + 4 business hours spans to Monday 11:00 UTC (May 25 2026)
        // Fri 15:00–17:00 = 2 hours; Mon 09:00–11:00 = 2 more hours
        var start = new DateTimeOffset(2026, 5, 22, 15, 0, 0, TimeSpan.Zero);
        var (factory, _) = BuildFactory(start);

        var sla = factory.Create(start, TimeSpan.FromHours(4));

        sla.BreachesAt.Should().Be(new DateTimeOffset(2026, 5, 25, 11, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void CreateNow_SetsStartedAtToCurrentClockTime()
    {
        var now = new DateTimeOffset(2026, 5, 25, 10, 0, 0, TimeSpan.Zero);
        var (factory, _) = BuildFactory(now);

        var sla = factory.CreateNow(TimeSpan.FromHours(2));

        sla.StartedAt.Should().Be(now);
    }

    [Fact]
    public void ElapsedBusinessTime_ReflectsClock_AfterAdvancing()
    {
        var start = new DateTimeOffset(2026, 5, 25, 9, 0, 0, TimeSpan.Zero);
        var (factory, clock) = BuildFactory(start);
        var sla = factory.Create(start, TimeSpan.FromHours(4));

        clock.Advance(TimeSpan.FromHours(3));

        sla.ElapsedBusinessTime.Should().Be(TimeSpan.FromHours(3));
    }
}
