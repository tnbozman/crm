using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using NetArchTest.Rules;

namespace Crm.Architecture.Tests;

/// <summary>CLAUDE.md layering rules over a set of assemblies. Each returns the violations it finds.</summary>
internal static partial class LayeringRules
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

    /// <summary>
    /// Controllers take service interfaces (their module's Services, or Crm.Platform such as ICurrentUser) in their
    /// constructors, and DTOs or framework types (Guid, CancellationToken, ...) as action parameters.
    /// </summary>
    public static IReadOnlyList<string> ControllerDependenciesOtherThanServiceInterfacesAndDtos(IReadOnlyList<Assembly> assemblies)
    {
        var controllers = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();

        var constructorProblems = controllers
            .SelectMany(c => c.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .SelectMany(ctor => ctor.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => !(t.IsInterface && (InNamespace(t, ".Services") || t.Assembly.GetName().Name == "Crm.Platform")))
                .Select(t => $"{c.FullName} depends on {t.FullName}"));

        var actionProblems = controllers
            .SelectMany(c => c.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .SelectMany(m => m.GetParameters().SelectMany(p => Unwrap(p.ParameterType))
                    .Where(t => !InNamespace(t, ".Dtos") && !IsFrameworkType(t))
                    .Select(t => $"{c.FullName}.{m.Name} takes {t.FullName}")));

        return [.. constructorProblems.Concat(actionProblems).Distinct(StringComparer.Ordinal)];
    }

    private static bool IsFrameworkType(Type type)
    {
        var assembly = type.Assembly.GetName().Name ?? string.Empty;
        return assembly.StartsWith("System", StringComparison.Ordinal) || assembly.StartsWith("Microsoft.", StringComparison.Ordinal);
    }

    /// <summary>Source files under a Controllers folder that contain a try block (comments ignored).</summary>
    public static IReadOnlyList<string> ControllerFilesWithTryCatch(IReadOnlyList<Assembly> assemblies) =>
        assemblies
            .Select(ProjectSources.DirectoryOf)
            .Select(project => Path.Combine(project, "Controllers"))
            .Where(Directory.Exists)
            .SelectMany(folder => Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories))
            .Where(file => TryBlock().IsMatch(Comments().Replace(File.ReadAllText(file), string.Empty)))
            .Select(ProjectSources.Relative)
            .Order(StringComparer.Ordinal)
            .ToList();

    [GeneratedRegex(@"\btry\s*\{")]
    private static partial Regex TryBlock();

    [GeneratedRegex(@"//[^\n]*|/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex Comments();

    private static readonly HashSet<string> ModuleLayout = new(StringComparer.Ordinal)
    {
        "Controllers", "Services", "Dtos", "Models", "Persistence", "Mapping", "Validation", "Handlers", "Skills",
        "Properties", "bin", "obj",
    };

    /// <summary>Top-level folders of a module implementation project that are not in the module layout (docs/architecture.md §1).</summary>
    public static IReadOnlyList<string> ModuleFoldersOutsideTheLayout(IReadOnlyList<Assembly> moduleImplementations) =>
        moduleImplementations
            .Select(ProjectSources.DirectoryOf)
            .SelectMany(Directory.EnumerateDirectories)
            .Where(folder => !ModuleLayout.Contains(Path.GetFileName(folder)))
            .Select(ProjectSources.Relative)
            .Order(StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Services, controllers and handlers never depend on IEventPublisher: they write events to their module's
    /// outbox, and only the outbox relay (in Persistence) publishes, after the commit (ADR-009).
    /// </summary>
    public static IReadOnlyList<string> PublishersOutsideTheRelay(IReadOnlyList<Assembly> assemblies) =>
        Failing(Types.InAssemblies(assemblies)
            .That().ResideInNamespaceEndingWith(".Services")
            .Or().ResideInNamespaceEndingWith(".Controllers")
            .Or().ResideInNamespaceEndingWith(".Handlers")
            .ShouldNot().HaveDependencyOn("Crm.Platform.Messaging.IEventPublisher")
            .GetResult());

    public static IReadOnlyList<string> DtosReferencingModels(IReadOnlyList<Assembly> assemblies)
    {
        var models = NamespacesEndingWith(assemblies, ".Models");
        return models.Length == 0
            ? []
            : Failing(Types.InAssemblies(assemblies)
                .That().ResideInNamespaceEndingWith(".Dtos")
                .ShouldNot().HaveDependencyOnAny(models)
                .GetResult());
    }

    /// <summary>Models are not public, and no DTO or controller member (of any visibility) has a Model in its signature.</summary>
    public static IReadOnlyList<string> LeakedModels(IReadOnlyList<Assembly> assemblies)
    {
        var publicModels = Failing(Types.InAssemblies(assemblies)
            .That().ResideInNamespaceEndingWith(".Models")
            .Should().NotBePublic()
            .GetResult());

        var inSignatures = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => InNamespace(t, ".Dtos") || InNamespace(t, ".Controllers"))
            .SelectMany(t => t.GetMembers(AllDeclared))
            .Where(m => !m.IsDefined(typeof(CompilerGeneratedAttribute)) && m is not MethodInfo { IsSpecialName: true })
            .SelectMany(m => SignatureTypes(m).SelectMany(Unwrap).Where(t => InNamespace(t, ".Models"))
                .Select(model => $"{m.DeclaringType!.FullName}.{m.Name} uses {model.FullName}"))
            .Distinct(StringComparer.Ordinal);

        return [.. publicModels, .. inSignatures];
    }

    private const BindingFlags AllDeclared =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private static bool InNamespace(Type type, string suffix) => type.Namespace?.EndsWith(suffix, StringComparison.Ordinal) == true;

    private static IEnumerable<Type> SignatureTypes(MemberInfo member) => member switch
    {
        MethodInfo method => [method.ReturnType, .. method.GetParameters().Select(p => p.ParameterType)],
        ConstructorInfo constructor => constructor.GetParameters().Select(p => p.ParameterType),
        PropertyInfo property => [property.PropertyType],
        FieldInfo field => [field.FieldType],
        _ => [],
    };

    /// <summary>The type and every type inside it: generic arguments, array elements, by-ref targets.</summary>
    private static IEnumerable<Type> Unwrap(Type type) =>
        type.HasElementType ? Unwrap(type.GetElementType()!)
        : type.IsGenericType ? [type, .. type.GetGenericArguments().SelectMany(Unwrap)]
        : [type];

    private static string[] NamespacesEndingWith(IReadOnlyList<Assembly> assemblies, string suffix) =>
        assemblies.SelectMany(a => a.GetTypes())
            .Select(t => t.Namespace)
            .Where(n => n is not null && n.EndsWith(suffix, StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray()!;

    private static List<string> Failing(TestResult result) => result.FailingTypeNames?.ToList() ?? [];
}
