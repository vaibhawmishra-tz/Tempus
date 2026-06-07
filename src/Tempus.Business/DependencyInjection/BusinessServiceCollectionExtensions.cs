using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Tempus.Business.Abstractions;
using Tempus.Business.Calendar;
using Tempus.Business.Sla;

namespace Tempus.Business.DependencyInjection;

public static class BusinessServiceCollectionExtensions
{
    /// <summary>
    /// Registers IBusinessCalendar and its dependencies. Call AddHolidayProvider() to plug in region-specific holidays.
    /// </summary>
    public static IServiceCollection AddBusinessCalendar(
        this IServiceCollection services,
        Action<BusinessCalendarOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
            services.Configure(configure);

        services.TryAddSingleton<IBusinessCalendar>(sp =>
        {
            var opts = sp.GetService<IOptions<BusinessCalendarOptions>>()?.Value
                       ?? new BusinessCalendarOptions();
            var providers = sp.GetServices<IHolidayProvider>();
            return new BusinessCalendar(opts, providers);
        });

        return services;
    }

    /// <summary>
    /// Registers a holiday provider. Multiple providers can be added — all are consulted when checking IsHoliday.
    /// </summary>
    public static IServiceCollection AddHolidayProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IHolidayProvider
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IHolidayProvider, TProvider>();
        return services;
    }

    /// <summary>
    /// Registers a holiday provider instance.
    /// </summary>
    public static IServiceCollection AddHolidayProvider(
        this IServiceCollection services,
        IHolidayProvider provider)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(provider);
        services.AddSingleton(provider);
        return services;
    }

    /// <summary>
    /// Registers ISlaTimerFactory. Requires AddBusinessCalendar() and ITempusClock (via AddTempus()) first.
    /// </summary>
    public static IServiceCollection AddSlaTimer(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<ISlaTimerFactory, SlaTimerFactory>();
        return services;
    }

    /// <summary>
    /// Registers FiscalCalendar as a singleton with optional configuration.
    /// </summary>
    public static IServiceCollection AddFiscalCalendar(
        this IServiceCollection services,
        Action<FiscalCalendarOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (configure is not null)
            services.Configure(configure);
        services.TryAddSingleton<FiscalCalendar>(sp =>
        {
            var opts = sp.GetService<IOptions<FiscalCalendarOptions>>()?.Value
                       ?? new FiscalCalendarOptions();
            return new FiscalCalendar(opts);
        });
        return services;
    }
}
