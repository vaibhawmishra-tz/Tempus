namespace Tempus.Business.Calendar;

public sealed class FiscalCalendarOptions
{
    public int StartMonth { get; set; } = 1;
    public int StartDay { get; set; } = 1;
    public FiscalYearNaming Naming { get; set; } = FiscalYearNaming.CalendarYear;
}
