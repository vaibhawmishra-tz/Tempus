using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Tempus.Business.Calendar;

namespace Tempus.Business.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by BenchmarkDotNet")]
[SuppressMessage("Design", "CA1852:Seal internal types", Justification = "BenchmarkDotNet requires non-sealed benchmark classes")]
internal class FiscalCalendarBenchmarks
{
    private FiscalCalendar _calJan = null!;
    private FiscalCalendar _calApr = null!;

    private static readonly DateOnly TestDate = new(2026, 11, 15);

    [GlobalSetup]
    public void Setup()
    {
        _calJan = new FiscalCalendar(1, 1);
        _calApr = new FiscalCalendar(4, 1);
    }

    [Benchmark(Baseline = true)]
    public int GetFiscalYear_JanStart()
        => _calJan.GetFiscalYear(TestDate);

    [Benchmark]
    public int GetFiscalYear_AprStart()
        => _calApr.GetFiscalYear(TestDate);

    [Benchmark]
    public int GetFiscalQuarter_JanStart()
        => _calJan.GetFiscalQuarter(TestDate);

    [Benchmark]
    public int GetFiscalQuarter_AprStart()
        => _calApr.GetFiscalQuarter(TestDate);

    [Benchmark]
    public DateOnly GetQuarterStart()
        => _calApr.GetQuarterStart(2026, 3);

    [Benchmark]
    public DateOnly GetFiscalYearStart()
        => _calApr.GetFiscalYearStart(2026);
}
