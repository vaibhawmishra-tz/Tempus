using System.Runtime.CompilerServices;

namespace Tempus.Core.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Sets Kind = Utc without converting the value. Use only when you are certain
    /// the DateTime is already in UTC but was returned with Unspecified kind (e.g. from EF Core).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime AsUtc(this DateTime value)
        => DateTime.SpecifyKind(value, DateTimeKind.Utc);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset AsUtcOffset(this DateTime value)
        => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWeekend(this DateTime value)
        => value.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWeekday(this DateTime value) => !value.IsWeekend();

    public static DateTime StartOfDay(this DateTime value)
        => value.Date;

    public static DateTime EndOfDay(this DateTime value)
        => value.Date.AddDays(1).AddTicks(-1);

    public static DateTime StartOfMonth(this DateTime value)
        => new(value.Year, value.Month, 1, 0, 0, 0, value.Kind);

    public static DateTime StartOfYear(this DateTime value)
        => new(value.Year, 1, 1, 0, 0, 0, value.Kind);
}
