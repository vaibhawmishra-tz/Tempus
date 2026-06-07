using System.Diagnostics.CodeAnalysis;
using Tempus.Core.Abstractions;

namespace Tempus.Core.Internal;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class AmbientAccessorInitializer : IAmbientAccessorInitializer
{
    public AmbientAccessorInitializer(ITimezoneResolver resolver)
        => TimezoneResolverAccessor.Set(resolver);
}
