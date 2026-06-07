namespace Tempus.Business.Abstractions;

/// <summary>
/// Tracks elapsed and remaining business time against an SLA target, accounting for weekends, holidays, and business hours.
/// </summary>
public interface ISlaTimer
{
    /// <summary>The instant at which the SLA clock started.</summary>
    DateTimeOffset StartedAt { get; }

    /// <summary>The total business-time budget allowed by the SLA.</summary>
    TimeSpan Target { get; }

    /// <summary>Business time elapsed since <see cref="StartedAt"/>.</summary>
    TimeSpan ElapsedBusinessTime { get; }

    /// <summary>Business time remaining before the SLA is breached. Returns <see cref="TimeSpan.Zero"/> once breached.</summary>
    TimeSpan RemainingBusinessTime { get; }

    /// <summary>Returns <c>true</c> if the elapsed business time has exceeded the <see cref="Target"/>.</summary>
    bool IsBreached { get; }

    /// <summary>The projected wall-clock instant at which the SLA will be (or was) breached.</summary>
    DateTimeOffset BreachesAt { get; }
}
