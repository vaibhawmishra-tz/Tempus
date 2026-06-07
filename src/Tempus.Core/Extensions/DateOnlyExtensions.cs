using System.Runtime.CompilerServices;

namespace Tempus.Core.Extensions;

public static class DateOnlyExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWeekend(this DateOnly date)
        => date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWeekday(this DateOnly date) => !date.IsWeekend();

    public static DateOnly StartOfWeek(this DateOnly date, DayOfWeek firstDay = DayOfWeek.Monday)
    {
        int diff = ((int)date.DayOfWeek - (int)firstDay + 7) % 7;
        return date.AddDays(-diff);
    }

    public static DateOnly EndOfWeek(this DateOnly date, DayOfWeek firstDay = DayOfWeek.Monday)
        => date.StartOfWeek(firstDay).AddDays(6);

    public static DateOnly StartOfMonth(this DateOnly date) => new(date.Year, date.Month, 1);

    public static DateOnly EndOfMonth(this DateOnly date)
        => new(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

    public static DateOnly StartOfYear(this DateOnly date) => new(date.Year, 1, 1);
    public static DateOnly EndOfYear(this DateOnly date) => new(date.Year, 12, 31);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInPast(this DateOnly date) => date < DateOnly.FromDateTime(DateTime.UtcNow);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInFuture(this DateOnly date) => date > DateOnly.FromDateTime(DateTime.UtcNow);

    public static int Age(this DateOnly birthDate)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        int age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age)) age--;
        return age;
    }
}
