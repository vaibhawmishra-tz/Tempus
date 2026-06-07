using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.DependencyInjection;
using Tempus.Business.Abstractions;
using Tempus.Holidays;
using Tempus.Holidays.US;

namespace Tempus.Business.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by BenchmarkDotNet")]
[SuppressMessage("Design", "CA1852:Seal internal types", Justification = "BenchmarkDotNet requires non-sealed benchmark classes")]
internal class HolidayProviderBenchmarks
{
    private IHolidayProvider _usLegacy = null!;
    private IHolidayProvider _caEmbedded = null!;
    private IHolidayProvider _jpEmbedded = null!;

    private static readonly DateOnly CacheHitDate   = new(2026, 1, 1);  // already computed year
    private static readonly DateOnly CacheMissYear1 = new(2031, 1, 1);  // cold year
    private static readonly DateOnly CacheMissYear2 = new(2032, 1, 1);  // cold year

    [GlobalSetup]
    public void Setup()
    {
        _usLegacy = new UsHolidayProvider();

        var services = new ServiceCollection();
        services.AddHolidays("CA", "JP");
        var sp = services.BuildServiceProvider();
        var providers = sp.GetServices<IHolidayProvider>().ToList();
        _caEmbedded = providers.First(p => string.Equals(p.Region, "CA", StringComparison.Ordinal));
        _jpEmbedded = providers.First(p => string.Equals(p.Region, "JP", StringComparison.Ordinal));

        // Warm cache for 2026
        _ = _usLegacy.IsHoliday(CacheHitDate);
        _ = _caEmbedded.IsHoliday(CacheHitDate);
        _ = _jpEmbedded.IsHoliday(CacheHitDate);
    }

    [Benchmark(Baseline = true)]
    public bool US_Legacy_CacheHit()
        => _usLegacy.IsHoliday(CacheHitDate);

    [Benchmark]
    public bool CA_Embedded_CacheHit()
        => _caEmbedded.IsHoliday(CacheHitDate);

    [Benchmark]
    public bool JP_Embedded_CacheHit()
        => _jpEmbedded.IsHoliday(CacheHitDate);

    [Benchmark]
    public bool CA_Embedded_CacheMiss()
        => _caEmbedded.IsHoliday(CacheMissYear1);

    [Benchmark]
    public bool JP_Embedded_CacheMiss_WithSubstitutes()
        => _jpEmbedded.IsHoliday(CacheMissYear2);
}
