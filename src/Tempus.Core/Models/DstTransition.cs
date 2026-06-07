using System.Runtime.InteropServices;

namespace Tempus.Core.Models;

/// <summary>
/// Describes a single DST transition — a spring-forward or fall-back — for a given timezone.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly struct DstTransition : IEquatable<DstTransition>
{
    /// <summary>The IANA timezone ID in which this transition occurs.</summary>
    public string TimeZoneId { get; init; }

    /// <summary>The UTC instant at which the clock changes.</summary>
    public DateTimeOffset OccursAt { get; init; }

    /// <summary>Whether this is a spring-forward or fall-back transition.</summary>
    public DstTransitionType Type { get; init; }

    /// <summary>The UTC offset in effect immediately before this transition.</summary>
    public TimeSpan OffsetBefore { get; init; }

    /// <summary>The UTC offset in effect immediately after this transition.</summary>
    public TimeSpan OffsetAfter { get; init; }

    /// <summary>The local time at which clocks are moved (e.g. 02:00 for most US zones).</summary>
    public TimeOnly AffectedLocalTime { get; init; }

    /// <summary>Returns <c>true</c> if this is a spring-forward (clocks advance, creating a gap).</summary>
    public bool IsSpringForward => Type == DstTransitionType.SpringForward;

    /// <summary>Returns <c>true</c> if this is a fall-back (clocks recede, creating an overlap).</summary>
    public bool IsFallBack => Type == DstTransitionType.FallBack;

    /// <summary>The magnitude and direction of the clock change (<see cref="OffsetAfter"/> minus <see cref="OffsetBefore"/>).</summary>
    public TimeSpan ClockShift => OffsetAfter - OffsetBefore;

    public bool Equals(DstTransition other) =>
        string.Equals(TimeZoneId, other.TimeZoneId, StringComparison.Ordinal) &&
        OccursAt == other.OccursAt && Type == other.Type;

    public override bool Equals(object? obj) => obj is DstTransition other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(TimeZoneId, OccursAt, Type);
    public override string ToString() =>
        $"{TimeZoneId}: {Type} at {OccursAt:yyyy-MM-dd HH:mm zzz} ({OffsetBefore:hh\\:mm} → {OffsetAfter:hh\\:mm})";

    public static bool operator ==(DstTransition left, DstTransition right) => left.Equals(right);
    public static bool operator !=(DstTransition left, DstTransition right) => !left.Equals(right);
}
