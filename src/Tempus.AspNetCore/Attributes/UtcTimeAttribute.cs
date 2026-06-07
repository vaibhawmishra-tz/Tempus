namespace Tempus.AspNetCore.Attributes;

/// <summary>
/// Always serializes a DateTimeOffset DTO property as UTC ISO 8601,
/// regardless of the requesting user's timezone. Use for system/audit timestamps.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class UtcTimeAttribute : Attribute
{
    public string Format { get; set; } = "yyyy-MM-ddTHH:mm:ssZ";
}
