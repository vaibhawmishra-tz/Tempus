namespace Tempus.Core.Abstractions;

/// <summary>
/// Abstraction over system time. Inject this instead of using DateTime.UtcNow directly
/// so that time can be controlled in tests.
/// </summary>
public interface ITempusClock
{
    /// <summary>The current instant expressed as a UTC <see cref="DateTimeOffset"/>.</summary>
    DateTimeOffset UtcNow { get; }

    /// <summary>Today's date in UTC.</summary>
    DateOnly TodayUtc { get; }

    /// <summary>The current instant expressed in the given IANA timezone.</summary>
    DateTimeOffset NowIn(string ianaTimeZoneId);

    /// <summary>Today's date in the given IANA timezone.</summary>
    DateOnly TodayIn(string ianaTimeZoneId);
}
