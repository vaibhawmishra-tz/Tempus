using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tempus.AspNetCore.Json;

/// <summary>
/// Serialises <see cref="DateTimeOffset"/> values as UTC ISO 8601 strings
/// (<c>"yyyy-MM-ddTHH:mm:ssZ"</c>) regardless of the original offset.
/// Deserialises any ISO 8601 string back to a <see cref="DateTimeOffset"/>.
/// </summary>
public sealed class UtcDateTimeJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => DateTimeOffset.Parse(
            reader.GetString()!,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    public override void Write(
        Utf8JsonWriter writer,
        DateTimeOffset value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(
            value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
    }
}
