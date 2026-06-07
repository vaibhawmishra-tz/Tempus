#pragma warning disable MA0048 // multiple types in one file — intentional for grouped DTOs
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tempus.Sample.WebApi.Controllers;

// ── Responses ────────────────────────────────────────────────────────────────

/// <summary>World-clock snapshot: current UTC plus local times in featured zones.</summary>
public sealed class WorldClockResponse
{
    /// <example>2026-05-30T14:00:00.0000000+00:00</example>
    public required string UtcNow { get; init; }

    /// <summary>One entry per featured timezone.</summary>
    public required IReadOnlyList<ZoneTimeEntry> Zones { get; init; }
}

/// <summary>Local time and offset for one timezone.</summary>
public sealed class ZoneTimeEntry
{
    /// <example>America/New_York</example>
    public required string TimeZoneId { get; init; }

    /// <example>2026-05-30T10:00:00.0000000-04:00</example>
    public required string LocalTime { get; init; }

    /// <example>-04:00:00</example>
    public required string UtcOffset { get; init; }

    /// <example>just now</example>
    public required string RelativeAge { get; init; }
}

/// <summary>Result of a single time-zone conversion.</summary>
public sealed class ConvertResponse
{
    /// <example>2026-05-30T14:00:00.0000000+00:00</example>
    public required string UtcTime { get; init; }

    /// <example>Asia/Tokyo</example>
    public required string TargetTimeZone { get; init; }

    /// <example>2026-05-30T23:00:00.0000000+09:00</example>
    public required string LocalTime { get; init; }

    /// <example>+09:00:00</example>
    public required string UtcOffset { get; init; }

    /// <example>just now</example>
    public required string RelativeTime { get; init; }
}

/// <summary>One timezone entry from the system list.</summary>
public sealed class TimeZoneEntry
{
    /// <example>America/Toronto</example>
    public required string Id { get; init; }

    /// <example>(UTC-05:00) Eastern Time (US &amp; Canada)</example>
    public required string DisplayName { get; init; }

    /// <example>-05:00:00</example>
    public required string UtcOffset { get; init; }

    /// <example>true</example>
    public bool SupportsDst { get; init; }
}

/// <summary>Relative-time result for a timezone.</summary>
public sealed class RelativeTimeResponse
{
    /// <example>Europe/London</example>
    public required string TimeZoneId { get; init; }

    /// <example>2026-05-30T15:00:00.0000000+01:00</example>
    public required string LocalTime { get; init; }

    /// <example>+01:00:00</example>
    public required string UtcOffset { get; init; }

    /// <example>just now</example>
    public required string RelativeTime { get; init; }
}

/// <summary>Standard error envelope.</summary>
public sealed class ErrorResponse
{
    /// <example>Unknown timezone: 'Bad/Zone'.</example>
    public required string Error { get; init; }
}

// ── Requests ─────────────────────────────────────────────────────────────────

/// <summary>Time-zone conversion request.</summary>
public sealed class ConvertRequest
{
    /// <summary>The UTC instant to convert.</summary>
    /// <example>2026-05-30T14:00:00Z</example>
    [DefaultValue("2026-05-30T14:00:00Z")]
    public DateTimeOffset UtcTime { get; init; } = new(2026, 5, 30, 14, 0, 0, TimeSpan.Zero);

    /// <summary>Target IANA timezone ID (e.g. <c>America/New_York</c>, <c>Asia/Tokyo</c>).</summary>
    /// <example>America/New_York</example>
    [Required]
    [DefaultValue("America/New_York")]
    public required string TargetTimeZone { get; init; }
}
