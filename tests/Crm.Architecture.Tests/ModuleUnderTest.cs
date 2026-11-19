using System.Reflection;
using Crm.Host;
using Crm.Platform.Modules;

namespace Crm.Architecture.Tests;

/// <summary>A module as the module rules see it: its registration, implementation assembly and Contracts assembly.</summary>
internal sealed record ModuleUnderTest(IModule Module, Assembly Implementation, Assembly Contracts)
{
    public string Name => Module.Name;

    public static ModuleUnderTest From(IModule module)
    {
        var implementation = module.GetType().Assembly;
        var contracts = Assembly.Load(implementation.GetName().Name + ".Contracts");
        return new ModuleUnderTest(module, implementation, contracts);
    }
}

/// <summary>The modules the host ships (Crm.Host HostModules.All).</summary>
internal static class ShippedModules
{
    public static IReadOnlyList<ModuleUnderTest> All { get; } = HostModules.All.Select(ModuleUnderTest.From).ToList();
}

/// <summary>Test-only modules (tests/Fixtures/Modules): Sample obeys every rule, Rogue breaks each once.</summary>
internal static class FixtureModules
{
    public static IReadOnlyList<ModuleUnderTest> All { get; } =
        [ModuleUnderTest.From(new Fixture.Sample.SampleModule()), ModuleUnderTest.From(new Fixture.Rogue.RogueModule())];
}
