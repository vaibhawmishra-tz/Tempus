using Microsoft.Extensions.DependencyInjection;
using Tempus.Business.DependencyInjection;

namespace Tempus.Holidays.Exchange;

public static class HolidaysExchangeServiceCollectionExtensions
{
    /// <summary>
    /// Registers the NYSE trading holiday provider with the business calendar.
    /// Call after AddBusinessCalendar().
    /// </summary>
    public static IServiceCollection AddNyseHolidays(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddHolidayProvider<NyseHolidayProvider>();
    }
}
