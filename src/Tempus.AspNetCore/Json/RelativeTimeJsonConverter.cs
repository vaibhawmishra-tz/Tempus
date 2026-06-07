using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Tempus.AspNetCore.Context;

namespace Tempus.AspNetCore.Json;

/// <summary>
/// Serialises <see cref="DateTimeOffset"/> values as a human-readable relative string
/// such as <c>"2 hours ago"</c> or <c>"in 3 days"</c>, using the requesting user's timezone
/// (resolved via <see cref="ITempusUserContext"/>). Falls back to UTC ISO 8601 when no HTTP
/// context or user context is available. Requires <see cref="IHttpContextAccessor"/> from DI —
/// register this converter via <c>AddTempusJson()</c>.
/// </summary>
public sealed class RelativeTimeJsonConverter : JsonConverter<DateTimeOffset>
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    /// <summary>
    /// Creates an instance without an HTTP context accessor. Serialisation will fall back to UTC ISO 8601.
    /// </summary>
    public RelativeTimeJsonConverter() { }

    /// <summary>
    /// Creates an instance backed by <paramref name="httpContextAccessor"/> for per-request
    /// user timezone resolution.
    /// </summary>
    public RelativeTimeJsonConverter(IHttpContextAccessor? httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

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

        ITempusUserContext? ctx = _httpContextAccessor?.HttpContext?.RequestServices
            .GetService<ITempusUserContext>();

        string output = ctx?.ToRelativeString(value)
            ?? value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

        writer.WriteStringValue(output);
    }
}
