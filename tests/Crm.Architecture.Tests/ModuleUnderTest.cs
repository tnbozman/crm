using System.Reflection;
using Crm.Host;
using Crm.SharedKernel.Modules;

namespace Crm.Architecture.Tests;

/// <summary>A module as the module rules see it: its registration, implementation assembly and Contracts assembly.</summary>
internal sealed record ModuleUnderTest(IModule Module, Assembly Implementation, Assembly Contracts)
{
    public string Name => Module.Name;

    public static ModuleUnderTest From(IModule module, Assembly contracts)
    {
        return new ModuleUnderTest(module, module.GetType().Assembly, contracts);
    }
}

/// <summary>The modules the host ships (Crm.Host HostModules.All).</summary>
internal static class ShippedModules
{
    public static IReadOnlyList<ModuleUnderTest> All { get; } = HostModules.All.Select(m => ModuleUnderTest.From(m, typeof(Crm.Mediator.IMediator).Assembly)).ToList();
}

/// <summary>Test-only modules (tests/Fixtures/Modules): Sample obeys every rule, Rogue breaks each once.</summary>
internal static class FixtureModules
{
    public static IReadOnlyList<ModuleUnderTest> All { get; } =
        [
            ModuleUnderTest.From(new Fixture.Sample.SampleModule(), typeof(Fixture.Sample.Contracts.GetSampleSummary).Assembly),
            ModuleUnderTest.From(new Fixture.Rogue.RogueModule(), typeof(Fixture.Rogue.Contracts.RogueChanged).Assembly),
        ];

    public static IReadOnlyList<Assembly> Implementations { get; } = All.Select(m => m.Implementation).ToList();
}
