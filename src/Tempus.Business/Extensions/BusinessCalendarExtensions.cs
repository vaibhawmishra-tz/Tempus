using Tempus.Business.Abstractions;
using Tempus.Core.Models;

namespace Tempus.Business.Extensions;

public static class BusinessCalendarExtensions
{
    /// <summary>
    /// Returns the number of business days in the given date range (start inclusive, end exclusive).
    /// Convenience alias for <see cref="IBusinessCalendar.BusinessDaysBetween"/>.
    /// </summary>
    public static int GetBusinessDays(this IBusinessCalendar calendar, DateRange range)
    {
        ArgumentNullException.ThrowIfNull(calendar);
        return calendar.BusinessDaysBetween(range.Start, range.End);
    }

    /// <summary>
    /// Returns the number of business days between <paramref name="from"/> (inclusive) and
    /// <paramref name="to"/> (exclusive). Convenience alias for
    /// <see cref="IBusinessCalendar.BusinessDaysBetween"/>.
    /// </summary>
    public static int GetBusinessDays(this IBusinessCalendar calendar, DateOnly from, DateOnly to)
    {
        ArgumentNullException.ThrowIfNull(calendar);
        return calendar.BusinessDaysBetween(from, to);
    }
}
