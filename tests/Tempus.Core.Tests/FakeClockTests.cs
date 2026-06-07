using Tempus.Core.Extensions;
using Tempus.Testing.Clocks;

namespace Tempus.Core.Tests;

public class FakeClockTests
{
    [Fact]
    public void UtcNow_ReturnsFixedTime()
    {
        var fixed_ = new DateTimeOffset(2026, 6, 15, 9, 0, 0, TimeSpan.Zero);
        var clock = new FakeClock(fixed_);
        clock.UtcNow.Should().Be(fixed_);
    }

    [Fact]
    public void Advance_MovesTimeForward()
    {
        var start = new DateTimeOffset(2026, 6, 15, 9, 0, 0, TimeSpan.Zero);
        var clock = new FakeClock(start);
        clock.Advance(TimeSpan.FromHours(2));
        clock.UtcNow.Should().Be(start.AddHours(2));
    }

    [Fact]
    public void SetTo_SetsExactTime()
    {
        var clock = new FakeClock(DateTimeOffset.UtcNow);
        var target = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);
        clock.SetTo(target);
        clock.UtcNow.Should().Be(target);
    }

    // StartOfDayUtc extension

    [Fact]
    public void StartOfDayUtc_ReturnsUtcMidnightForDate()
    {
        var clock    = new FakeClock(new DateTimeOffset(2026, 5, 27, 14, 30, 0, TimeSpan.Zero));
        var midnight = clock.StartOfDayUtc(new DateOnly(2026, 5, 27));
        midnight.Should().Be(new DateTimeOffset(2026, 5, 27, 0, 0, 0, TimeSpan.Zero));
        midnight.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void StartOfDayUtc_IgnoresClocksCurrentTime()
    {
        var clock = new FakeClock(new DateTimeOffset(2026, 12, 31, 23, 59, 59, TimeSpan.Zero));
        clock.StartOfDayUtc(new DateOnly(2025, 1, 1))
            .Should().Be(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }
}
