using System.Reflection;
using NetArchTest.Rules;

namespace Crm.Architecture.Tests;

/// <summary>CLAUDE.md layering rules over a set of assemblies. Each returns the violations it finds.</summary>
internal static class LayeringRules
{
    public static IReadOnlyList<string> ControllersReferencingDbContextOrModels(IReadOnlyList<Assembly> assemblies) =>
        Failing(Types.InAssemblies(assemblies)
            .That().ResideInNamespaceEndingWith(".Controllers")
            .ShouldNot().HaveDependencyOnAny(["Microsoft.EntityFrameworkCore", .. NamespacesEndingWith(assemblies, ".Models")])
            .GetResult());

    public static IReadOnlyList<string> ServicesReferencingHttpContextOrMvc(IReadOnlyList<Assembly> assemblies) =>
        Failing(Types.InAssemblies(assemblies)
            .That().ResideInNamespaceEndingWith(".Services")
            .ShouldNot().HaveDependencyOnAny("Microsoft.AspNetCore.Http", "Microsoft.AspNetCore.Mvc")
            .GetResult());

    public static IReadOnlyList<string> PublicModels(IReadOnlyList<Assembly> assemblies) =>
        Failing(Types.InAssemblies(assemblies)
            .That().ResideInNamespaceEndingWith(".Models")
            .Should().NotBePublic()
            .GetResult());

    private static string[] NamespacesEndingWith(IReadOnlyList<Assembly> assemblies, string suffix) =>
        assemblies.SelectMany(a => a.GetTypes())
            .Select(t => t.Namespace)
            .Where(n => n is not null && n.EndsWith(suffix, StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray()!;

    private static List<string> Failing(TestResult result) => result.FailingTypeNames?.ToList() ?? [];
}
