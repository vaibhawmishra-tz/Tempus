using Microsoft.Extensions.DependencyInjection;

namespace Tempus.Core.DependencyInjection;

public sealed class TempusBuilder
{
    public IServiceCollection Services { get; }
    internal TempusBuilder(IServiceCollection services) => Services = services;
}
