using Crm.SharedKernel.Modules;

namespace Crm.Migrator;

/// <summary>
/// Every module whose migrations the Migrator applies. The scaffolder (make module-new) adds a line here;
/// Migrator_ModulesList_MatchesModuleProjects keeps it in step with src/Modules.
/// </summary>
internal static class MigratorModules
{
    public static IReadOnlyList<IModule> All { get; } =
    [
        // make module-new adds modules above this line
    ];
}
