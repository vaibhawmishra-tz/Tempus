namespace Tempus.Core.Configuration;

public sealed class DisplayOptions
{
    public string DefaultFormat { get; set; } = "yyyy-MM-dd HH:mm:ss zzz";
    public string ShortDateFormat { get; set; } = "MMM dd, yyyy";
    public string TimeOnlyFormat { get; set; } = "h:mm tt zzz";
    public TimeSpan RelativeTimeThreshold { get; set; } = TimeSpan.FromDays(7);
    public string Locale { get; set; } = "en-US";
}
