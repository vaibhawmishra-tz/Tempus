using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Tempus.EFCore.Converters;

/// <summary>
/// Converts <see cref="TimeOnly"/> to <see cref="TimeSpan"/> for database storage
/// and back to <see cref="TimeOnly"/> on read.
/// </summary>
public sealed class TimeOnlyConverter : ValueConverter<TimeOnly, TimeSpan>
{
    public TimeOnlyConverter()
        : base(
            t => t.ToTimeSpan(),
            t => TimeOnly.FromTimeSpan(t))
    {
    }
}
