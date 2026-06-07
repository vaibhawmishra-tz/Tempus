using Tempus.Business.Calendar;

namespace Tempus.Business.Tests;

public class FiscalCalendarTests
{
    private static FiscalCalendar JanFiscal()
        => new(new FiscalCalendarOptions());

    private static FiscalCalendar AprFiscal(FiscalYearNaming naming = FiscalYearNaming.CalendarYear)
        => new(new FiscalCalendarOptions { StartMonth = 4, StartDay = 1, Naming = naming });

    // GetFiscalYear

    [Fact]
    public void GetFiscalYear_JanStart_ReturnsCalendarYear()
    {
        JanFiscal().GetFiscalYear(new DateOnly(2026, 6, 15)).Should().Be(2026);
    }

    [Fact]
    public void GetFiscalYear_AprStart_BeforeApril_ReturnsPreviousYear()
    {
        AprFiscal().GetFiscalYear(new DateOnly(2026, 3, 31)).Should().Be(2025);
    }

    [Fact]
    public void GetFiscalYear_AprStart_OnAprilFirst_ReturnsCurrentYear()
    {
        AprFiscal().GetFiscalYear(new DateOnly(2026, 4, 1)).Should().Be(2026);
    }

    [Fact]
    public void GetFiscalYear_AprStart_AfterApril_ReturnsCurrentYear()
    {
        AprFiscal().GetFiscalYear(new DateOnly(2026, 10, 15)).Should().Be(2026);
    }

    [Fact]
    public void GetFiscalYear_EndYearNaming_ReturnsEndCalendarYear()
    {
        // Apr 1 2025 – Mar 31 2026 is FY2026 under EndYear naming
        AprFiscal(FiscalYearNaming.EndYear).GetFiscalYear(new DateOnly(2025, 6, 15))
            .Should().Be(2026);
    }

    [Fact]
    public void GetFiscalYear_JanStart_EndYearNamingMatchesCalendarYear()
    {
        var cal = new FiscalCalendar(new FiscalCalendarOptions { Naming = FiscalYearNaming.EndYear });
        cal.GetFiscalYear(new DateOnly(2026, 6, 15)).Should().Be(2026);
    }

    // GetFiscalYearStart / GetFiscalYearEnd

    [Fact]
    public void GetFiscalYearStart_AprFiscal_ReturnsAprilFirst()
    {
        AprFiscal().GetFiscalYearStart(2026).Should().Be(new DateOnly(2026, 4, 1));
    }

    [Fact]
    public void GetFiscalYearEnd_AprFiscal_ReturnsMarchThirtyFirst()
    {
        AprFiscal().GetFiscalYearEnd(2026).Should().Be(new DateOnly(2027, 3, 31));
    }

    [Fact]
    public void GetFiscalYearEnd_JanFiscal_ReturnsDecember31()
    {
        JanFiscal().GetFiscalYearEnd(2026).Should().Be(new DateOnly(2026, 12, 31));
    }

    // GetFiscalQuarter

    [Fact]
    public void GetFiscalQuarter_AprStart_AprilIsQ1()
    {
        AprFiscal().GetFiscalQuarter(new DateOnly(2026, 4, 15)).Should().Be(1);
    }

    [Fact]
    public void GetFiscalQuarter_AprStart_JulyIsQ2()
    {
        AprFiscal().GetFiscalQuarter(new DateOnly(2026, 7, 1)).Should().Be(2);
    }

    [Fact]
    public void GetFiscalQuarter_AprStart_OctoberIsQ3()
    {
        AprFiscal().GetFiscalQuarter(new DateOnly(2026, 10, 1)).Should().Be(3);
    }

    [Fact]
    public void GetFiscalQuarter_AprStart_JanuaryIsQ4()
    {
        // Jan 2027 falls in FY2026's Q4 (Apr 2026 – Mar 2027)
        AprFiscal().GetFiscalQuarter(new DateOnly(2027, 1, 1)).Should().Be(4);
    }

    [Fact]
    public void GetFiscalQuarter_JanStart_JanuaryIsQ1()
    {
        JanFiscal().GetFiscalQuarter(new DateOnly(2026, 1, 15)).Should().Be(1);
    }

    [Fact]
    public void GetFiscalQuarter_JanStart_OctoberIsQ4()
    {
        JanFiscal().GetFiscalQuarter(new DateOnly(2026, 10, 1)).Should().Be(4);
    }

    // GetQuarterStart / GetQuarterEnd

    [Fact]
    public void GetQuarterStart_AprFiscal_Q1StartsApril1()
    {
        AprFiscal().GetQuarterStart(2026, 1).Should().Be(new DateOnly(2026, 4, 1));
    }

    [Fact]
    public void GetQuarterStart_AprFiscal_Q2StartsJuly1()
    {
        AprFiscal().GetQuarterStart(2026, 2).Should().Be(new DateOnly(2026, 7, 1));
    }

    [Fact]
    public void GetQuarterStart_AprFiscal_Q4StartsJanuary1()
    {
        AprFiscal().GetQuarterStart(2026, 4).Should().Be(new DateOnly(2027, 1, 1));
    }

    [Fact]
    public void GetQuarterEnd_AprFiscal_Q1EndsJune30()
    {
        AprFiscal().GetQuarterEnd(2026, 1).Should().Be(new DateOnly(2026, 6, 30));
    }

    [Fact]
    public void GetQuarterEnd_AprFiscal_Q4EndsMarch31()
    {
        AprFiscal().GetQuarterEnd(2026, 4).Should().Be(new DateOnly(2027, 3, 31));
    }

    // IsSameFiscalYear

    [Fact]
    public void IsSameFiscalYear_BothInSameYear_ReturnsTrue()
    {
        AprFiscal().IsSameFiscalYear(new DateOnly(2026, 5, 1), new DateOnly(2026, 12, 31))
            .Should().BeTrue();
    }

    [Fact]
    public void IsSameFiscalYear_AcrossFiscalYearBoundary_ReturnsFalse()
    {
        // Mar 31 is in FY2025, Apr 1 is in FY2026
        AprFiscal().IsSameFiscalYear(new DateOnly(2026, 3, 31), new DateOnly(2026, 4, 1))
            .Should().BeFalse();
    }

    // Short constructor

    [Fact]
    public void ShortConstructor_AprStart_SameResultAsOptions()
    {
        var short_ = new FiscalCalendar(4, 1);
        var long_  = AprFiscal();
        var date   = new DateOnly(2026, 6, 15);
        short_.GetFiscalYear(date).Should().Be(long_.GetFiscalYear(date));
        short_.GetFiscalQuarter(date).Should().Be(long_.GetFiscalQuarter(date));
    }

    [Fact]
    public void ShortConstructor_JanStart_MatchesDefault()
    {
        var short_ = new FiscalCalendar(1, 1);
        var long_  = JanFiscal();
        var date   = new DateOnly(2026, 9, 1);
        short_.GetFiscalYear(date).Should().Be(long_.GetFiscalYear(date));
    }
}
