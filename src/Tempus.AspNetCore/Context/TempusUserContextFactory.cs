using Tempus.Core.Abstractions;

namespace Tempus.AspNetCore.Context;

internal sealed class TempusUserContextFactory : ITempusUserContextFactory
{
    private readonly ITempusClock _clock;
    private readonly ITimezoneResolver _resolver;

    internal TempusUserContextFactory(ITempusClock clock, ITimezoneResolver resolver)
    {
        _clock = clock;
        _resolver = resolver;
    }

    public ITempusUserContext Create(string ianaTimeZoneId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ianaTimeZoneId);
        TimeZoneInfo tz = _resolver.Resolve(ianaTimeZoneId);
        return new TempusUserContext(ianaTimeZoneId, tz, () => _clock.UtcNow);
    }
}
