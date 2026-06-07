using System.Collections.Concurrent;
using Tempus.Business.Abstractions;
using Tempus.Business.Models;

namespace Tempus.Holidays.Exchange;

/// <summary>
/// NYSE trading holiday provider. The exchange is closed on 10 scheduled holidays.
/// Note: ad-hoc market closures (national mourning days, weather events) are not included
/// and must be added manually via a custom provider.
/// </summary>
public sealed class NyseHolidayProvider : IHolidayProvider
{
    private readonly ConcurrentDictionary<int, HashSet<DateOnly>> _cache = new();

    public string Region => "NYSE";

    public IEnumerable<Holiday> GetHolidays(int year)
    {
        yield return Observed("New Year's Day",              year, 1,  1);
        yield return Floating("Martin Luther King Jr. Day",  NthWeekday(year, 1,  DayOfWeek.Monday, 3));
        yield return Floating("Presidents' Day",             NthWeekday(year, 2,  DayOfWeek.Monday, 3));

        var easterSunday = Easter(year);
        yield return new Holiday { Name = "Good Friday", Date = easterSunday.AddDays(-2), IsNational = true };

        yield return Floating("Memorial Day",                LastWeekday(year, 5, DayOfWeek.Monday));
        if (year >= 2021)
            yield return Observed("Juneteenth",              year, 6, 19);
        yield return Observed("Independence Day",            year, 7,  4);
        yield return Floating("Labor Day",                   NthWeekday(year, 9,  DayOfWeek.Monday, 1));
        yield return Floating("Thanksgiving Day",            NthWeekday(year, 11, DayOfWeek.Thursday, 4));
        yield return Observed("Christmas Day",               year, 12, 25);
    }

    public bool IsHoliday(DateOnly calendarDate)
    {
        var set = _cache.GetOrAdd(calendarDate.Year, y => [..GetHolidays(y).Select(h => h.Date)]);
        return set.Contains(calendarDate);
    }

    private static Holiday Observed(string name, int year, int month, int day)
    {
        var date = new DateOnly(year, month, day);
        var observed = date.DayOfWeek switch
        {
            DayOfWeek.Saturday => date.AddDays(-1),
            DayOfWeek.Sunday   => date.AddDays(1),
            _                  => date
        };
        return new Holiday { Name = name, Date = observed, IsNational = true };
    }

    private static Holiday Floating(string name, DateOnly date)
        => new() { Name = name, Date = date, IsNational = true };

    private static DateOnly NthWeekday(int year, int month, DayOfWeek dow, int n)
    {
        var first = new DateOnly(year, month, 1);
        int skip = ((int)dow - (int)first.DayOfWeek + 7) % 7;
        return first.AddDays(skip + (n - 1) * 7);
    }

    private static DateOnly LastWeekday(int year, int month, DayOfWeek dow)
    {
        var last = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
        int back = ((int)last.DayOfWeek - (int)dow + 7) % 7;
        return last.AddDays(-back);
    }

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
