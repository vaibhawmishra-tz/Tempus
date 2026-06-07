namespace Tempus.Business.Calendar;

public sealed class FiscalCalendar
{
    private readonly FiscalCalendarOptions _options;

    public FiscalCalendar(FiscalCalendarOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.StartMonth, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(options.StartMonth, 12);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.StartDay, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(options.StartDay, 28);
        _options = options;
    }

    /// <summary>
    /// Short constructor for the common case where only the start month and day are needed.
    /// Defaults to <see cref="FiscalYearNaming.CalendarYear"/> naming.
    /// </summary>
    public FiscalCalendar(int startMonth, int startDay)
        : this(new FiscalCalendarOptions { StartMonth = startMonth, StartDay = startDay }) { }

    /// <summary>Returns the fiscal year number that contains the given date.</summary>
    public int GetFiscalYear(DateOnly date)
    {
        var fyStartThisCalYear = new DateOnly(date.Year, _options.StartMonth, _options.StartDay);
        int calStartYear = date >= fyStartThisCalYear ? date.Year : date.Year - 1;
        return ToFiscalYear(calStartYear);
    }

    /// <summary>Returns the first calendar date of the given fiscal year.</summary>
    public DateOnly GetFiscalYearStart(int fiscalYear)
    {
        int calStartYear = FromFiscalYear(fiscalYear);
        return new DateOnly(calStartYear, _options.StartMonth, _options.StartDay);
    }

    /// <summary>Returns the last calendar date of the given fiscal year.</summary>
    public DateOnly GetFiscalYearEnd(int fiscalYear)
        => GetFiscalYearStart(fiscalYear + 1).AddDays(-1);

    /// <summary>Returns the fiscal quarter (1–4) that contains the given date.</summary>
    public int GetFiscalQuarter(DateOnly date)
        => (FiscalMonthOffset(date) / 3) + 1;

    /// <summary>Returns the first calendar date of the given fiscal quarter.</summary>
    public DateOnly GetQuarterStart(int fiscalYear, int quarter)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quarter, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(quarter, 4);

        int calStartYear = FromFiscalYear(fiscalYear);
        int totalMonth = _options.StartMonth + (quarter - 1) * 3;
        int year = calStartYear + (totalMonth - 1) / 12;
        int month = ((totalMonth - 1) % 12) + 1;
        int day = Math.Min(_options.StartDay, DateTime.DaysInMonth(year, month));
        return new DateOnly(year, month, day);
    }

    /// <summary>Returns the last calendar date of the given fiscal quarter.</summary>
    public DateOnly GetQuarterEnd(int fiscalYear, int quarter)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quarter, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(quarter, 4);

        return quarter < 4
            ? GetQuarterStart(fiscalYear, quarter + 1).AddDays(-1)
            : GetFiscalYearEnd(fiscalYear);
    }

    /// <summary>Returns true if both dates fall within the same fiscal year.</summary>
    public bool IsSameFiscalYear(DateOnly a, DateOnly b)
        => GetFiscalYear(a) == GetFiscalYear(b);

    // Returns 0-based month offset from the start of the fiscal year (0 = first fiscal month).
    private int FiscalMonthOffset(DateOnly date)
    {
        int fiscalYear = GetFiscalYear(date);
        var fyStart = GetFiscalYearStart(fiscalYear);
        int months = (date.Year - fyStart.Year) * 12 + (date.Month - fyStart.Month);
        if (date.Day < fyStart.Day) months--;
        return Math.Max(0, months);
    }

    private int ToFiscalYear(int calStartYear)
        => _options.Naming == FiscalYearNaming.EndYear && !IsCalendarAligned()
            ? calStartYear + 1
            : calStartYear;

    private int FromFiscalYear(int fiscalYear)
        => _options.Naming == FiscalYearNaming.EndYear && !IsCalendarAligned()
            ? fiscalYear - 1
            : fiscalYear;

    private bool IsCalendarAligned()
        => _options.StartMonth == 1 && _options.StartDay == 1;
}
