using System.Reflection;
using System.Runtime.CompilerServices;

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

    /// <summary>Contracts hold records (messages and DTOs), enums and static classes of constants only (ADR-009).</summary>
    public static IReadOnlyList<string> ContractsTypesThatAreNotMessagesDtosOrConstants(IReadOnlyList<ModuleUnderTest> modules) =>
        modules
            .SelectMany(m => m.Contracts.GetTypes())
            .Where(t => !t.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
            .Select(t => (Type: t, Problem: ContractsProblem(t)))
            .Where(x => x.Problem is not null)
            .Select(x => $"{x.Type.FullName} {x.Problem}")
            .ToList();

    private static string? ContractsProblem(Type type)
    {
        if (type.IsInterface)
        {
            return "is an interface";
        }

        if (type.IsEnum || IsRecord(type))
        {
            return null;
        }

        if (type.IsAbstract && type.IsSealed)
        {
            var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
            return members.All(member => member is FieldInfo { IsLiteral: true } or FieldInfo { IsInitOnly: true } or ConstructorInfo { IsStatic: true })
                ? null
                : "is a static class with members other than constants";
        }

        return type.IsValueType ? "is a struct, not a record" : "is a class, not a record";
    }

    private static bool IsRecord(Type type) =>
        type.GetMethod("<Clone>$") is not null
        || (type.IsValueType && type.GetMethod("PrintMembers", BindingFlags.NonPublic | BindingFlags.Instance)?.IsDefined(typeof(CompilerGeneratedAttribute)) == true);
}
