using System.Diagnostics.CodeAnalysis;
using Tempus.Business.Abstractions;

namespace Tempus.Business.Sla;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by SlaTimerFactory")]
internal sealed class SlaTimer : ISlaTimer
{
    private readonly IBusinessCalendar _calendar;
    private readonly Func<DateTimeOffset> _utcNow;

    internal SlaTimer(
        DateTimeOffset startedAt,
        TimeSpan target,
        IBusinessCalendar calendar,
        Func<DateTimeOffset> utcNow)
    {
        StartedAt = startedAt;
        Target = target;
        _calendar = calendar;
        _utcNow = utcNow;
    }

    public DateTimeOffset StartedAt { get; }
    public TimeSpan Target { get; }

    public TimeSpan ElapsedBusinessTime => _calendar.BusinessTimeBetween(StartedAt, _utcNow());
    public TimeSpan RemainingBusinessTime => Target - ElapsedBusinessTime;
    public bool IsBreached => ElapsedBusinessTime >= Target;
    public DateTimeOffset BreachesAt => _calendar.AddBusinessHours(StartedAt, Target.TotalHours);
}
