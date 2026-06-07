using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Tempus.Core.Extensions;

namespace Tempus.Core.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by BenchmarkDotNet")]
[SuppressMessage("Design", "CA1852:Seal internal types", Justification = "BenchmarkDotNet requires non-sealed benchmark classes")]
internal class TimezoneBenchmarks
{
    private readonly DateTimeOffset _utcNow = DateTimeOffset.UtcNow;

    [Benchmark(Baseline = true)]
    public DateTimeOffset NativeConversion()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        return TimeZoneInfo.ConvertTime(_utcNow, tz);
    }

    [Benchmark]
    public DateTimeOffset TempusToZone()
        => _utcNow.ToZone("America/New_York");

    [Benchmark]
    public bool IsWeekend()
        => _utcNow.IsWeekend();

    [Benchmark]
    public DateTimeOffset StartOfDay()
        => _utcNow.StartOfDay();

    [Benchmark]
    public DateTimeOffset EndOfMonth()
        => _utcNow.EndOfMonth();

    [Benchmark]
    public string ToRelativeString()
        => _utcNow.AddHours(-3).ToRelativeString();
}
