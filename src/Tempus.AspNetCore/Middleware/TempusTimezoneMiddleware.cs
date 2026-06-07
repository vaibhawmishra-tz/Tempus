using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tempus.AspNetCore.Context;

namespace Tempus.AspNetCore.Middleware;

/// <summary>
/// Resolves user timezone from the request and makes it available via ITempusUserContext.
/// Resolution order: X-Timezone header → JWT claim → query param → configured fallback.
/// </summary>
public sealed class TempusTimezoneMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TempusAspNetCoreOptions _options;

    public TempusTimezoneMiddleware(RequestDelegate next, IOptions<TempusAspNetCoreOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        string? timezoneId = ResolveTimezoneId(context);

        ITempusUserContext userContext = context.RequestServices
            .GetRequiredService<ITempusUserContextFactory>()
            .Create(timezoneId ?? _options.FallbackTimeZone);

        context.Items[TempusContextKeys.UserContext] = userContext;

        await _next(context).ConfigureAwait(false);
    }

    private string? ResolveTimezoneId(HttpContext context)
    {
        if (!string.IsNullOrEmpty(_options.HeaderName) &&
            context.Request.Headers.TryGetValue(_options.HeaderName, out var headerValue))
            return headerValue.ToString();

        if (!string.IsNullOrEmpty(_options.JwtClaimName) && context.User.Identity?.IsAuthenticated == true)
        {
            string? claim = context.User.FindFirst(_options.JwtClaimName)?.Value;
            if (!string.IsNullOrEmpty(claim)) return claim;
        }

        if (!string.IsNullOrEmpty(_options.QueryParamName) &&
            context.Request.Query.TryGetValue(_options.QueryParamName, out var queryValue))
            return queryValue.ToString();

        return null;
    }
}
