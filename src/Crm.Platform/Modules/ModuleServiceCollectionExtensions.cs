using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Platform.Modules;

public static class ModuleServiceCollectionExtensions
{
    /// <summary>Composes every module into the host: its services, and its controllers through AddApplicationPart.</summary>
    public static IServiceCollection AddModules(this IServiceCollection services, ModuleRegistry registry, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(registry);

        services.AddSingleton(registry);
        var mvc = services.AddControllers()
            .ConfigureApplicationPartManager(parts => parts.FeatureProviders.Add(
                new ModuleControllerFeatureProvider(registry.Modules.Select(m => m.ControllersAssembly).ToHashSet())));
        foreach (var module in registry.Modules)
        {
            module.AddServices(services, configuration);
            mvc.AddApplicationPart(module.ControllersAssembly);
        }

        RequestHandlerValidation.ThrowIfInvalid(services, registry);

        return services;
    }
}
