using System.Collections.Concurrent;
using Tempus.Business.Abstractions;
using Tempus.Business.Models;

namespace Tempus.Holidays.EU;

/// <summary>
/// Common European public holiday provider. Covers holidays observed across most EU member states.
/// Country-specific observances (e.g. Bastille Day in France, German Unity Day) are not included —
/// add a country-specific provider alongside this one for full coverage.
/// </summary>
public sealed class EuHolidayProvider : IHolidayProvider
{
    private readonly ConcurrentDictionary<int, HashSet<DateOnly>> _cache = new();

    public string Region => "EU";

    public IEnumerable<Holiday> GetHolidays(int year)
    {
        yield return Public("New Year's Day",    new DateOnly(year, 1, 1));

        var easterSunday = Easter(year);
        yield return Public("Good Friday",       easterSunday.AddDays(-2));
        yield return Public("Easter Monday",     easterSunday.AddDays(1));

        yield return Public("Labour Day",        new DateOnly(year, 5, 1));

        // Ascension is 39 days after Easter (Thu); included in many EU countries
        yield return Public("Ascension Day",     easterSunday.AddDays(39));

        // Whit Monday / Pentecost Monday: 50 days after Easter
        yield return Public("Whit Monday",       easterSunday.AddDays(50));

        yield return Public("Christmas Day",     new DateOnly(year, 12, 25));
        yield return Public("St. Stephen's Day", new DateOnly(year, 12, 26));
    }

    public bool IsHoliday(DateOnly calendarDate)
    {
        var set = _cache.GetOrAdd(calendarDate.Year, y => [..GetHolidays(y).Select(h => h.Date)]);
        return set.Contains(calendarDate);
    }

    private static Holiday Public(string name, DateOnly date)
        => new() { Name = name, Date = date, IsNational = true, Region = "EU" };

    // Anonymous Gregorian algorithm for Easter Sunday
    private static DateOnly Easter(int year)
    {
        int a = year % 19;
        int b = year / 100, c = year % 100;
        int d = b / 4,      e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4,      k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int month = (h + l - 7 * m + 114) / 31;
        int day   = (h + l - 7 * m + 114) % 31 + 1;
        return new DateOnly(year, month, day);
    }
}
