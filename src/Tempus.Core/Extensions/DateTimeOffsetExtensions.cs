using System.Globalization;
using System.Runtime.CompilerServices;
using Tempus.Core.Internal;

namespace Tempus.Core.Extensions;

public static class DateTimeOffsetExtensions
{
    // ── Timezone conversion ──────────────────────────────────────────

    public static DateTimeOffset ToZone(this DateTimeOffset value, string ianaTimeZoneId)
    {
        TimeZoneInfo tz = TimezoneResolverAccessor.Resolver.Resolve(ianaTimeZoneId);
        return TimeZoneInfo.ConvertTime(value, tz);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset ToUtc(this DateTimeOffset value)
        => value.ToUniversalTime();

    // ── Period boundaries ────────────────────────────────────────────

    public static DateTimeOffset StartOfDay(this DateTimeOffset value)
        => new(value.Year, value.Month, value.Day, 0, 0, 0, value.Offset);

    public static DateTimeOffset EndOfDay(this DateTimeOffset value)
        => new(value.Year, value.Month, value.Day, 23, 59, 59, 999, value.Offset);

    public static DateTimeOffset StartOfMonth(this DateTimeOffset value)
        => new(value.Year, value.Month, 1, 0, 0, 0, value.Offset);

    public static DateTimeOffset EndOfMonth(this DateTimeOffset value)
    {
        int lastDay = DateTime.DaysInMonth(value.Year, value.Month);
        return new(value.Year, value.Month, lastDay, 23, 59, 59, 999, value.Offset);
    }

    public static DateTimeOffset StartOfYear(this DateTimeOffset value)
        => new(value.Year, 1, 1, 0, 0, 0, value.Offset);

    public static DateTimeOffset EndOfYear(this DateTimeOffset value)
        => new(value.Year, 12, 31, 23, 59, 59, 999, value.Offset);

    // ── Validation ───────────────────────────────────────────────────

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInPast(this DateTimeOffset value) => value < DateTimeOffset.UtcNow;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInFuture(this DateTimeOffset value) => value > DateTimeOffset.UtcNow;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWeekend(this DateTimeOffset value)
        => value.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWeekday(this DateTimeOffset value) => !value.IsWeekend();

    // ── Human-readable ───────────────────────────────────────────────

    public static string ToRelativeString(this DateTimeOffset value)
        => ToRelativeString(value, DateTimeOffset.UtcNow);

    public static string ToRelativeString(this DateTimeOffset value, DateTimeOffset relativeTo)
    {
        TimeSpan diff = relativeTo - value;
        bool past = diff.TotalSeconds >= 0;
        TimeSpan abs = past ? diff : -diff;

        if (abs.TotalSeconds < 60)
            return "just now";

        if (abs.TotalSeconds < 3600)
        {
            int mins = (int)abs.TotalMinutes;
            return $"{mins} minute{(mins != 1 ? "s" : "")} {(past ? "ago" : "from now")}";
        }

        if (abs.TotalSeconds < 86400)
        {
            int hrs = (int)abs.TotalHours;
            return $"{hrs} hour{(hrs != 1 ? "s" : "")} {(past ? "ago" : "from now")}";
        }

        if (abs.TotalDays < 7)
        {
            int days = (int)abs.TotalDays;
            return $"{days} day{(days != 1 ? "s" : "")} {(past ? "ago" : "from now")}";
        }

        return value.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture);
    }
}
