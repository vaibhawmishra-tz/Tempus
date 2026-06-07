namespace Tempus.AspNetCore.Context;

public sealed class TempusAspNetCoreOptions
{
    public string HeaderName { get; set; } = "X-Timezone";
    public string JwtClaimName { get; set; } = "timezone";
    public string QueryParamName { get; set; } = "tz";
    public string FallbackTimeZone { get; set; } = "UTC";
}
