namespace Tempus.EFCore.DependencyInjection;

/// <summary>
/// Configuration options for Tempus EF Core integration.
/// </summary>
public sealed class TempusEFCoreOptions
{
    /// <summary>
    /// When <c>true</c>, <see cref="Tempus.EFCore.Extensions.ModelBuilderExtensions.UseTemporalConventions"/>
    /// enforces UTC storage for <see cref="DateTime"/> and <see cref="DateTimeOffset"/> properties.
    /// Default: <c>true</c>.
    /// </summary>
    public bool EnforceDateTimeUtc { get; set; } = true;
}
