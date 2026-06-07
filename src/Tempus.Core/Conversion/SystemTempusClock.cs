using System.Diagnostics.CodeAnalysis;
using Tempus.Core.Abstractions;

namespace Tempus.Core.Conversion;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class SystemTempusClock : ITempusClock
{
    private readonly ITimezoneResolver _resolver;

    public SystemTempusClock(ITimezoneResolver resolver) => _resolver = resolver;

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    public DateOnly TodayUtc => DateOnly.FromDateTime(DateTime.UtcNow);

    public DateTimeOffset NowIn(string ianaTimeZoneId)
    {
        TimeZoneInfo tz = _resolver.Resolve(ianaTimeZoneId);
        return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
    }

    public DateOnly TodayIn(string ianaTimeZoneId)
        => DateOnly.FromDateTime(NowIn(ianaTimeZoneId).DateTime);
}
