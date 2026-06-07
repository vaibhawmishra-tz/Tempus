using Microsoft.Extensions.DependencyInjection;
using Tempus.Business.DependencyInjection;

namespace Tempus.Holidays.India;

public static class HolidaysInServiceCollectionExtensions
{
    /// <summary>
    /// Registers the India national holiday provider with the business calendar.
    /// Call after AddBusinessCalendar().
    /// </summary>
    public static IServiceCollection AddInHolidays(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddHolidayProvider<InHolidayProvider>();
    }
}
