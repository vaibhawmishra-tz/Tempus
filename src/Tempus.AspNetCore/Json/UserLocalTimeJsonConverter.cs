using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Tempus.AspNetCore.Context;

namespace Tempus.AspNetCore.Json;

/// <summary>
/// Serialises <see cref="DateTimeOffset"/> values converted to the requesting user's local
/// timezone (resolved via <see cref="ITempusUserContext"/>). Falls back to UTC when no HTTP
/// context or user context is available. Requires <see cref="IHttpContextAccessor"/> from DI —
/// register this converter via <c>AddTempusJson()</c> rather than instantiating it directly.
/// </summary>
public sealed class UserLocalTimeJsonConverter : JsonConverter<DateTimeOffset>
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    /// <summary>
    /// Creates an instance without an HTTP context accessor. Serialisation will fall back to UTC.
    /// </summary>
    public UserLocalTimeJsonConverter() { }

    /// <summary>
    /// Creates an instance backed by <paramref name="httpContextAccessor"/> so that per-request
    /// user timezone resolution works during serialisation.
    /// </summary>
    public UserLocalTimeJsonConverter(IHttpContextAccessor? httpContextAccessor)
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

        DateTimeOffset output = ctx?.ToUserTime(value) ?? value.ToUniversalTime();
        writer.WriteStringValue(output.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
    }
}
