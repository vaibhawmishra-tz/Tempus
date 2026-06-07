using Tempus.Core.Models;

namespace Tempus.Core.Abstractions;

/// <summary>
/// Resolves DST ambiguous and invalid times using a configured strategy.
/// </summary>
public interface IDstResolver
{
    /// <summary>Converts a local <see cref="DateTime"/> in the given IANA timezone to UTC, resolving DST ambiguity.</summary>
    ConversionResult ToUtc(DateTime localDateTime, string ianaTimeZoneId);

    /// <summary>Converts a local <see cref="DateTime"/> to UTC using the provided <see cref="TimeZoneInfo"/>, resolving DST ambiguity.</summary>
    ConversionResult ToUtc(DateTime localDateTime, TimeZoneInfo timeZone);

    /// <summary>Returns the next DST transition for the given IANA timezone after the current instant.</summary>
    DstTransition? NextTransition(string ianaTimeZoneId);

    /// <summary>Returns the next DST transition for the given IANA timezone after <paramref name="after"/>.</summary>
    DstTransition? NextTransition(string ianaTimeZoneId, DateTimeOffset after);

    /// <summary>Returns all DST transitions that occur in the given calendar year for the specified IANA timezone.</summary>
    IReadOnlyList<DstTransition> TransitionsInYear(string ianaTimeZoneId, int year);

    /// <summary>
    /// Returns <c>true</c> if <paramref name="utcTime"/> is within <paramref name="within"/> of the next
    /// DST transition in the given IANA timezone. Useful for proactively alerting users.
    /// </summary>
    bool IsNearTransition(DateTimeOffset utcTime, string ianaTimeZoneId, TimeSpan within);
}
