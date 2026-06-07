using Tempus.Core.Abstractions;

namespace Tempus.Core.Internal;

/// <summary>
/// Ambient accessor so extension methods can reach the configured resolver
/// without requiring every call site to carry an injected dependency.
/// Set once at startup via DI; not intended for direct use by consumers.
/// </summary>
internal static class TimezoneResolverAccessor
{
    private static ITimezoneResolver? _resolver;

    internal static ITimezoneResolver Resolver =>
        _resolver ?? throw new InvalidOperationException(
            "Tempus has not been configured. Call services.AddTempus() in your DI setup.");

    internal static void Set(ITimezoneResolver resolver) => _resolver = resolver;
}
