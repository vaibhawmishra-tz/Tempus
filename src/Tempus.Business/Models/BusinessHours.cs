using System.Runtime.InteropServices;

namespace Tempus.Business.Models;

/// <summary>
/// Defines the start and end of the business day within a specific timezone.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly struct BusinessHours : IEquatable<BusinessHours>
{
    /// <summary>The time of day when business hours begin (inclusive).</summary>
    public TimeOnly Start { get; init; }

    /// <summary>The time of day when business hours end (exclusive).</summary>
    public TimeOnly End { get; init; }

    /// <summary>The IANA timezone ID in which <see cref="Start"/> and <see cref="End"/> are expressed.</summary>
    public string TimeZoneId { get; init; }

    /// <summary>The total length of the business day.</summary>
    public TimeSpan Duration => End - Start;

    /// <summary>Returns <c>true</c> if <paramref name="time"/> falls within [<see cref="Start"/>, <see cref="End"/>).</summary>
    public bool Contains(TimeOnly time) => time >= Start && time < End;

    /// <summary>Returns a standard 09:00–17:00 window in the given timezone (defaults to UTC).</summary>
    public static BusinessHours NineToFive(string timeZoneId = "UTC") => new()
    {
        Start = new TimeOnly(9, 0),
        End = new TimeOnly(17, 0),
        TimeZoneId = timeZoneId
    };

    public bool Equals(BusinessHours other) =>
        Start == other.Start && End == other.End &&
        string.Equals(TimeZoneId, other.TimeZoneId, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is BusinessHours other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Start, End, TimeZoneId);
    public static bool operator ==(BusinessHours left, BusinessHours right) => left.Equals(right);
    public static bool operator !=(BusinessHours left, BusinessHours right) => !left.Equals(right);
}
