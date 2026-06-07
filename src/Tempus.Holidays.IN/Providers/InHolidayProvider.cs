using System.Collections.Concurrent;
using Tempus.Business.Abstractions;
using Tempus.Business.Models;

namespace Tempus.Holidays.India;

/// <summary>
/// India national public holiday provider. Covers the three Gazette-notified national holidays
/// (Republic Day, Independence Day, Gandhi Jayanti) and Christmas.
/// Regional / state holidays and lunar-calendar observances (Holi, Diwali, Eid, etc.)
/// require a separate provider backed by a Hijri/Hindu calendar library.
/// </summary>
public sealed class InHolidayProvider : IHolidayProvider
{
    private readonly ConcurrentDictionary<int, HashSet<DateOnly>> _cache = new();

    public string Region => "IN";

    public IEnumerable<Holiday> GetHolidays(int year)
    {
        yield return National("New Year's Day",   year, 1,  1);
        yield return National("Republic Day",     year, 1, 26);
        yield return National("Ambedkar Jayanti", year, 4, 14);
        yield return National("Independence Day", year, 8, 15);
        yield return National("Gandhi Jayanti",   year, 10, 2);
        yield return National("Christmas Day",    year, 12, 25);
    }

    public bool IsHoliday(DateOnly calendarDate)
    {
        var set = _cache.GetOrAdd(calendarDate.Year, y => [..GetHolidays(y).Select(h => h.Date)]);
        return set.Contains(calendarDate);
    }

    private static Holiday National(string name, int year, int month, int day)
        => new() { Name = name, Date = new DateOnly(year, month, day), IsNational = true, Region = "IN" };
}
