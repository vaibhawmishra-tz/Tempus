using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Tempus.Business.Abstractions;
using Tempus.Business.Models;

namespace Tempus.Holidays.Internal;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by HolidaysServiceCollectionExtensions.LoadRegistry")]
internal sealed class EmbeddedHolidayProvider : IHolidayProvider
{
    private readonly HolidayDataFile _data;
    private readonly ConcurrentDictionary<int, HashSet<DateOnly>> _cache = new();

    internal EmbeddedHolidayProvider(HolidayDataFile data) => _data = data;

    public string Region => _data.Region;

    public IEnumerable<Holiday> GetHolidays(int year)
        => HolidayRuleEngine.Compute(_data, year);

    public bool IsHoliday(DateOnly calendarDate)
    {
        HashSet<DateOnly> set = _cache.GetOrAdd(
            calendarDate.Year,
            y => [..GetHolidays(y).Select(h => h.Date)]);
        return set.Contains(calendarDate);
    }
}
