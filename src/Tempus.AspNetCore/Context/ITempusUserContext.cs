namespace Tempus.AspNetCore.Context;

/// <summary>
/// Per-request timezone context resolved from X-Timezone header, JWT claim, or query param.
/// Inject this in controllers and services to convert UTC to the user's local time.
/// </summary>
public interface ITempusUserContext
{
    string TimeZoneId { get; }
    TimeZoneInfo TimeZone { get; }
    DateTimeOffset NowForUser { get; }
    DateOnly TodayForUser { get; }

    DateTimeOffset ToUserTime(DateTimeOffset utc);
    DateTimeOffset ToUtc(DateTime userLocal);
    string Format(DateTimeOffset utc, string? format = null);
    string ToRelativeString(DateTimeOffset utc);
}
