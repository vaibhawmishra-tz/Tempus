using Microsoft.AspNetCore.Builder;
using Tempus.AspNetCore.Middleware;
using Tempus.Core.DependencyInjection;

namespace Tempus.AspNetCore.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Tempus timezone resolution middleware to the pipeline.
    /// Resolves user timezone from X-Timezone header, JWT claim, or query param.
    /// Must be added before UseAuthentication if using JWT claim resolution.
    /// </summary>
    public static IApplicationBuilder UseTempusTimezone(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Force the ambient resolver initializer so that DateTimeOffset/DateTime
        // extension methods (ToZone, ToRelativeString, etc.) work throughout the app.
        app.ApplicationServices.EnsureTempusInitialized();

        return app.UseMiddleware<TempusTimezoneMiddleware>();
    }
}
