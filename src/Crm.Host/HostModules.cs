using Crm.SharedKernel.Modules;

namespace Crm.Host;

/// <summary>
/// Every module the host composes. The scaffolder (make module-new) adds a line here;
/// Host_ModulesList_MatchesModuleProjects keeps it in step with src/Modules.
/// </summary>
internal static class HostModules
{
    public static IReadOnlyList<IModule> All { get; } = [];
}
