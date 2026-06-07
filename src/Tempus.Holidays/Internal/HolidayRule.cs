using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Tempus.Holidays.Internal;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by System.Text.Json")]
internal sealed class HolidayRule
{
    [JsonPropertyName("name")]    public string  Name    { get; init; } = "";
    /// <summary>fixed | easter | nth | last | last-before | boxing-day | equinox</summary>
    [JsonPropertyName("type")]    public string  Type    { get; init; } = "";
    [JsonPropertyName("month")]   public int     Month   { get; init; }
    [JsonPropertyName("day")]     public int     Day     { get; init; }
    /// <summary>Days offset from Easter Sunday (may be negative).</summary>
    [JsonPropertyName("offset")]  public int     Offset  { get; init; }
    /// <summary>Nth occurrence for the "nth" rule type.</summary>
    [JsonPropertyName("n")]       public int     N       { get; init; }
    /// <summary>Day-of-week name: Monday, Tuesday … Sunday</summary>
    [JsonPropertyName("weekday")] public string  Weekday { get; init; } = "";
    /// <summary>For equinox: "vernal" or "autumnal"</summary>
    [JsonPropertyName("which")]   public string  Which   { get; init; } = "";
    /// <summary>sat-mon-sun-mon | sat-fri-sun-mon | null (no shift)</summary>
    [JsonPropertyName("observe")] public string? Observe { get; init; }
    /// <summary>Holiday applies from this year onwards (inclusive).</summary>
    [JsonPropertyName("since")]   public int?    Since   { get; init; }
    /// <summary>Holiday applies up to and including this year.</summary>
    [JsonPropertyName("until")]   public int?    Until   { get; init; }
    [JsonPropertyName("note")]    public string? Note    { get; init; }
}
