namespace Tempus.AspNetCore.Attributes;

/// <summary>
/// Serializes a DateTimeOffset DTO property as a relative time string
/// ("2 hours ago", "in 3 days") based on the requesting user's timezone.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RelativeTimeAttribute : Attribute { }
