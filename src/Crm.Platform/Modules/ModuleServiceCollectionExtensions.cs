using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Platform.Modules;

public static class ModuleServiceCollectionExtensions
{
    /// <summary>Composes every module in the registry into the host.</summary>
    public static IServiceCollection AddModules(this IServiceCollection services, ModuleRegistry registry, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(registry);

        foreach (var module in registry.Modules)
        {
            module.AddServices(services, configuration);
        }

        return services;
    }
}
