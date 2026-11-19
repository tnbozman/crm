using System.Reflection;
using Crm.Host;
using Crm.Mediator;
using Crm.SharedKernel.Modules;

namespace Crm.Architecture.Tests;

/// <summary>A module as the rules see it: its registration, its library, and its namespace for messages.</summary>
internal sealed record ModuleUnderTest(IModule Module, Assembly Implementation, string ContractsNamespace)
{
    public string Name => Module.Name;
}

/// <summary>
/// A group of modules and where their messages live (ADR-013): the shipped modules with Crm.Mediator.Contracts,
/// or the test fixtures with Fixture.Contracts. Module project files may reference only <see cref="AllowedReferences"/>.
/// </summary>
internal sealed record ModuleSet(
    IReadOnlyList<ModuleUnderTest> Modules,
    Assembly ContractsAssembly,
    string ContractsRoot,
    string ProjectsFolder,
    IReadOnlySet<string> AllowedReferences)
{
    public IReadOnlyList<Assembly> Implementations => [.. Modules.Select(m => m.Implementation)];

    /// <summary>Every module project file under <see cref="ProjectsFolder"/> (one folder per module).</summary>
    public IReadOnlyList<string> ProjectFiles =>
        Directory.Exists(RepositoryRoot.File(ProjectsFolder))
            ? [.. Directory.EnumerateDirectories(RepositoryRoot.File(ProjectsFolder))
                .SelectMany(d => Directory.EnumerateFiles(d, "*.csproj"))
                .Order(StringComparer.Ordinal)]
            : [];

    public static ModuleSet Of(IEnumerable<IModule> modules, Assembly contractsAssembly, string contractsRoot, string projectsFolder, params string[] allowed) =>
        new(
            [.. modules.Select(m => new ModuleUnderTest(m, m.GetType().Assembly, $"{contractsRoot}.{m.Name}"))],
            contractsAssembly,
            contractsRoot,
            projectsFolder,
            allowed.ToHashSet(StringComparer.Ordinal));
}

/// <summary>The modules the host ships (Crm.Host HostModules.All); messages in Crm.Mediator.Contracts.</summary>
internal static class ShippedModules
{
    public static ModuleSet Set { get; } = ModuleSet.Of(
        HostModules.All, typeof(IMediator).Assembly, MessagingServiceCollectionExtensions.ContractsNamespace, "src/Modules",
        "Crm.SharedKernel", "Crm.Mediator");

    public static IReadOnlyList<ModuleUnderTest> All => Set.Modules;
}

/// <summary>Test-only modules (tests/Fixtures): Sample obeys every rule, Rogue breaks each once.</summary>
internal static class FixtureModules
{
    public static ModuleSet Set { get; } = ModuleSet.Of(
        [new Fixture.Sample.SampleModule(), new Fixture.Rogue.RogueModule()],
        typeof(Fixture.Contracts.Sample.GetSampleSummary).Assembly, "Fixture.Contracts", "tests/Fixtures/Modules",
        "Crm.SharedKernel", "Crm.Mediator", "Fixture.Contracts");

    public static IReadOnlyList<ModuleUnderTest> All => Set.Modules;

    public static IReadOnlyList<Assembly> Implementations => Set.Implementations;
}
