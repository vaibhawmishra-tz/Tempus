using FsCheck;
using FsCheck.Xunit;
using Tempus.Business.Schedule;

namespace Tempus.Business.Tests;

/// <summary>
/// Property-based tests for RecurringSchedule using FsCheck.
/// Each [Property] test generates random inputs and verifies invariants.
/// </summary>
public class RecurringSchedulePropertyTests
{
    // ── Daily ─────────────────────────────────────────────────────────────────

    [Property(MaxTest = 200)]
    public Property Daily_OccurrencesAreMonotonicallyIncreasing(
        PositiveInt interval)
    {
        var schedule = new RecurringSchedule
        {
            Start     = new DateOnly(2026, 1, 1),
            Frequency = RecurrenceFrequency.Daily,
            Interval  = interval.Get,
            Count     = 50,
        };

        var dates = schedule.GetOccurrences().ToList();
        bool monotonic = dates.Zip(dates.Skip(1), (a, b) => a < b).All(x => x);
        return monotonic.ToProperty();
    }

    [Property(MaxTest = 200)]
    public Property Daily_CountLimitIsExact(PositiveInt count)
    {
        int n = count.Get % 100 + 1; // keep 1..100
        var schedule = new RecurringSchedule
        {
            Start     = new DateOnly(2026, 1, 1),
            Frequency = RecurrenceFrequency.Daily,
            Count     = n,
        };

        return (schedule.GetOccurrences().Count() == n).ToProperty();
    }

    [Property(MaxTest = 200)]
    public Property Daily_UntilBoundNeverExceeded(PositiveInt daysUntil)
    {
        var start = new DateOnly(2026, 1, 1);
        int d     = daysUntil.Get % 365 + 1;
        var until = start.AddDays(d);

        var schedule = new RecurringSchedule
        {
            Start     = start,
            Frequency = RecurrenceFrequency.Daily,
            Until     = until,
        };

        return schedule.GetOccurrences().All(o => o <= until).ToProperty();
    }

    // ── OccursOn consistency ──────────────────────────────────────────────────

    [Property(MaxTest = 300)]
    public Property OccursOn_MatchesGetOccurrences_Weekly(PositiveInt weeksAhead)
    {
        var start = new DateOnly(2026, 3, 2); // Monday
        int w     = weeksAhead.Get % 52 + 1;
        var date  = start.AddDays(w * 7);

        var schedule = new RecurringSchedule
        {
            Start     = start,
            Frequency = RecurrenceFrequency.Weekly,
            Count     = w + 5,
        };

        bool inList   = schedule.GetOccurrences().Contains(date);
        bool occursOn = schedule.OccursOn(date);
        return (inList == occursOn).ToProperty();
    }

    [Property(MaxTest = 300)]
    public Property OccursOn_Consistent_WithGetOccurrences_Monthly(PositiveInt monthsAhead)
    {
        var start  = new DateOnly(2026, 1, 15);
        int m      = monthsAhead.Get % 24 + 1;
        var target = start.AddMonths(m);

        var schedule = new RecurringSchedule
        {
            Start     = start,
            Frequency = RecurrenceFrequency.Monthly,
            Count     = m + 5,
        };

        bool inList   = schedule.GetOccurrences().Contains(target);
        bool occursOn = schedule.OccursOn(target);
        return (inList == occursOn).ToProperty();
    }

    // ── Bounded GetOccurrences(from, to) ─────────────────────────────────────

    [Property(MaxTest = 200)]
    public Property BoundedOccurrences_NeverExceedBounds(
        PositiveInt fromOffset,
        PositiveInt toOffset)
    {
        var start = new DateOnly(2026, 1, 1);
        var from  = start.AddDays(fromOffset.Get % 100);
        var to    = from.AddDays(toOffset.Get % 365 + 1);

        var schedule = new RecurringSchedule
        {
            Start     = start,
            Frequency = RecurrenceFrequency.Daily,
            Interval  = 3,
        };

        return schedule.GetOccurrences(from, to)
            .All(d => d >= from && d <= to)
            .ToProperty();
    }

    // ── Interval spacing ──────────────────────────────────────────────────────

    [Property(MaxTest = 100)]
    public Property Daily_IntervalSpacingIsExact(PositiveInt interval)
    {
        int step = interval.Get % 20 + 1;
        var schedule = new RecurringSchedule
        {
            Start     = new DateOnly(2026, 1, 1),
            Frequency = RecurrenceFrequency.Daily,
            Interval  = step,
            Count     = 20,
        };

        var dates = schedule.GetOccurrences().ToList();
        bool correctSpacing = dates.Zip(dates.Skip(1), (a, b) => (b.DayNumber - a.DayNumber) == step)
            .All(x => x);
        return correctSpacing.ToProperty();
    }
}
