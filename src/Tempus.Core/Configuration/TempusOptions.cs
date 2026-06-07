namespace Tempus.Core.Configuration;

public sealed class TempusOptions
{
    public string DefaultTimeZone { get; set; } = "UTC";
    public bool FallbackToUtc { get; set; } = true;
    public DstOptions Dst { get; set; } = new();
    public TimezoneOptions Timezone { get; set; } = new();
    public DisplayOptions Display { get; set; } = new();
}
