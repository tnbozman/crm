using Shouldly;
using Xunit;

namespace Crm.Architecture.Tests;

/// <summary>
/// Module boundary rules (ADR-009, ADR-012, ADR-013). Each rule runs on the shipped modules (must be clean) and on the
/// fixtures (must report exactly the Rogue violation, which also proves Sample is clean).
/// </summary>
[Trait("Category", "Architecture")]
public sealed class ModuleBoundaryTests
{
    [Fact]
    public void ModuleProjects_ReferenceOnlySharedKernelAndMediator() =>
        ModuleRules.ModuleProjectReferenceProblems(ShippedModules.Set).ShouldBeEmpty();

    [Fact]
    public void ModuleProjects_ReferenceOnlySharedKernelAndMediator_FlagsRogueOnly() =>
        ModuleRules.ModuleProjectReferenceProblems(FixtureModules.Set).ShouldBe(["Fixture.Rogue references Fixture.Sample"]);

    [Fact]
    public void Contracts_ContainOnlyMessagesDtosAndConstants() =>
        ModuleRules.ContractsTypesThatAreNotMessagesDtosOrConstants(ShippedModules.Set).ShouldBeEmpty();

    [Fact]
    public void Contracts_ContainOnlyMessagesDtosAndConstants_FlagsRogueOnly() =>
        ModuleRules.ContractsTypesThatAreNotMessagesDtosOrConstants(FixtureModules.Set).ShouldBe(
            ["Fixture.Contracts.Rogue.IRogueQueries is an interface", "Fixture.Contracts.Rogue.RogueCalculator is a class, not a record"],
            ignoreOrder: true);

    [Fact]
    public void Module_Types_AreInternal_ExceptTheModuleClass() =>
        ModuleRules.PublicImplementationTypes(ShippedModules.All).ShouldBeEmpty();

    [Fact]
    public void Module_Types_AreInternal_ExceptTheModuleClass_FlagsRogueOnly() =>
        ModuleRules.PublicImplementationTypes(FixtureModules.All).ShouldBe(
            ["Fixture.Rogue.Dtos.RogueResponse", "Fixture.Rogue.Services.RogueService"], ignoreOrder: true);

    [Fact]
    public void EachModule_HasOneDbContext_WithDefaultSchemaEqualToModuleSchema() =>
        ModuleRules.DbContextProblems(ShippedModules.All).ShouldBeEmpty();

    [Fact]
    public void EachModule_HasOneDbContext_WithDefaultSchemaEqualToModuleSchema_FlagsRogueOnly() =>
        ModuleRules.DbContextProblems(FixtureModules.All).ShouldBe(
            [
                "Rogue has 2 DbContexts: RogueDbContext, RogueReportingDbContext",
                "Fixture.Rogue.Persistence.RogueReportingDbContext has default schema 'reporting', expected 'rogue'",
            ],
            ignoreOrder: true);

    [Fact]
    public void EachRequest_HasExactlyOneHandler_InOwningModule() =>
        ModuleRules.RequestHandlerProblems(ShippedModules.Set).ShouldBeEmpty();

    [Fact]
    public void EachRequest_HasExactlyOneHandler_InOwningModule_FlagsRogueOnly() =>
        ModuleRules.RequestHandlerProblems(FixtureModules.Set).ShouldBe(
            [
                "Fixture.Contracts.Rogue.GetRogueStatus has no handler",
                "Fixture.Contracts.Sample.GetSampleSummary has 2 handlers",
                "Fixture.Contracts.Sample.GetSampleSummary is handled by Fixture.Rogue.Handlers.SampleSummaryHijacker, outside Fixture.Sample",
            ],
            ignoreOrder: true);
}
