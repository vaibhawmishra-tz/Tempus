using Tempus.Business.Models;

namespace Tempus.Business.Abstractions;

/// <summary>
/// Provides holiday data for a given year and region.
/// Implement this to plug in custom, database-driven, or third-party holiday sources.
/// </summary>
public interface IHolidayProvider
{
    /// <summary>The ISO 3166-1 alpha-2 country code or region code this provider covers (e.g. "US", "CA-ON").</summary>
    string Region { get; }

    /// <summary>Returns all public holidays in the given calendar year for this region.</summary>
    IEnumerable<Holiday> GetHolidays(int year);

    /// <summary>Returns <c>true</c> if <paramref name="calendarDate"/> is a public holiday in this region.</summary>
    bool IsHoliday(DateOnly calendarDate);
}
