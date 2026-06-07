using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Tempus.Holidays.Internal;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by System.Text.Json")]
internal sealed class HolidayDataFile
{
    [JsonPropertyName("region")] public string       Region         { get; init; } = "";
    [JsonPropertyName("name")]   public string       Name           { get; init; } = "";
    /// <summary>"japan" enables Sunday-substitute post-processing.</summary>
    [JsonPropertyName("substituteRule")] public string? SubstituteRule { get; init; }
    [JsonPropertyName("rules")]  public HolidayRule[] Rules          { get; init; } = [];
}
