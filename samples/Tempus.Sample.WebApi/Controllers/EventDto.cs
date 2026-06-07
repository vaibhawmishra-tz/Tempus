using Tempus.AspNetCore.Attributes;

namespace Tempus.Sample.WebApi.Controllers;

internal sealed class EventDto
{
    public required string Title { get; init; }

    /// <summary>Always serialized as UTC "yyyy-MM-ddTHH:mm:ssZ".</summary>
    [UtcTime]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Serialized in the caller's timezone (X-Timezone header / ?tz= / JWT claim).</summary>
    [UserLocalTime]
    public DateTimeOffset StartsAt { get; init; }

    /// <summary>Serialized as a human-readable relative string, e.g. "15 minutes ago".</summary>
    [RelativeTime]
    public DateTimeOffset UpdatedAt { get; init; }
}
