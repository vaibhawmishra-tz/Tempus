using Microsoft.AspNetCore.Mvc;
using Tempus.Core.Abstractions;
using Tempus.Core.Extensions;

namespace Tempus.Sample.WebApi.Controllers;

/// <summary>
/// World clock, timezone lookup, and time-conversion endpoints.
/// All timestamps are returned as ISO 8601 strings with UTC offset.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class WorldClockController : ControllerBase
{
    private static readonly string[] FeaturedZones =
    [
        "America/Toronto",
        "America/New_York",
        "America/Los_Angeles",
        "America/Chicago",
        "Europe/London",
        "Europe/Paris",
        "Europe/Berlin",
        "Asia/Kolkata",
        "Asia/Tokyo",
        "Asia/Singapore",
        "Australia/Sydney",
    ];

    private readonly ITempusClock _clock;

    public WorldClockController(ITempusClock clock) => _clock = clock;

    /// <summary>Returns current UTC time alongside local times in eleven popular timezones.</summary>
    /// <returns>A world-clock snapshot with one entry per featured timezone.</returns>
    [HttpGet("now")]
    [ProducesResponseType(typeof(WorldClockResponse), StatusCodes.Status200OK)]
    public IActionResult GetNow()
    {
        var utc = _clock.UtcNow;

        return Ok(new WorldClockResponse
        {
            UtcNow = utc.ToString("o"),
            Zones  = FeaturedZones.Select(tz =>
            {
                var local = utc.ToZone(tz);
                return new ZoneTimeEntry
                {
                    TimeZoneId  = tz,
                    LocalTime   = local.ToString("o"),
                    UtcOffset   = local.Offset.ToString(),
                    RelativeAge = utc.ToRelativeString(),
                };
            }).ToArray(),
        });
    }

    /// <summary>Converts a UTC timestamp to a target IANA timezone.</summary>
    /// <remarks>
    /// Use the <c>GET /api/worldclock/zones</c> endpoint to discover valid timezone IDs.
    ///
    /// **Sample request body:**
    /// ```json
    /// { "utcTime": "2026-05-30T14:00:00Z", "targetTimeZone": "America/New_York" }
    /// ```
    /// </remarks>
    /// <param name="request">The UTC instant and target timezone ID.</param>
    /// <returns>The converted local time with offset details.</returns>
    /// <response code="200">Conversion succeeded.</response>
    /// <response code="400">The timezone ID is unknown or the request body is invalid.</response>
    [HttpPost("convert")]
    [ProducesResponseType(typeof(ConvertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IActionResult Convert([FromBody] ConvertRequest request)
    {
        try
        {
            var local = request.UtcTime.ToZone(request.TargetTimeZone);
            return Ok(new ConvertResponse
            {
                UtcTime        = request.UtcTime.ToString("o"),
                TargetTimeZone = request.TargetTimeZone,
                LocalTime      = local.ToString("o"),
                UtcOffset      = local.Offset.ToString(),
                RelativeTime   = request.UtcTime.ToRelativeString(),
            });
        }
        catch (TimeZoneNotFoundException)
        {
            return BadRequest(new ErrorResponse
            {
                Error = $"Unknown timezone: '{request.TargetTimeZone}'. Call GET /api/worldclock/zones for valid IDs.",
            });
        }
    }

    /// <summary>Lists all IANA-compatible system timezones available on this host.</summary>
    /// <returns>Timezones sorted by UTC offset then ID.</returns>
    [HttpGet("zones")]
    [ProducesResponseType(typeof(IEnumerable<TimeZoneEntry>), StatusCodes.Status200OK)]
    public IActionResult ListZones()
    {
        var zones = TimeZoneInfo.GetSystemTimeZones()
            .OrderBy(z => z.BaseUtcOffset)
            .ThenBy(z => z.Id, StringComparer.Ordinal)
            .Select(z => new TimeZoneEntry
            {
                Id          = z.Id,
                DisplayName = z.DisplayName,
                UtcOffset   = z.BaseUtcOffset.ToString(),
                SupportsDst = z.SupportsDaylightSavingTime,
            });

        return Ok(zones);
    }

    /// <summary>Returns the current local time and a relative-time string for any timezone.</summary>
    /// <param name="zone">
    /// An IANA timezone ID, e.g. <c>Asia/Tokyo</c> or <c>Europe/London</c>.
    /// URL-encode the slash: <c>Asia%2FTokyo</c>.
    /// </param>
    /// <returns>Local time, UTC offset, and a human-readable relative-time string.</returns>
    /// <response code="200">Timezone found and local time returned.</response>
    /// <response code="404">The timezone ID is not recognised on this host.</response>
    [HttpGet("{zone}/info")]
    [ProducesResponseType(typeof(RelativeTimeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetZoneInfo(string zone)
    {
        try
        {
            var utc   = _clock.UtcNow;
            var local = utc.ToZone(zone);
            return Ok(new RelativeTimeResponse
            {
                TimeZoneId   = zone,
                LocalTime    = local.ToString("o"),
                UtcOffset    = local.Offset.ToString(),
                RelativeTime = utc.ToRelativeString(),
            });
        }
        catch (TimeZoneNotFoundException)
        {
            return NotFound(new ErrorResponse
            {
                Error = $"Unknown timezone: '{zone}'. Call GET /api/worldclock/zones for valid IDs.",
            });
        }
    }
}
