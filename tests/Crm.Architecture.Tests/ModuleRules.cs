namespace Crm.Architecture.Tests;

/// <summary>Module boundary rules (CLAUDE.md, ADR-009). Each returns the violations it finds, one string per violation.</summary>
internal static class ModuleRules
{
    public static IReadOnlyList<string> ImplementationReferencesToOtherModules(IReadOnlyList<ModuleUnderTest> modules)
    {
        var implementations = modules.Select(m => m.Implementation.GetName().Name!).ToHashSet(StringComparer.Ordinal);

        return modules
            .SelectMany(m => m.Implementation.GetReferencedAssemblies()
                .Select(r => r.Name!)
                .Where(implementations.Contains)
                .Select(r => $"{m.Implementation.GetName().Name} references {r}"))
            .ToList();
    }

    public static IReadOnlyList<string> ContractsReferencesToEfCore(IReadOnlyList<ModuleUnderTest> modules) =>
        modules
            .SelectMany(m => m.Contracts.GetReferencedAssemblies()
                .Select(r => r.Name!)
                .Where(r => r.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal))
                .Select(r => $"{m.Contracts.GetName().Name} references {r}"))
            .ToList();
}
