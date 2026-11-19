using System.Text.RegularExpressions;

namespace Crm.SharedKernel.Modules;

/// <summary>The validated set of modules the host composes.</summary>
public sealed partial class ModuleRegistry
{
    public ModuleRegistry(IEnumerable<IModule> modules)
    {
        ArgumentNullException.ThrowIfNull(modules);
        Modules = modules.ToList();

        var badName = Modules.FirstOrDefault(m => !PascalCaseLetters().IsMatch(m.Name));
        if (badName is not null)
        {
            throw new InvalidOperationException($"Module name '{badName.Name}' must be PascalCase letters only (it names the schema).");
        }

        var duplicate = Modules.GroupBy(m => m.Name, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Module '{duplicate.Key}' is registered more than once.");
        }
    }

    public IReadOnlyList<IModule> Modules { get; }

    [GeneratedRegex("^[A-Z][A-Za-z]*$")]
    private static partial Regex PascalCaseLetters();
}
