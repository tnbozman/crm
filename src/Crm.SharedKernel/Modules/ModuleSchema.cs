namespace Crm.SharedKernel.Modules;

public static class ModuleSchema
{
    /// <summary>The module's SQL schema: its name in lower case ("Directory" → "directory").</summary>
    public static string Schema(this IModule module)
    {
        ArgumentNullException.ThrowIfNull(module);
#pragma warning disable CA1308 // SQL schema names are lower case by convention (docs/architecture.md §2); not a comparison.
        return module.Name.ToLowerInvariant();
#pragma warning restore CA1308
    }
}
