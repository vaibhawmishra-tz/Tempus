namespace Tempus.Business.Models;

/// <summary>
/// Represents a single public holiday returned by an <see cref="Tempus.Business.Abstractions.IHolidayProvider"/>.
/// </summary>
public sealed class Holiday
{
    /// <summary>The display name of the holiday (e.g. "Christmas Day").</summary>
    public required string Name { get; init; }

    /// <summary>The observed date of the holiday in the provider's region.</summary>
    public required DateOnly Date { get; init; }

    /// <summary>The region code this holiday belongs to, if sub-national (e.g. "CA-ON"). Null for national holidays.</summary>
    public string? Region { get; init; }

    /// <summary>Whether this holiday applies nationally rather than to a specific sub-region.</summary>
    public bool IsNational { get; init; } = true;

    /// <summary>Optional annotation (e.g. "observed" or "substitute for Sunday holiday").</summary>
    public string? Note { get; init; }
}
