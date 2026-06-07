using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Tempus.EFCore.DependencyInjection;

public static class EFCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers Tempus EF Core services. Call this alongside <c>AddTempus()</c> and your
    /// <c>AddDbContext()</c> registration. In each <c>DbContext.OnModelCreating</c> call
    /// <see cref="Tempus.EFCore.Extensions.ModelBuilderExtensions.UseTemporalConventions"/> to
    /// apply value converters for <c>DateTime</c>, <c>DateTimeOffset</c>, <c>DateOnly</c>,
    /// and <c>TimeOnly</c> properties.
    /// </summary>
    public static IServiceCollection AddTempusEFCore(
        this IServiceCollection services,
        Action<TempusEFCoreOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new TempusEFCoreOptions();
        configure?.Invoke(opts);
        services.TryAddSingleton(opts);

        return services;
    }
}
