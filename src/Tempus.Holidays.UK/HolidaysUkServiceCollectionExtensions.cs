using Microsoft.Extensions.DependencyInjection;
using Tempus.Business.DependencyInjection;

namespace Tempus.Holidays.UK;

public static class HolidaysUkServiceCollectionExtensions
{
    /// <summary>
    /// Registers the England and Wales public holiday provider with the business calendar.
    /// Call after AddBusinessCalendar().
    /// </summary>
    public static IServiceCollection AddUkHolidays(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddHolidayProvider<UkHolidayProvider>();
    }
}
