using Tempus.Core.Abstractions;

namespace Tempus.Core.Extensions;

public static class TempusClockExtensions
{
    /// <summary>
    /// Returns midnight UTC on the given calendar date as a <see cref="DateTimeOffset"/>.
    /// Useful when you have a <see cref="DateOnly"/> and need a UTC instant for storage or comparison.
    /// </summary>
    public static DateTimeOffset StartOfDayUtc(this ITempusClock _, DateOnly date)
        => new(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);
}
