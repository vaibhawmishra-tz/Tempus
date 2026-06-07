namespace Tempus.Business.Calendar;

public enum FiscalYearNaming
{
    /// <summary>Fiscal year is named after the calendar year in which it starts.</summary>
    CalendarYear,

    /// <summary>Fiscal year is named after the calendar year in which it ends (common for Apr–Mar cycles).</summary>
    EndYear,

    /// <summary>Explicit alias for CalendarYear; fiscal year is named after the calendar year it starts in.</summary>
    StartYear
}
