namespace Tempus.Core.Configuration;

public sealed class TimezoneOptions
{
    public TimezoneSource Source { get; set; } = TimezoneSource.Bundled;
    public bool AcceptWindowsIds { get; set; } = true;
    public bool WarnOnWindowsIds { get; set; } = true;
    public MissingZoneStrategy OnMissingZone { get; set; } = MissingZoneStrategy.FallbackToUtc;
}
