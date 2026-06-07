using Tempus.Business.Models;

namespace Tempus.Business;

public sealed class BusinessCalendarOptions
{
    public IReadOnlyList<DayOfWeek> WeekendDays { get; set; } = [DayOfWeek.Saturday, DayOfWeek.Sunday];
    public BusinessHours BusinessHours { get; set; } = BusinessHours.NineToFive();

    /// <summary>
    /// Maximum iterations when searching for the next/previous business day.
    /// Guards against infinite loops caused by holiday providers that mark every day as a holiday.
    /// </summary>
    public int MaxIterations { get; set; } = 1000;
}
