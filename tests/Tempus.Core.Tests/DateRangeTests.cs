using Tempus.Core.Models;

namespace Tempus.Core.Tests;

public class DateRangeTests
{
    [Fact]
    public void Contains_DateInsideRange_ReturnsTrue()
    {
        var range = new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        range.Contains(new DateOnly(2026, 1, 15)).Should().BeTrue();
    }

    [Fact]
    public void Contains_DateOutsideRange_ReturnsFalse()
    {
        var range = new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        range.Contains(new DateOnly(2026, 2, 1)).Should().BeFalse();
    }

    [Fact]
    public void Overlaps_OverlappingRanges_ReturnsTrue()
    {
        var a = new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 15));
        var b = new DateRange(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 20));
        a.Overlaps(b).Should().BeTrue();
    }

    [Fact]
    public void Overlaps_NonOverlappingRanges_ReturnsFalse()
    {
        var a = new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 10));
        var b = new DateRange(new DateOnly(2026, 1, 11), new DateOnly(2026, 1, 20));
        a.Overlaps(b).Should().BeFalse();
    }

    [Fact]
    public void Intersect_OverlappingRanges_ReturnsIntersection()
    {
        var a = new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 15));
        var b = new DateRange(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 20));
        var result = a.Intersect(b);
        result.Should().NotBeNull();
        result!.Value.Start.Should().Be(new DateOnly(2026, 1, 10));
        result!.Value.End.Should().Be(new DateOnly(2026, 1, 15));
    }

    [Fact]
    public void TotalDays_FullJanuary_Returns31()
    {
        var range = new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        range.TotalDays.Should().Be(31);
    }

    [Fact]
    public void ToDates_SmallRange_YieldsAllDates()
    {
        var range = new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 3));
        range.ToDates().Should().HaveCount(3);
    }
}
