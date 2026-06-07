namespace Tempus.Core.Abstractions;

/// <summary>
/// Resolves IANA and Windows timezone IDs to TimeZoneInfo cross-platform.
/// </summary>
public interface ITimezoneResolver
{
    /// <summary>Resolves an IANA or Windows timezone ID to a <see cref="TimeZoneInfo"/>. Throws if the ID is unknown.</summary>
    TimeZoneInfo Resolve(string timezoneId);

    /// <summary>Attempts to resolve an IANA or Windows timezone ID. Returns <c>false</c> if the ID is unknown.</summary>
    bool TryResolve(string timezoneId, out TimeZoneInfo? timeZoneInfo);

    /// <summary>Normalises any timezone ID (IANA or Windows) to its canonical IANA form.</summary>
    string ToIanaId(string timezoneId);

    /// <summary>Converts an IANA timezone ID to its Windows registry equivalent (e.g. "America/New_York" → "Eastern Standard Time").</summary>
    string ToWindowsId(string ianaId);

    /// <summary>All IANA timezone IDs recognised by this resolver on the current platform.</summary>
    IReadOnlyList<string> AllIanaIds { get; }
}
