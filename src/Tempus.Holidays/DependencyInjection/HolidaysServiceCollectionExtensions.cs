using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Tempus.Business.Abstractions;
using Tempus.Holidays.Internal;

namespace Tempus.Holidays;

public static class HolidaysServiceCollectionExtensions
{
    private static readonly Lazy<IReadOnlyDictionary<string, HolidayDataFile>> _registry
        = new(LoadRegistry);

    /// <summary>
    /// Registers <see cref="IHolidayProvider"/> instances for the given region codes
    /// (e.g. "CA", "CA-ON", "AU", "DE", "JP").
    /// </summary>
    public static IServiceCollection AddHolidays(
        this IServiceCollection services,
        params string[] regions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(regions);

        IReadOnlyDictionary<string, HolidayDataFile> registry = _registry.Value;

        foreach (string region in regions)
        {
            if (!registry.TryGetValue(region, out HolidayDataFile? data))
            {
                string available = string.Join(", ", registry.Keys.OrderBy(k => k, StringComparer.Ordinal));
                throw new InvalidOperationException(
                    $"No holiday data found for region '{region}'. Available: {available}");
            }

            services.AddSingleton<IHolidayProvider>(new EmbeddedHolidayProvider(data));
        }

        return services;
    }

    private static Dictionary<string, HolidayDataFile> LoadRegistry()
    {
        var assembly = typeof(HolidaysServiceCollectionExtensions).Assembly;
        string prefix = $"{assembly.GetName().Name}.Data.";
        var result = new Dictionary<string, HolidayDataFile>(StringComparer.OrdinalIgnoreCase);

        foreach (string name in assembly.GetManifestResourceNames())
        {
            if (!name.StartsWith(prefix, StringComparison.Ordinal) ||
                !name.EndsWith(".json", StringComparison.Ordinal))
                continue;

            using Stream stream = assembly.GetManifestResourceStream(name)!;
            HolidayDataFile? data = JsonSerializer.Deserialize<HolidayDataFile>(stream);
            if (data is not null)
                result[data.Region] = data;
        }

        return result;
    }
}
