using Microsoft.Extensions.DependencyInjection;
using Tempus.Business.DependencyInjection;

namespace Tempus.Holidays.EU;

public static class HolidaysEuServiceCollectionExtensions
{
    /// <summary>
    /// Registers the common EU public holiday provider with the business calendar.
    /// Call after AddBusinessCalendar().
    /// </summary>
    public static IServiceCollection AddEuHolidays(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddHolidayProvider<EuHolidayProvider>();
    }
}
