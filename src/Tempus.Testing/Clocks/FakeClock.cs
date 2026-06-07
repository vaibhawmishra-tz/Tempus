using Tempus.Core.Abstractions;

namespace Tempus.Testing.Clocks;

/// <summary>
/// Controllable clock for tests. Time does not advance automatically.
/// Use Advance() or SetTo() to move time forward/backward.
/// </summary>
public sealed class FakeClock : ITempusClock
{
    private DateTimeOffset _current;
    private readonly ITimezoneResolver _resolver;

    public FakeClock(DateTimeOffset fixedTime, ITimezoneResolver? resolver = null)
    {
        _current = fixedTime;
        _resolver = resolver ?? new PassthroughTimezoneResolver();
    }

    public DateTimeOffset UtcNow => _current.ToUniversalTime();
    public DateOnly TodayUtc => DateOnly.FromDateTime(_current.UtcDateTime);

    public DateTimeOffset NowIn(string ianaTimeZoneId)
    {
        TimeZoneInfo tz = _resolver.Resolve(ianaTimeZoneId);
        return TimeZoneInfo.ConvertTime(_current.ToUniversalTime(), tz);
    }

    public DateOnly TodayIn(string ianaTimeZoneId)
        => DateOnly.FromDateTime(NowIn(ianaTimeZoneId).DateTime);

    public void Advance(TimeSpan by) => _current = _current.Add(by);
    public void SetTo(DateTimeOffset time) => _current = time;
    public void AdvanceDays(int days) => Advance(TimeSpan.FromDays(days));
    public void AdvanceHours(double hours) => Advance(TimeSpan.FromHours(hours));

    private sealed class PassthroughTimezoneResolver : ITimezoneResolver
    {
        public TimeZoneInfo Resolve(string id) => TimeZoneInfo.FindSystemTimeZoneById(id);
        public bool TryResolve(string id, out TimeZoneInfo? tz)
        {
            try { tz = TimeZoneInfo.FindSystemTimeZoneById(id); return true; }
            catch (TimeZoneNotFoundException) { tz = null; return false; }
        }
        public string ToIanaId(string id) => id;
        public string ToWindowsId(string id) => id;
        public IReadOnlyList<string> AllIanaIds => [];
    }
}
