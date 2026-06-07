using System.Globalization;
using Tempus.Core.Extensions;

namespace Tempus.AspNetCore.Context;

internal sealed class TempusUserContext : ITempusUserContext
{
    private readonly Func<DateTimeOffset> _utcNow;

    internal TempusUserContext(string timeZoneId, TimeZoneInfo timeZone, Func<DateTimeOffset> utcNow)
    {
        TimeZoneId = timeZoneId;
        TimeZone = timeZone;
        _utcNow = utcNow;
    }

    public string TimeZoneId { get; }
    public TimeZoneInfo TimeZone { get; }

    public DateTimeOffset NowForUser => TimeZoneInfo.ConvertTime(_utcNow(), TimeZone);
    public DateOnly TodayForUser => DateOnly.FromDateTime(NowForUser.DateTime);

    public DateTimeOffset ToUserTime(DateTimeOffset utc)
        => TimeZoneInfo.ConvertTime(utc, TimeZone);

    public DateTimeOffset ToUtc(DateTime userLocal)
    {
        var unspecified = DateTime.SpecifyKind(userLocal, DateTimeKind.Unspecified);
        return new DateTimeOffset(unspecified, TimeZone.GetUtcOffset(unspecified)).ToUniversalTime();
    }

    public string Format(DateTimeOffset utc, string? format = null)
        => ToUserTime(utc).ToString(format ?? "yyyy-MM-dd HH:mm zzz", CultureInfo.InvariantCulture);

    public string ToRelativeString(DateTimeOffset utc) => utc.ToRelativeString();
}
