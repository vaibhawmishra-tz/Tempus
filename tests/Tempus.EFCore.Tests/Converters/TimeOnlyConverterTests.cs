using Tempus.EFCore.Converters;

namespace Tempus.EFCore.Tests.Converters;

public class TimeOnlyConverterTests
{
    private readonly TimeOnlyConverter _converter = new();

    [Fact]
    public void ToProvider_ProducesEquivalentTimeSpan()
    {
        var time = new TimeOnly(14, 30, 45);
        var result = (TimeSpan)_converter.ConvertToProvider!(time)!;
        result.Should().Be(new TimeSpan(14, 30, 45));
    }

    [Fact]
    public void FromProvider_ReconstructsOriginalTimeOnly()
    {
        var stored = new TimeSpan(14, 30, 45);
        var result = (TimeOnly)_converter.ConvertFromProvider!(stored)!;
        result.Should().Be(new TimeOnly(14, 30, 45));
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(14, 30, 45)]
    [InlineData(23, 59, 59)]
    public void RoundTrip_IsLossless(int hour, int minute, int second)
    {
        var original = new TimeOnly(hour, minute, second);
        var stored = (TimeSpan)_converter.ConvertToProvider!(original)!;
        var restored = (TimeOnly)_converter.ConvertFromProvider!(stored)!;
        restored.Should().Be(original);
    }

    [Fact]
    public void RoundTrip_PreservesMilliseconds()
    {
        var original = new TimeOnly(9, 0, 0, 500);
        var stored = (TimeSpan)_converter.ConvertToProvider!(original)!;
        var restored = (TimeOnly)_converter.ConvertFromProvider!(stored)!;
        restored.Should().Be(original);
    }
}
