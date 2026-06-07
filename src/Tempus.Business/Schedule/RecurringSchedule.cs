namespace Tempus.Business.Schedule;

public sealed class RecurringSchedule
{
    public required DateOnly Start { get; init; }
    public required RecurrenceFrequency Frequency { get; init; }
    public int Interval { get; init; } = 1;
    public IReadOnlyList<DayOfWeek>? ByDayOfWeek { get; init; }
    public int? ByMonthDay { get; init; }
    public DateOnly? Until { get; init; }
    public int? Count { get; init; }

    /// <summary>
    /// Returns all occurrences on or after <paramref name="from"/> (defaults to Start),
    /// respecting Until and Count limits.
    /// </summary>
    public IEnumerable<DateOnly> GetOccurrences(DateOnly? from = null)
    {
        var effectiveFrom = from ?? Start;
        return GenerateCandidates().Where(d => d >= effectiveFrom);
    }

    /// <summary>
    /// Returns all occurrences within the inclusive range [<paramref name="from"/>, <paramref name="to"/>],
    /// respecting Until and Count limits.
    /// </summary>
    public IEnumerable<DateOnly> GetOccurrences(DateOnly from, DateOnly to)
        => GetOccurrences(from).TakeWhile(d => d <= to);

    /// <summary>Returns true if the schedule has an occurrence on the exact given date.</summary>
    public bool OccursOn(DateOnly date)
    {
        if (date < Start) return false;
        if (Until.HasValue && date > Until.Value) return false;
        return GetOccurrences(date).FirstOrDefault() == date;
    }

    private IEnumerable<DateOnly> GenerateCandidates()
    {
        int yielded = 0;
        int maxCount = Count ?? int.MaxValue;

        foreach (var candidate in GenerateRawCandidates())
        {
            if (Until.HasValue && candidate > Until.Value) yield break;
            if (yielded >= maxCount) yield break;
            yielded++;
            yield return candidate;
        }
    }

    private IEnumerable<DateOnly> GenerateRawCandidates() => Frequency switch
    {
        RecurrenceFrequency.Daily   => GenerateDailyCandidates(),
        RecurrenceFrequency.Weekly  => GenerateWeeklyCandidates(),
        RecurrenceFrequency.Monthly => GenerateMonthlyCandidates(),
        RecurrenceFrequency.Yearly  => GenerateYearlyCandidates(),
        _ => throw new InvalidOperationException($"Unknown frequency: {Frequency}")
    };

    private IEnumerable<DateOnly> GenerateDailyCandidates()
    {
        var current = Start;
        while (true)
        {
            yield return current;
            current = current.AddDays(Interval);
        }
    }

    private IEnumerable<DateOnly> GenerateWeeklyCandidates()
    {
        var activeDays = ByDayOfWeek is { Count: > 0 }
            ? ByDayOfWeek.OrderBy(d => ((int)d - (int)DayOfWeek.Monday + 7) % 7).ToList()
            : (IReadOnlyList<DayOfWeek>)[Start.DayOfWeek];

        // Anchor to the Monday of Start's ISO week so the cycle boundary is predictable.
        int fromMonday = ((int)Start.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var baseMonday = Start.AddDays(-fromMonday);

        for (int week = 0; ; week++)
        {
            var weekMonday = baseMonday.AddDays(week * 7 * Interval);
            foreach (var dow in activeDays)
            {
                int offset = ((int)dow - (int)DayOfWeek.Monday + 7) % 7;
                var candidate = weekMonday.AddDays(offset);
                if (candidate >= Start)
                    yield return candidate;
            }
        }
    }

    private IEnumerable<DateOnly> GenerateMonthlyCandidates()
    {
        int targetDay = ByMonthDay ?? Start.Day;
        var cursor = new DateOnly(Start.Year, Start.Month, 1);

        while (true)
        {
            int day = Math.Min(targetDay, DateTime.DaysInMonth(cursor.Year, cursor.Month));
            var candidate = new DateOnly(cursor.Year, cursor.Month, day);
            if (candidate >= Start)
                yield return candidate;
            cursor = cursor.AddMonths(Interval);
        }
    }

    private IEnumerable<DateOnly> GenerateYearlyCandidates()
    {
        for (int i = 0; ; i++)
            yield return Start.AddYears(i * Interval);
    }
}
