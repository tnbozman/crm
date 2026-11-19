namespace Crm.Platform.Modules;

/// <summary>The validated set of modules the host composes.</summary>
public sealed class ModuleRegistry
{
    public ModuleRegistry(IEnumerable<IModule> modules)
    {
        ArgumentNullException.ThrowIfNull(modules);
        Modules = modules.ToList();

        var duplicate = Modules.GroupBy(m => m.Name, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Module '{duplicate.Key}' is registered more than once.");
        }
    }

    public IReadOnlyList<IModule> Modules { get; }
}
