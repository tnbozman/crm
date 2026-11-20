using System.Reflection;
using Crm.__Module__.Handlers;
using Crm.__Module__.Persistence;
using Crm.__Module__.Services;
using Crm.Mediator;
using Crm.SharedKernel.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Crm.__Module__;

/// <summary>The __Module__ module's registration with the host (the only public type in the module, ADR-012).</summary>
public sealed class __Module__Module : IModule
{
    public string Name => "__Module__";

    public Assembly ControllersAssembly => typeof(__Module__Module).Assembly;

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.Add__Module__DbContext(configuration);
        services.AddScoped<IItemService, ItemService>();
        services.AddRequestHandler<Get__Module__ItemSummaryHandler>();
    }
}
