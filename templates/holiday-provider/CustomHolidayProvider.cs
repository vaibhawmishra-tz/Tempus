using System.Collections.Concurrent;
using Tempus.Business.Abstractions;
using Tempus.Business.Models;

namespace MyApp;

/// <summary>
/// Custom holiday provider. Register with:
///   services.AddSingleton&lt;IHolidayProvider, CustomHolidayProvider&gt;();
/// </summary>
public sealed class CustomHolidayProvider : IHolidayProvider
{
    private readonly ConcurrentDictionary<int, HashSet<DateOnly>> _cache = new();

    public string Region => "CUSTOM";

    public IEnumerable<Holiday> GetHolidays(int year)
    {
        return
        [
            Make("New Year's Day", Fixed(year, 1,  1)),
            Make("My Holiday",     Fixed(year, 6, 15)),
            // Add more holidays here. Helper methods are below.
        ];
    }

    public bool IsHoliday(DateOnly date)
    {
        HashSet<DateOnly> set = _cache.GetOrAdd(
            date.Year,
            y => [..GetHolidays(y).Select(h => h.Date)]);
        return set.Contains(date);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static DateOnly Fixed(int year, int month, int day) => new(year, month, day);

    // First/Nth occurrence of a weekday in a month (n is 1-based).
    private static DateOnly NthWeekday(int year, int month, DayOfWeek dow, int n)
    {
        var first = new DateOnly(year, month, 1);
        int skip = ((int)dow - (int)first.DayOfWeek + 7) % 7;
        return first.AddDays(skip + (n - 1) * 7);
    }

    // Last occurrence of a weekday in a month.
    private static DateOnly LastWeekday(int year, int month, DayOfWeek dow)
    {
        var last = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
        int back = ((int)last.DayOfWeek - (int)dow + 7) % 7;
        return last.AddDays(-back);
    }

    // Observed date: Sat → Mon, Sun → Mon.
    private static DateOnly ObserveSatMonSunMon(DateOnly date) => date.DayOfWeek switch
    {
        DayOfWeek.Saturday or DayOfWeek.Sunday => NextMonday(date),
        _ => date
    };

    private static DateOnly NextMonday(DateOnly date)
    {
        int days = ((int)DayOfWeek.Monday - (int)date.DayOfWeek + 7) % 7;
        return date.AddDays(days == 0 ? 7 : days);
    }

    private Holiday Make(string name, DateOnly date)
        => new() { Name = name, Date = date, Region = Region, IsNational = true };
}
