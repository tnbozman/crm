using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Platform.Modules;

/// <summary>
/// A module's registration with the host. Each module implements this once, in its implementation project,
/// and is listed in the host's module list. S1-3 adds mediator handler and outbox relay registration here.
/// </summary>
public interface IModule
{
    /// <summary>PascalCase module name, for example "Directory".</summary>
    string Name { get; }

    /// <summary>The assembly whose controllers the host adds with AddApplicationPart.</summary>
    Assembly ControllersAssembly { get; }

    /// <summary>Registers the module's services, DbContext and options.</summary>
    void AddServices(IServiceCollection services, IConfiguration configuration);
}
