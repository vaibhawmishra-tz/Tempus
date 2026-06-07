using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.DependencyInjection;
using Tempus.Business;
using Tempus.Business.Abstractions;
using Tempus.Business.Models;
using Tempus.Holidays;
using Tempus.Holidays.US;

namespace Tempus.Business.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by BenchmarkDotNet")]
[SuppressMessage("Design", "CA1852:Seal internal types", Justification = "BenchmarkDotNet requires non-sealed benchmark classes")]
internal class BusinessCalendarBenchmarks
{
    private IBusinessCalendar _calPlain = null!;
    private IBusinessCalendar _calWithUsHolidays = null!;
    private IBusinessCalendar _calWithCaHolidays = null!;

    private static readonly DateOnly BaseDate = new(2026, 1, 2);   // Friday

    [GlobalSetup]
    public void Setup()
    {
        var opts = new BusinessCalendarOptions
        {
            BusinessHours = BusinessHours.NineToFive("America/New_York"),
        };

        _calPlain = new BusinessCalendar(opts);
        _calWithUsHolidays = new BusinessCalendar(opts, [new UsHolidayProvider()]);

        var services = new ServiceCollection();
        services.AddHolidays("CA");
        var sp  = services.BuildServiceProvider();
        var caProvider = sp.GetServices<IHolidayProvider>().First();
        _calWithCaHolidays = new BusinessCalendar(opts, [caProvider]);
    }

    [Benchmark(Baseline = true)]
    public bool IsBusinessDay_NoHolidays()
        => _calPlain.IsBusinessDay(BaseDate);

    [Benchmark]
    public bool IsBusinessDay_WithUsHolidays()
        => _calWithUsHolidays.IsBusinessDay(BaseDate);

    [Benchmark]
    public bool IsBusinessDay_WithCaHolidays_EmbeddedJson()
        => _calWithCaHolidays.IsBusinessDay(BaseDate);

    [Benchmark]
    public DateOnly AddBusinessDays_5()
        => _calPlain.AddBusinessDays(BaseDate, 5);

    [Benchmark]
    public DateOnly AddBusinessDays_20_WithHolidays()
        => _calWithUsHolidays.AddBusinessDays(BaseDate, 20);

    [Benchmark]
    public TimeSpan BusinessTimeBetween()
    {
        var start = new DateTimeOffset(2026, 1, 2, 9, 0, 0, TimeSpan.Zero);
        var end   = new DateTimeOffset(2026, 1, 9, 17, 0, 0, TimeSpan.Zero);
        return _calPlain.BusinessTimeBetween(start, end);
    }
}
