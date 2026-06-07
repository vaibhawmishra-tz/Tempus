using Tempus.Business.Schedule;

namespace Tempus.Business.Tests;

public class RecurringScheduleTests
{
    // Daily

    [Fact]
    public void Daily_GeneratesConsecutiveDays()
    {
        var schedule = new RecurringSchedule { Start = new DateOnly(2026, 1, 1), Frequency = RecurrenceFrequency.Daily };

        schedule.GetOccurrences().Take(5).Should().Equal(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 2),
            new DateOnly(2026, 1, 3),
            new DateOnly(2026, 1, 4),
            new DateOnly(2026, 1, 5));
    }

    [Fact]
    public void Daily_Interval2_EveryOtherDay()
    {
        var schedule = new RecurringSchedule
        {
            Start = new DateOnly(2026, 1, 1),
            Frequency = RecurrenceFrequency.Daily,
            Interval = 2
        };

        schedule.GetOccurrences().Take(3).Should().Equal(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 3),
            new DateOnly(2026, 1, 5));
    }

    // Weekly

    [Fact]
    public void Weekly_DefaultDay_RepeatsSameDayEachWeek()
    {
        // Start on a Monday
        var schedule = new RecurringSchedule
        {
            Start = new DateOnly(2026, 5, 25),
            Frequency = RecurrenceFrequency.Weekly
        };

        schedule.GetOccurrences().Take(3).Should().Equal(
            new DateOnly(2026, 5, 25),
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 8));
    }

    [Fact]
    public void Weekly_Interval2_EveryOtherWeek()
    {
        var schedule = new RecurringSchedule
        {
            Start = new DateOnly(2026, 5, 25),
            Frequency = RecurrenceFrequency.Weekly,
            Interval = 2
        };

        schedule.GetOccurrences().Take(3).Should().Equal(
            new DateOnly(2026, 5, 25),
            new DateOnly(2026, 6, 8),
            new DateOnly(2026, 6, 22));
    }

    [Fact]
    public void Weekly_ByDayOfWeek_MondayWednesdayFriday()
    {
        var schedule = new RecurringSchedule
        {
            Start = new DateOnly(2026, 5, 25), // Monday
            Frequency = RecurrenceFrequency.Weekly,
            ByDayOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday]
        };

        schedule.GetOccurrences().Take(6).Should().Equal(
            new DateOnly(2026, 5, 25), // Mon
            new DateOnly(2026, 5, 27), // Wed
            new DateOnly(2026, 5, 29), // Fri
            new DateOnly(2026, 6, 1),  // Mon
            new DateOnly(2026, 6, 3),  // Wed
            new DateOnly(2026, 6, 5)); // Fri
    }

    // Monthly

    [Fact]
    public void Monthly_SameDayEachMonth()
    {
        var schedule = new RecurringSchedule
        {
            Start = new DateOnly(2026, 1, 15),
            Frequency = RecurrenceFrequency.Monthly
        };

        schedule.GetOccurrences().Take(4).Should().Equal(
            new DateOnly(2026, 1, 15),
            new DateOnly(2026, 2, 15),
            new DateOnly(2026, 3, 15),
            new DateOnly(2026, 4, 15));
    }

    [Fact]
    public void Monthly_Day31_ClampsToLastDayOfMonth()
    {
        var schedule = new RecurringSchedule
        {
            Start = new DateOnly(2026, 1, 31),
            Frequency = RecurrenceFrequency.Monthly,
            ByMonthDay = 31
        };

        schedule.GetOccurrences().Take(4).Should().Equal(
            new DateOnly(2026, 1, 31),
            new DateOnly(2026, 2, 28), // Feb 2026 has 28 days
            new DateOnly(2026, 3, 31),
            new DateOnly(2026, 4, 30));
    }

    // Yearly

    [Fact]
    public void Yearly_SameDateEachYear()
    {
        var schedule = new RecurringSchedule
        {
            Start = new DateOnly(2026, 3, 15),
            Frequency = RecurrenceFrequency.Yearly
        };

        schedule.GetOccurrences().Take(3).Should().Equal(
            new DateOnly(2026, 3, 15),
            new DateOnly(2027, 3, 15),
            new DateOnly(2028, 3, 15));
    }

    // Limits

    [Fact]
    public void Until_StopsAtLastInclusiveDate()
    {
        var schedule = new RecurringSchedule
        {
            Start = new DateOnly(2026, 1, 1),
            Frequency = RecurrenceFrequency.Daily,
            Until = new DateOnly(2026, 1, 5)
        };

        var occurrences = schedule.GetOccurrences().ToList();
        occurrences.Should().HaveCount(5);
        occurrences.Last().Should().Be(new DateOnly(2026, 1, 5));
    }

    [Fact]
    public void Count_LimitsOccurrencesFromStart()
    {
        var schedule = new RecurringSchedule
        {
            Start = new DateOnly(2026, 1, 1),
            Frequency = RecurrenceFrequency.Daily,
            Count = 3
        };

        schedule.GetOccurrences().Should().HaveCount(3);
    }

    // GetOccurrences(from)

    [Fact]
    public void GetOccurrences_From_StartsFromGivenDateInclusive()
    {
        var schedule = new RecurringSchedule
        {
            Start = new DateOnly(2026, 1, 1),
            Frequency = RecurrenceFrequency.Daily
        };

        schedule.GetOccurrences(new DateOnly(2026, 1, 5)).Take(3).Should().Equal(
            new DateOnly(2026, 1, 5),
            new DateOnly(2026, 1, 6),
            new DateOnly(2026, 1, 7));
    }

    // OccursOn

    [Fact]
    public void OccursOn_WeeklyOccurrenceDate_ReturnsTrue()
    {
        var schedule = new RecurringSchedule
        {
            Start = new DateOnly(2026, 1, 1), // Thursday
            Frequency = RecurrenceFrequency.Weekly
        };

        schedule.OccursOn(new DateOnly(2026, 1, 8)).Should().BeTrue();
        schedule.OccursOn(new DateOnly(2026, 1, 15)).Should().BeTrue();
    }

    [Fact]
    public void OccursOn_NonPatternDate_ReturnsFalse()
    {
        var schedule = new RecurringSchedule
        {
            Start = new DateOnly(2026, 1, 1), // Thursday
            Frequency = RecurrenceFrequency.Weekly
        };

        schedule.OccursOn(new DateOnly(2026, 1, 9)).Should().BeFalse(); // Friday, not Thursday
    }

    [Fact]
    public void OccursOn_BeforeStart_ReturnsFalse()
    {
        var schedule = new RecurringSchedule
        {
            Start = new DateOnly(2026, 1, 5),
            Frequency = RecurrenceFrequency.Daily
        };

        schedule.OccursOn(new DateOnly(2026, 1, 4)).Should().BeFalse();
    }

    [Fact]
    public void OccursOn_AfterUntil_ReturnsFalse()
    {
        var schedule = new RecurringSchedule
        {
            Start = new DateOnly(2026, 1, 1),
            Frequency = RecurrenceFrequency.Daily,
            Until = new DateOnly(2026, 1, 10)
        };

        schedule.OccursOn(new DateOnly(2026, 1, 11)).Should().BeFalse();
    }

    // Bounded GetOccurrences(from, to)

    [Fact]
    public void GetOccurrences_Bounded_ReturnsOnlyDatesInRange()
    {
        var schedule = new RecurringSchedule
        {
            Start     = new DateOnly(2026, 1, 1),
            Frequency = RecurrenceFrequency.Weekly,
        };
        var from = new DateOnly(2026, 1, 8);
        var to   = new DateOnly(2026, 1, 22);

        schedule.GetOccurrences(from, to).Should().Equal(
            new DateOnly(2026, 1, 8),
            new DateOnly(2026, 1, 15),
            new DateOnly(2026, 1, 22));
    }

    [Fact]
    public void GetOccurrences_Bounded_EmptyWhenFromExceedsTo()
    {
        var schedule = new RecurringSchedule
        {
            Start     = new DateOnly(2026, 1, 1),
            Frequency = RecurrenceFrequency.Daily,
            Count     = 100,
        };
        schedule.GetOccurrences(new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1))
            .Should().BeEmpty();
    }
}
