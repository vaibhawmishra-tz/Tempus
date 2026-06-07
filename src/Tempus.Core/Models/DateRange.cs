using System.Runtime.InteropServices;

namespace Tempus.Core.Models;

/// <summary>
/// An inclusive, calendar-date range from <see cref="Start"/> to <see cref="End"/>.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly struct DateRange : IEquatable<DateRange>
{
    /// <summary>The first date in the range (inclusive).</summary>
    public DateOnly Start { get; init; }

    /// <summary>The last date in the range (inclusive).</summary>
    public DateOnly End { get; init; }

    /// <summary>Returns <c>true</c> if <see cref="Start"/> is after <see cref="End"/>.</summary>
    public bool IsEmpty => Start > End;

    /// <summary>Total number of calendar days in the range, or 0 if <see cref="IsEmpty"/>.</summary>
    public int TotalDays => IsEmpty ? 0 : End.DayNumber - Start.DayNumber + 1;

    /// <summary>Initializes a new <see cref="DateRange"/> with the given start and end dates.</summary>
    public DateRange(DateOnly start, DateOnly end)
    {
        Start = start;
        End = end;
    }

    /// <summary>Initializes a <see cref="DateRange"/> from two UTC <see cref="DateTimeOffset"/> values.</summary>
    public DateRange(DateTimeOffset start, DateTimeOffset end)
        : this(DateOnly.FromDateTime(start.UtcDateTime), DateOnly.FromDateTime(end.UtcDateTime)) { }

    /// <summary>Returns <c>true</c> if <paramref name="date"/> is within [<see cref="Start"/>, <see cref="End"/>].</summary>
    public bool Contains(DateOnly date) => date >= Start && date <= End;

    /// <summary>Returns <c>true</c> if this range shares at least one day with <paramref name="other"/>.</summary>
    public bool Overlaps(DateRange other) => Start <= other.End && End >= other.Start;

    /// <summary>Returns the intersection of this range and <paramref name="other"/>, or <c>null</c> if they do not overlap.</summary>
    public DateRange? Intersect(DateRange other)
    {
        DateOnly start = Start > other.Start ? Start : other.Start;
        DateOnly end = End < other.End ? End : other.End;
        return start <= end ? new DateRange(start, end) : null;
    }

    /// <summary>Enumerates every <see cref="DateOnly"/> in the range, from <see cref="Start"/> to <see cref="End"/> inclusive.</summary>
    public IEnumerable<DateOnly> ToDates()
    {
        DateOnly current = Start;
        while (current <= End)
        {
            yield return current;
            current = current.AddDays(1);
        }
    }

    public bool Equals(DateRange other) => Start == other.Start && End == other.End;
    public override bool Equals(object? obj) => obj is DateRange other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Start, End);
    public override string ToString() => $"{Start:yyyy-MM-dd} — {End:yyyy-MM-dd}";

    public static bool operator ==(DateRange left, DateRange right) => left.Equals(right);
    public static bool operator !=(DateRange left, DateRange right) => !left.Equals(right);
}
