namespace Tempus.Core.Models;

/// <summary>Indicates the direction of a DST clock change.</summary>
public enum DstTransitionType
{
    /// <summary>Clocks advance (e.g. 02:00 → 03:00), creating a one-hour gap in local time.</summary>
    SpringForward,

    /// <summary>Clocks recede (e.g. 02:00 → 01:00), creating a one-hour overlap in local time.</summary>
    FallBack,
}
