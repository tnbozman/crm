using Shouldly;
using Xunit;

namespace Crm.Architecture.Tests;

/// <summary>
/// Module boundary rules. Each rule runs on the shipped modules (must be clean) and on the fixtures
/// (must report exactly the Rogue violation, which also proves Sample is clean).
/// </summary>
[Trait("Category", "Architecture")]
public sealed class ModuleBoundaryTests
{
    [Fact]
    public void ModuleImplementations_DoNotReference_OtherModuleImplementations() =>
        ModuleRules.ImplementationReferencesToOtherModules(ShippedModules.All).ShouldBeEmpty();

    [Fact]
    public void ModuleImplementations_DoNotReference_OtherModuleImplementations_FlagsRogueOnly() =>
        ModuleRules.ImplementationReferencesToOtherModules(FixtureModules.All).ShouldBe(["Fixture.Rogue references Fixture.Sample"]);

    [Fact]
    public void Contracts_Assemblies_DoNotReference_EfCore() =>
        ModuleRules.ContractsReferencesToEfCore(ShippedModules.All).ShouldBeEmpty();

    [Fact]
    public void Contracts_Assemblies_DoNotReference_EfCore_FlagsRogueOnly() =>
        ModuleRules.ContractsReferencesToEfCore(FixtureModules.All).ShouldBe(["Fixture.Rogue.Contracts references Microsoft.EntityFrameworkCore"]);
}
