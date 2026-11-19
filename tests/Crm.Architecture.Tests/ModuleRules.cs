using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Crm.Mediator;
using Crm.SharedKernel.Modules;
using Microsoft.EntityFrameworkCore;

namespace Crm.Architecture.Tests;

/// <summary>Module boundary rules (CLAUDE.md, ADR-009, ADR-013). Each returns the violations it finds, one string per violation.</summary>
internal static class ModuleRules
{
    /// <summary>A module project references only the shared kernel and the mediator (ADR-013), never another module.</summary>
    public static IReadOnlyList<string> ModuleProjectReferenceProblems(ModuleSet set) =>
        set.ProjectFiles
            .SelectMany(project => XDocument.Load(project).Descendants("ProjectReference")
                .Select(r => Path.GetFileNameWithoutExtension(((string)r.Attribute("Include")!).Replace('\\', '/')))
                .Where(reference => !set.AllowedReferences.Contains(reference))
                .Select(reference => $"{Path.GetFileNameWithoutExtension(project)} references {reference}"))
            .ToList();

    /// <summary>Messages under the contracts namespace are records, enums and static classes of constants only (ADR-009).</summary>
    public static IReadOnlyList<string> ContractsTypesThatAreNotMessagesDtosOrConstants(ModuleSet set) =>
        set.ContractsAssembly.GetTypes()
            .Where(t => IsUnder(t.Namespace, set.ContractsRoot) && !t.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
            .Select(t => (Type: t, Problem: ContractsProblem(t)))
            .Where(x => x.Problem is not null)
            .Select(x => $"{x.Type.FullName} {x.Problem}")
            .ToList();

    /// <summary>
    /// Implementation types are internal, except the IModule class, which the host creates. Controllers and DTOs
    /// are internal too; the shared kernel discovers internal controllers in module assemblies (ADR-012).
    /// </summary>
    public static IReadOnlyList<string> PublicImplementationTypes(IReadOnlyList<ModuleUnderTest> modules) =>
        modules
            .SelectMany(m => m.Implementation.GetExportedTypes())
            .Where(t => !typeof(IModule).IsAssignableFrom(t))
            .Select(t => t.FullName!)
            .ToList();

    /// <summary>Each module has exactly one DbContext, and its default schema is the module schema.</summary>
    public static IReadOnlyList<string> DbContextProblems(IReadOnlyList<ModuleUnderTest> modules)
    {
        var problems = new List<string>();

        foreach (var module in modules)
        {
            var contexts = module.Implementation.GetTypes()
                .Where(t => typeof(DbContext).IsAssignableFrom(t) && !t.IsAbstract)
                .OrderBy(t => t.Name, StringComparer.Ordinal)
                .ToList();

            if (contexts.Count != 1)
            {
                problems.Add($"{module.Name} has {contexts.Count} DbContexts: {string.Join(", ", contexts.Select(t => t.Name))}");
            }

            var expected = module.Module.Schema();
            problems.AddRange(contexts
                .Select(t => (Type: t, Schema: DbContextModels.Load(t).GetDefaultSchema()))
                .Where(x => x.Schema != expected)
                .Select(x => $"{x.Type.FullName} has default schema '{x.Schema}', expected '{expected}'"));
        }

        return problems;
    }

    /// <summary>
    /// Each request under a module's contracts namespace has exactly one handler, in that module's library (ADR-013).
    /// </summary>
    public static IReadOnlyList<string> RequestHandlerProblems(ModuleSet set)
    {
        var handlers = set.Implementations
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => (Request: i.GetGenericArguments()[0], Handler: t)))
            .ToLookup(x => x.Request, x => x.Handler);

        var problems = new List<string>();
        foreach (var module in set.Modules)
        {
            var requests = set.ContractsAssembly.GetTypes()
                .Where(t => IsUnder(t.Namespace, module.ContractsNamespace) && !t.IsAbstract
                    && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)));

            foreach (var request in requests)
            {
                var found = handlers[request].ToList();
                if (found.Count != 1)
                {
                    problems.Add(found.Count == 0 ? $"{request.FullName} has no handler" : $"{request.FullName} has {found.Count} handlers");
                }

                problems.AddRange(found
                    .Where(h => h.Assembly != module.Implementation)
                    .Select(h => $"{request.FullName} is handled by {h.FullName}, outside {module.Implementation.GetName().Name}"));
            }
        }

        return problems;
    }

    private static bool IsUnder(string? typeNamespace, string root) =>
        typeNamespace is not null && (typeNamespace == root || typeNamespace.StartsWith(root + ".", StringComparison.Ordinal));

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
