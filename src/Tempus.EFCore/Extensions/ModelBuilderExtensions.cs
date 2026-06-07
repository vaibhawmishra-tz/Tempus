using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tempus.EFCore.Converters;

namespace Tempus.EFCore.Extensions;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies UTC enforcement to all DateTime and DateTimeOffset properties
    /// in the model. DateTime values are stored and retrieved with Kind = Utc.
    /// Call this once in OnModelCreating.
    /// </summary>
    public static ModelBuilder EnforceUtcDateTimes(this ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v : v.Value.ToUniversalTime()) : null,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(dateTimeConverter);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(nullableDateTimeConverter);
            }
        }

        return builder;
    }

    /// <summary>
    /// Registers value converters for all temporal CLR types in the model:
    /// <list type="bullet">
    ///   <item><see cref="DateTime"/> (and nullable) — stored as UTC</item>
    ///   <item><see cref="DateTimeOffset"/> (and nullable) — stored as UTC offset</item>
    ///   <item><see cref="DateOnly"/> (and nullable) — stored as <see cref="DateTime"/> (midnight UTC)</item>
    ///   <item><see cref="TimeOnly"/> (and nullable) — stored as <see cref="TimeSpan"/></item>
    /// </list>
    /// Call this once in <c>OnModelCreating</c>.
    /// </summary>
    public static ModelBuilder UseTemporalConventions(this ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v : v.Value.ToUniversalTime()) : null,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        var dateTimeOffsetConverter = new ValueConverter<DateTimeOffset, DateTimeOffset>(
            v => v.ToUniversalTime(),
            v => v);

        var nullableDateTimeOffsetConverter = new ValueConverter<DateTimeOffset?, DateTimeOffset?>(
            v => v.HasValue ? v.Value.ToUniversalTime() : null,
            v => v);

        var dateOnlyConverter = new DateOnlyConverter();

        var nullableDateOnlyConverter = new ValueConverter<DateOnly?, DateTime?>(
            d => d.HasValue ? d.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) : (DateTime?)null,
            d => d.HasValue ? (DateOnly?)DateOnly.FromDateTime(d.Value) : null);

        var timeOnlyConverter = new TimeOnlyConverter();

        var nullableTimeOnlyConverter = new ValueConverter<TimeOnly?, TimeSpan?>(
            t => t.HasValue ? t.Value.ToTimeSpan() : (TimeSpan?)null,
            t => t.HasValue ? (TimeOnly?)TimeOnly.FromTimeSpan(t.Value) : null);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(dateTimeConverter);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(nullableDateTimeConverter);
                else if (property.ClrType == typeof(DateTimeOffset))
                    property.SetValueConverter(dateTimeOffsetConverter);
                else if (property.ClrType == typeof(DateTimeOffset?))
                    property.SetValueConverter(nullableDateTimeOffsetConverter);
                else if (property.ClrType == typeof(DateOnly))
                    property.SetValueConverter(dateOnlyConverter);
                else if (property.ClrType == typeof(DateOnly?))
                    property.SetValueConverter(nullableDateOnlyConverter);
                else if (property.ClrType == typeof(TimeOnly))
                    property.SetValueConverter(timeOnlyConverter);
                else if (property.ClrType == typeof(TimeOnly?))
                    property.SetValueConverter(nullableTimeOnlyConverter);
            }
        }

        return builder;
    }
}
