namespace Tempus.Business.Abstractions;

/// <summary>
/// Defines business-day and business-hour arithmetic for a configured locale, work-week, and holiday set.
/// </summary>
public interface IBusinessCalendar
{
    /// <summary>Returns <c>true</c> if <paramref name="calendarDate"/> is a working day (not a weekend or holiday).</summary>
    bool IsBusinessDay(DateOnly calendarDate);

    /// <summary>Returns <c>true</c> if <paramref name="moment"/> falls within the configured business hours on a business day.</summary>
    bool IsBusinessHour(DateTimeOffset moment);

    /// <summary>Returns the first business day strictly after <paramref name="calendarDate"/>.</summary>
    DateOnly NextBusinessDay(DateOnly calendarDate);

    /// <summary>Returns the last business day strictly before <paramref name="calendarDate"/>.</summary>
    DateOnly PreviousBusinessDay(DateOnly calendarDate);

    /// <summary>Advances <paramref name="calendarDate"/> by <paramref name="days"/> business days (negative values move backwards).</summary>
    DateOnly AddBusinessDays(DateOnly calendarDate, int days);

    /// <summary>Advances <paramref name="moment"/> by <paramref name="hours"/> of business time, skipping non-business periods.</summary>
    DateTimeOffset AddBusinessHours(DateTimeOffset moment, double hours);

    /// <summary>Counts the number of business days in the range [<paramref name="startDate"/>, <paramref name="endDate"/>].</summary>
    int BusinessDaysBetween(DateOnly startDate, DateOnly endDate);

    /// <summary>Returns the total business time elapsed between two moments, skipping weekends, holidays, and off-hours.</summary>
    TimeSpan BusinessTimeBetween(DateTimeOffset startDate, DateTimeOffset endDate);
}
