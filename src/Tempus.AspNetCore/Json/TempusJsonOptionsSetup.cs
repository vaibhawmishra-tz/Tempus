using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Tempus.AspNetCore.Json;

/// <summary>
/// Wires Tempus JSON converters into ASP.NET Core MVC's <see cref="JsonOptions"/> via DI.
/// Adds <see cref="UtcDateTimeJsonConverter"/> as the global default for
/// <see cref="DateTimeOffset"/> serialisation, and installs a type-info resolver modifier
/// that activates <see cref="UserLocalTimeJsonConverter"/> and
/// <see cref="RelativeTimeJsonConverter"/> for properties annotated with the corresponding
/// Tempus attributes.
/// </summary>
internal sealed class TempusJsonOptionsSetup : IConfigureOptions<JsonOptions>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal TempusJsonOptionsSetup(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Configure(JsonOptions options)
    {
        // Attribute-driven per-property converters
        options.JsonSerializerOptions.TypeInfoResolverChain.Insert(
            0,
            new TempusTypeInfoResolver(_httpContextAccessor));

        // Global default: all DateTimeOffset values are serialised as UTC
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
    }
}
