using Microsoft.Extensions.DependencyInjection;
using Tempus.Business.DependencyInjection;

namespace Tempus.Holidays.US;

public static class HolidaysUsServiceCollectionExtensions
{
    /// <summary>
    /// Registers the US Federal holiday provider with the business calendar.
    /// Call after AddBusinessCalendar().
    /// </summary>
    public static IServiceCollection AddUsHolidays(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddHolidayProvider<UsHolidayProvider>();
    }
}
