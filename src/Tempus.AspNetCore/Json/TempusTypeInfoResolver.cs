using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http;
using Tempus.AspNetCore.Attributes;

namespace Tempus.AspNetCore.Json;

/// <summary>
/// Applies Tempus JSON converters to DTO properties annotated with
/// <see cref="UtcTimeAttribute"/>, <see cref="UserLocalTimeAttribute"/>, or
/// <see cref="RelativeTimeAttribute"/>. Registered automatically by <c>AddTempusJson()</c>.
/// </summary>
internal sealed class TempusTypeInfoResolver : IJsonTypeInfoResolver
{
    private readonly DefaultJsonTypeInfoResolver _inner = new();
    private readonly IHttpContextAccessor? _httpContextAccessor;

    internal TempusTypeInfoResolver(IHttpContextAccessor? httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo? info = _inner.GetTypeInfo(type, options);
        if (info?.Kind != JsonTypeInfoKind.Object) return info;

        foreach (JsonPropertyInfo prop in info.Properties)
        {
            if (prop.AttributeProvider is null) continue;

            object[] attrs = prop.AttributeProvider.GetCustomAttributes(false);
            foreach (object attr in attrs)
            {
                if (attr is UtcTimeAttribute)
                {
                    prop.CustomConverter = new UtcDateTimeJsonConverter();
                    break;
                }

                if (attr is UserLocalTimeAttribute)
                {
                    prop.CustomConverter = new UserLocalTimeJsonConverter(_httpContextAccessor);
                    break;
                }

                if (attr is RelativeTimeAttribute)
                {
                    prop.CustomConverter = new RelativeTimeJsonConverter(_httpContextAccessor);
                    break;
                }
            }
        }

        return info;
    }
}
