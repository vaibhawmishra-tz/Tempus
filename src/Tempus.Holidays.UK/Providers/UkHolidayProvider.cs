using System.Collections.Concurrent;
using Tempus.Business.Abstractions;
using Tempus.Business.Models;

namespace Tempus.Holidays.UK;

/// <summary>
/// England and Wales public holiday provider. Includes Easter-based bank holidays
/// and the standard Christmas/Boxing Day substitution rules.
/// </summary>
public sealed class UkHolidayProvider : IHolidayProvider
{
    private readonly ConcurrentDictionary<int, HashSet<DateOnly>> _cache = new();

    public string Region => "UK";

    public IEnumerable<Holiday> GetHolidays(int year)
    {
        yield return BankHoliday("New Year's Day",       SubstituteWeekend(new DateOnly(year, 1, 1)));

        var easterSunday = Easter(year);
        yield return BankHoliday("Good Friday",          easterSunday.AddDays(-2));
        yield return BankHoliday("Easter Monday",        easterSunday.AddDays(1));

        yield return BankHoliday("Early May Bank Holiday", NthWeekday(year, 5, DayOfWeek.Monday, 1));
        yield return BankHoliday("Spring Bank Holiday",    LastWeekday(year, 5, DayOfWeek.Monday));
        yield return BankHoliday("Summer Bank Holiday",    LastWeekday(year, 8, DayOfWeek.Monday));

        foreach (var h in ChristmasHolidays(year))
            yield return h;
    }

    public bool IsHoliday(DateOnly calendarDate)
    {
        var set = _cache.GetOrAdd(calendarDate.Year, y => [..GetHolidays(y).Select(h => h.Date)]);
        return set.Contains(calendarDate);
    }

    private static Holiday BankHoliday(string name, DateOnly date)
        => new() { Name = name, Date = date, IsNational = true };

    // UK Christmas/Boxing Day substitution: both holidays are always on weekdays.
    private static IEnumerable<Holiday> ChristmasHolidays(int year)
    {
        var dec25 = new DateOnly(year, 12, 25);
        return dec25.DayOfWeek switch
        {
            // Sat: Christmas→Mon 27, Boxing→Tue 28
            DayOfWeek.Saturday => [
                new Holiday { Name = "Christmas Day (substitute)",  Date = dec25.AddDays(2), IsNational = true },
                new Holiday { Name = "Boxing Day (substitute)",     Date = dec25.AddDays(3), IsNational = true }
            ],
            // Sun: Christmas→Mon 26, Boxing→Tue 27
            DayOfWeek.Sunday => [
                new Holiday { Name = "Christmas Day (substitute)",  Date = dec25.AddDays(1), IsNational = true },
                new Holiday { Name = "Boxing Day (substitute)",     Date = dec25.AddDays(2), IsNational = true }
            ],
            // Fri: Christmas→Fri 25, Boxing Day Sat→Mon 28
            DayOfWeek.Friday => [
                new Holiday { Name = "Christmas Day",              Date = dec25,             IsNational = true },
                new Holiday { Name = "Boxing Day (substitute)",    Date = dec25.AddDays(3),  IsNational = true }
            ],
            // Any other weekday: both on actual date
            _ => [
                new Holiday { Name = "Christmas Day", Date = dec25,            IsNational = true },
                new Holiday { Name = "Boxing Day",    Date = dec25.AddDays(1), IsNational = true }
            ]
        };
    }

    private static DateOnly SubstituteWeekend(DateOnly date) => date.DayOfWeek switch
    {
        DayOfWeek.Saturday => date.AddDays(2),
        DayOfWeek.Sunday   => date.AddDays(1),
        _                  => date
    };

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
