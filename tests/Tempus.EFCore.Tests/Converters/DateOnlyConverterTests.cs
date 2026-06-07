using Tempus.EFCore.Converters;

namespace Tempus.EFCore.Tests.Converters;

public class DateOnlyConverterTests
{
    private readonly DateOnlyConverter _converter = new();

    [Fact]
    public void ToProvider_ProducesDateTimeWithMidnightUtc()
    {
        var date = new DateOnly(2026, 5, 27);
        var result = (DateTime)_converter.ConvertToProvider!(date)!;
        result.Should().Be(new DateTime(2026, 5, 27, 0, 0, 0, DateTimeKind.Utc));
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void FromProvider_ReconstructsOriginalDateOnly()
    {
        var stored = new DateTime(2026, 5, 27, 0, 0, 0, DateTimeKind.Utc);
        var result = (DateOnly)_converter.ConvertFromProvider!(stored)!;
        result.Should().Be(new DateOnly(2026, 5, 27));
    }

    [Theory]
    [InlineData(2000, 1, 1)]
    [InlineData(2026, 5, 27)]
    [InlineData(2099, 12, 31)]
    public void RoundTrip_IsLossless(int year, int month, int day)
    {
        var original = new DateOnly(year, month, day);
        var stored = (DateTime)_converter.ConvertToProvider!(original)!;
        var restored = (DateOnly)_converter.ConvertFromProvider!(stored)!;
        restored.Should().Be(original);
    }
}
