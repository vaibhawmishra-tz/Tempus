namespace Tempus.AspNetCore.Attributes;

/// <summary>
/// Applied to DateTimeOffset DTO properties. The JSON serializer converts
/// the UTC value to the requesting user's local timezone before writing.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class UserLocalTimeAttribute : Attribute
{
    public string? Format { get; set; }
}
