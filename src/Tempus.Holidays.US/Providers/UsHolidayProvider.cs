using System.Collections.Concurrent;
using Tempus.Business.Abstractions;
using Tempus.Business.Models;

namespace Tempus.Holidays.US;

/// <summary>
/// US Federal holiday provider. Includes all 11 federal public holidays with
/// Saturday→Friday / Sunday→Monday observed-day substitution rules.
/// Juneteenth is included for years 2021 and later.
/// </summary>
public sealed class UsHolidayProvider : IHolidayProvider
{
    private readonly ConcurrentDictionary<int, HashSet<DateOnly>> _cache = new();

    public string Region => "US";

    public IEnumerable<Holiday> GetHolidays(int year)
    {
        yield return Fixed("New Year's Day",                           year, 1,  1);
        yield return Floating("Martin Luther King Jr. Day",            NthWeekday(year, 1,  DayOfWeek.Monday, 3));
        yield return Floating("Presidents' Day",                       NthWeekday(year, 2,  DayOfWeek.Monday, 3));
        yield return Floating("Memorial Day",                          LastWeekday(year, 5, DayOfWeek.Monday));
        if (year >= 2021)
            yield return Fixed("Juneteenth National Independence Day", year, 6, 19);
        yield return Fixed("Independence Day",                         year, 7,  4);
        yield return Floating("Labor Day",                             NthWeekday(year, 9,  DayOfWeek.Monday, 1));
        yield return Floating("Columbus Day",                          NthWeekday(year, 10, DayOfWeek.Monday, 2));
        yield return Fixed("Veterans Day",                             year, 11, 11);
        yield return Floating("Thanksgiving Day",                      NthWeekday(year, 11, DayOfWeek.Thursday, 4));
        yield return Fixed("Christmas Day",                            year, 12, 25);
    }

    public bool IsHoliday(DateOnly calendarDate)
    {
        var set = _cache.GetOrAdd(calendarDate.Year, y => [..GetHolidays(y).Select(h => h.Date)]);
        return set.Contains(calendarDate);
    }

    private static Holiday Fixed(string name, int year, int month, int day)
        => new() { Name = name, Date = Observed(new DateOnly(year, month, day)), IsNational = true };

    private static Holiday Floating(string name, DateOnly date)
        => new() { Name = name, Date = date, IsNational = true };

    private static DateOnly Observed(DateOnly date) => date.DayOfWeek switch
    {
        DayOfWeek.Saturday => date.AddDays(-1),
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
}
