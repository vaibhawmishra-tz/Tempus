using System.Diagnostics.CodeAnalysis;
using Tempus.Business.Abstractions;
using Tempus.Core.Abstractions;

namespace Tempus.Business.Sla;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class SlaTimerFactory : ISlaTimerFactory
{
    private readonly IBusinessCalendar _calendar;
    private readonly ITempusClock _clock;

    public SlaTimerFactory(IBusinessCalendar calendar, ITempusClock clock)
    {
        _calendar = calendar;
        _clock = clock;
    }

    public ISlaTimer Create(DateTimeOffset startedAt, TimeSpan target)
        => new SlaTimer(startedAt, target, _calendar, () => _clock.UtcNow);

    public ISlaTimer CreateNow(TimeSpan target)
        => Create(_clock.UtcNow, target);
}
