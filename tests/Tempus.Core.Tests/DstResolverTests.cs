using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tempus.Core.Abstractions;
using Tempus.Core.DependencyInjection;
using Tempus.Core.Models;

namespace Tempus.Core.Tests;

public class DstResolverTests
{
    private static IDstResolver BuildResolver()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddTempus();
        return services.BuildServiceProvider().GetRequiredService<IDstResolver>();
    }

    [Fact]
    public void TransitionsInYear_AmericaNewYork_ReturnsTwoTransitions()
    {
        BuildResolver().TransitionsInYear("America/New_York", 2026)
            .Should().HaveCount(2);
    }

    [Fact]
    public void TransitionsInYear_AmericaNewYork_ContainsBothTypes()
    {
        var transitions = BuildResolver().TransitionsInYear("America/New_York", 2026);
        transitions.Should().Contain(t => t.Type == DstTransitionType.SpringForward);
        transitions.Should().Contain(t => t.Type == DstTransitionType.FallBack);
    }

    [Fact]
    public void TransitionsInYear_Utc_ReturnsEmpty()
    {
        BuildResolver().TransitionsInYear("Etc/UTC", 2026)
            .Should().BeEmpty();
    }

    [Fact]
    public void TransitionsInYear_EuropeLondon_ReturnsTwoTransitions()
    {
        BuildResolver().TransitionsInYear("Europe/London", 2026)
            .Should().HaveCount(2);
    }

    [Fact]
    public void TransitionsInYear_AllHaveCorrectTimeZoneId()
    {
        var transitions = BuildResolver().TransitionsInYear("America/New_York", 2026);
        transitions.Should().AllSatisfy(t => t.TimeZoneId.Should().Be("America/New_York"));
    }

    [Fact]
    public void NextTransition_BeforeSpringForward2026_ReturnsSpringForward()
    {
        var before = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var next = BuildResolver().NextTransition("America/New_York", before);
        next.Should().NotBeNull();
        next!.Value.Type.Should().Be(DstTransitionType.SpringForward);
    }

    [Fact]
    public void SpringForward_NewYork2026_OffsetChangesMinusFiveToMinusFour()
    {
        var before = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var next = BuildResolver().NextTransition("America/New_York", before);
        next!.Value.OffsetBefore.Should().Be(TimeSpan.FromHours(-5));
        next!.Value.OffsetAfter.Should().Be(TimeSpan.FromHours(-4));
    }

    [Fact]
    public void NextTransition_UnknownZone_ReturnsNull()
    {
        BuildResolver().NextTransition("Invalid/Zone", DateTimeOffset.UtcNow)
            .Should().BeNull();
    }

    [Fact]
    public void IsNearTransition_OneHourBeforeSpringForward_ReturnsTrue()
    {
        // NY spring 2026: second Sunday in March = Mar 8 at 2:00 AM EST = 07:00 UTC
        var oneHourBefore = new DateTimeOffset(2026, 3, 8, 6, 0, 0, TimeSpan.Zero);
        BuildResolver()
            .IsNearTransition(oneHourBefore, "America/New_York", TimeSpan.FromHours(2))
            .Should().BeTrue();
    }

    [Fact]
    public void IsNearTransition_TwoDaysBeforeSpringForward_ReturnsFalse()
    {
        var twoDaysBefore = new DateTimeOffset(2026, 3, 6, 0, 0, 0, TimeSpan.Zero);
        BuildResolver()
            .IsNearTransition(twoDaysBefore, "America/New_York", TimeSpan.FromHours(24))
            .Should().BeFalse();
    }

    [Fact]
    public void FallBack_NewYork2026_OffsetChangesMinusFourToMinusFive()
    {
        // Fall back: first Sunday in November = Nov 1, 2026
        var before = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var next = BuildResolver().NextTransition("America/New_York", before);
        next.Should().NotBeNull();
        next!.Value.Type.Should().Be(DstTransitionType.FallBack);
        next!.Value.OffsetBefore.Should().Be(TimeSpan.FromHours(-4));
        next!.Value.OffsetAfter.Should().Be(TimeSpan.FromHours(-5));
    }
}
