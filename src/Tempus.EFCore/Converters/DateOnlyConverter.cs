using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Tempus.EFCore.Converters;

/// <summary>
/// Converts <see cref="DateOnly"/> to <see cref="DateTime"/> for database storage
/// and back to <see cref="DateOnly"/> on read. The stored DateTime has a midnight
/// time component with <see cref="DateTimeKind.Utc"/>.
/// </summary>
public sealed class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyConverter()
        : base(
            d => d.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            d => DateOnly.FromDateTime(d))
    {
    }
}
