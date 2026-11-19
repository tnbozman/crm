using System.Reflection;
using System.Runtime.CompilerServices;
using Crm.Mediator;
using Crm.SharedKernel.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

    /// <summary>
    /// Implementation types are internal, except the IModule class, which the host creates. Controllers and DTOs
    /// are internal too; the platform discovers internal controllers in module assemblies.
    /// </summary>
    public static IReadOnlyList<string> PublicImplementationTypes(IReadOnlyList<ModuleUnderTest> modules) =>
        modules
            .SelectMany(m => m.Implementation.GetExportedTypes())
            .Where(t => !typeof(IModule).IsAssignableFrom(t))
            .Select(t => t.FullName!)
            .ToList();

    /// <summary>
    /// A module's AddServices registers nothing from another module's implementation, as a service type,
    /// an implementation type or a constructor dependency. Only Contracts types and the mediator cross modules.
    /// </summary>
    public static IReadOnlyList<string> RegistrationsOfOtherModulesTypes(IReadOnlyList<ModuleUnderTest> modules)
    {
        var configuration = new ConfigurationBuilder().Build();
        var violations = new List<string>();

        foreach (var module in modules)
        {
            var others = modules.Where(o => o != module).Select(o => o.Implementation).ToHashSet();
            var services = new ServiceCollection();
            module.Module.AddServices(services, configuration);

            var referenced = services
                .SelectMany(d => new[] { d.ServiceType, d.ImplementationType }
                    .Concat(d.ImplementationType?.GetConstructors().SelectMany(c => c.GetParameters()).Select(p => p.ParameterType) ?? []))
                .OfType<Type>()
                .Where(t => others.Contains(t.Assembly))
                .Select(t => $"{module.Name} registers {t.FullName}")
                .Distinct(StringComparer.Ordinal);

            violations.AddRange(referenced);
        }

        return violations;
    }

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

    /// <summary>Each request in a module's Contracts has exactly one handler, in that module's implementation (ADR-009).</summary>
    public static IReadOnlyList<string> RequestHandlerProblems(IReadOnlyList<ModuleUnderTest> modules)
    {
        var handlers = modules
            .SelectMany(m => m.Implementation.GetTypes())
            .Where(t => !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => (Request: i.GetGenericArguments()[0], Handler: t)))
            .ToLookup(x => x.Request, x => x.Handler);

        var problems = new List<string>();
        foreach (var module in modules)
        {
            var requests = module.Contracts.GetTypes()
                .Where(t => !t.IsAbstract && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)));

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
}
