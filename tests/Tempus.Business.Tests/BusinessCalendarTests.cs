using Tempus.Business;
using Tempus.Business.Abstractions;
using Tempus.Business.Extensions;
using Tempus.Business.Models;
using Tempus.Core.Models;

namespace Tempus.Business.Tests;

public class BusinessCalendarTests
{
    private static IBusinessCalendar MakeCalendar(IHolidayProvider? provider = null)
    {
        var options = new BusinessCalendarOptions
        {
            BusinessHours = BusinessHours.NineToFive("UTC"),
            WeekendDays = [DayOfWeek.Saturday, DayOfWeek.Sunday]
        };
        return new BusinessCalendar(options, provider is null ? null : [provider]);
    }

    // IsBusinessDay
    [Fact]
    public void IsBusinessDay_Weekday_ReturnsTrue()
    {
        var cal = MakeCalendar();
        cal.IsBusinessDay(new DateOnly(2026, 5, 25)).Should().BeTrue(); // Monday
    }

    [Fact]
    public void IsBusinessDay_Saturday_ReturnsFalse()
    {
        var cal = MakeCalendar();
        cal.IsBusinessDay(new DateOnly(2026, 5, 23)).Should().BeFalse(); // Saturday
    }

    [Fact]
    public void IsBusinessDay_Sunday_ReturnsFalse()
    {
        var cal = MakeCalendar();
        cal.IsBusinessDay(new DateOnly(2026, 5, 24)).Should().BeFalse(); // Sunday
    }

    [Fact]
    public void IsBusinessDay_Holiday_ReturnsFalse()
    {
        var holiday = new DateOnly(2026, 12, 25);
        var provider = new FixedHolidayProvider([holiday]);
        var cal = MakeCalendar(provider);
        cal.IsBusinessDay(holiday).Should().BeFalse();
    }

    // NextBusinessDay
    [Fact]
    public void NextBusinessDay_FromFriday_ReturnsMondaySkippingWeekend()
    {
        var cal = MakeCalendar();
        var friday = new DateOnly(2026, 5, 22);
        cal.NextBusinessDay(friday).Should().Be(new DateOnly(2026, 5, 25)); // Monday
    }

    [Fact]
    public void NextBusinessDay_SkipsHoliday()
    {
        var monday = new DateOnly(2026, 5, 25);
        var provider = new FixedHolidayProvider([monday]);
        var cal = MakeCalendar(provider);
        cal.NextBusinessDay(new DateOnly(2026, 5, 22)).Should().Be(new DateOnly(2026, 5, 26)); // Tuesday
    }

    // PreviousBusinessDay
    [Fact]
    public void PreviousBusinessDay_FromMonday_ReturnsFridaySkippingWeekend()
    {
        var cal = MakeCalendar();
        var monday = new DateOnly(2026, 5, 25);
        cal.PreviousBusinessDay(monday).Should().Be(new DateOnly(2026, 5, 22)); // Friday
    }

    // AddBusinessDays
    [Fact]
    public void AddBusinessDays_FiveDays_SkipsWeekend()
    {
        var cal = MakeCalendar();
        var monday = new DateOnly(2026, 5, 25);
        cal.AddBusinessDays(monday, 5).Should().Be(new DateOnly(2026, 6, 1)); // Next Monday
    }

    [Fact]
    public void AddBusinessDays_Zero_ReturnsSameDay()
    {
        var cal = MakeCalendar();
        var date = new DateOnly(2026, 5, 25);
        cal.AddBusinessDays(date, 0).Should().Be(date);
    }

    [Fact]
    public void AddBusinessDays_Negative_GoesBackward()
    {
        var cal = MakeCalendar();
        var friday = new DateOnly(2026, 5, 29);
        cal.AddBusinessDays(friday, -5).Should().Be(new DateOnly(2026, 5, 22)); // Previous Friday
    }

    // BusinessDaysBetween
    [Fact]
    public void BusinessDaysBetween_FullWorkWeek_ReturnsFive()
    {
        var cal = MakeCalendar();
        var monday = new DateOnly(2026, 5, 25);
        var saturday = new DateOnly(2026, 5, 30);
        cal.BusinessDaysBetween(monday, saturday).Should().Be(5);
    }

    [Fact]
    public void BusinessDaysBetween_SameDate_ReturnsZero()
    {
        var cal = MakeCalendar();
        var date = new DateOnly(2026, 5, 25);
        cal.BusinessDaysBetween(date, date).Should().Be(0);
    }

    [Fact]
    public void BusinessDaysBetween_EndBeforeStart_ReturnsZero()
    {
        var cal = MakeCalendar();
        cal.BusinessDaysBetween(new DateOnly(2026, 5, 28), new DateOnly(2026, 5, 25)).Should().Be(0);
    }

    // IsBusinessHour
    [Fact]
    public void IsBusinessHour_MondayAt10am_ReturnsTrue()
    {
        var cal = MakeCalendar();
        var moment = new DateTimeOffset(2026, 5, 25, 10, 0, 0, TimeSpan.Zero); // Monday 10:00 UTC
        cal.IsBusinessHour(moment).Should().BeTrue();
    }

    [Fact]
    public void IsBusinessHour_MondayAt8am_ReturnsFalse()
    {
        var cal = MakeCalendar();
        var moment = new DateTimeOffset(2026, 5, 25, 8, 0, 0, TimeSpan.Zero); // Before 9:00
        cal.IsBusinessHour(moment).Should().BeFalse();
    }

    [Fact]
    public void IsBusinessHour_Saturday_ReturnsFalse()
    {
        var cal = MakeCalendar();
        var moment = new DateTimeOffset(2026, 5, 23, 10, 0, 0, TimeSpan.Zero); // Saturday
        cal.IsBusinessHour(moment).Should().BeFalse();
    }

    // AddBusinessHours
    [Fact]
    public void AddBusinessHours_TwoHoursBeforeEndOfDay_StaysOnSameDay()
    {
        var cal = MakeCalendar();
        var moment = new DateTimeOffset(2026, 5, 25, 14, 0, 0, TimeSpan.Zero); // Monday 14:00
        var result = cal.AddBusinessHours(moment, 2);
        result.Hour.Should().Be(16);
        result.Day.Should().Be(25);
    }

    [Fact]
    public void AddBusinessHours_RollsOverToNextDay()
    {
        var cal = MakeCalendar();
        var moment = new DateTimeOffset(2026, 5, 25, 15, 0, 0, TimeSpan.Zero); // Monday 15:00
        var result = cal.AddBusinessHours(moment, 4); // 2h left today + 2h tomorrow
        result.Date.Should().Be(new DateTime(2026, 5, 26)); // Tuesday
        result.Hour.Should().Be(11); // 09:00 + 2h
    }

    [Fact]
    public void AddBusinessHours_RollsOverWeekend()
    {
        var cal = MakeCalendar();
        var moment = new DateTimeOffset(2026, 5, 22, 16, 0, 0, TimeSpan.Zero); // Friday 16:00
        var result = cal.AddBusinessHours(moment, 2); // 1h left Friday + 1h Monday
        result.Date.Should().Be(new DateTime(2026, 5, 25)); // Monday
        result.Hour.Should().Be(10); // 09:00 + 1h
    }

    // GetBusinessDays extension

    [Fact]
    public void GetBusinessDays_DateRange_MatchesBetween()
    {
        var cal   = MakeCalendar();
        var from  = new DateOnly(2026, 5, 18); // Monday
        var to    = new DateOnly(2026, 5, 25); // Monday
        var range = new DateRange(from, to);
        cal.GetBusinessDays(range).Should().Be(cal.BusinessDaysBetween(from, to));
    }

    [Fact]
    public void GetBusinessDays_TwoParams_MatchesBetween()
    {
        var cal  = MakeCalendar();
        var from = new DateOnly(2026, 5, 18);
        var to   = new DateOnly(2026, 5, 29);
        cal.GetBusinessDays(from, to).Should().Be(cal.BusinessDaysBetween(from, to));
    }

    // BusinessTimeBetween
    [Fact]
    public void BusinessTimeBetween_SameDay_ReturnsCorrectSpan()
    {
        var cal = MakeCalendar();
        var startDate = new DateTimeOffset(2026, 5, 25, 10, 0, 0, TimeSpan.Zero); // Monday 10:00
        var endDate = new DateTimeOffset(2026, 5, 25, 14, 0, 0, TimeSpan.Zero);   // Monday 14:00
        cal.BusinessTimeBetween(startDate, endDate).Should().Be(TimeSpan.FromHours(4));
    }

    [Fact]
    public void BusinessTimeBetween_SpansWeekend_ExcludesWeekend()
    {
        var cal = MakeCalendar();
        var startDate = new DateTimeOffset(2026, 5, 22, 9, 0, 0, TimeSpan.Zero);  // Friday 09:00
        var endDate = new DateTimeOffset(2026, 5, 25, 17, 0, 0, TimeSpan.Zero);   // Monday 17:00
        // Friday full day (8h) + Monday full day (8h) = 16h
        cal.BusinessTimeBetween(startDate, endDate).Should().Be(TimeSpan.FromHours(16));
    }
}

file sealed class FixedHolidayProvider : IHolidayProvider
{
    private readonly HashSet<DateOnly> _holidays;

    public FixedHolidayProvider(IEnumerable<DateOnly> holidays)
        => _holidays = [..holidays];

    public string Region => "test";
    public IEnumerable<Holiday> GetHolidays(int year)
        => _holidays.Where(h => h.Year == year)
                    .Select(h => new Holiday { Name = "Test Holiday", Date = h });
    public bool IsHoliday(DateOnly calendarDate) => _holidays.Contains(calendarDate);
}
