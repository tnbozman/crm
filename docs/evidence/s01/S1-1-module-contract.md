# S1-1 evidence: module contract and architecture tests

Date: 2026-11-19 (UTC). Branch `sprint-1`, commits `ef40d99`..`d5678b3` plus the docs commit that ticks the story. Design: ADR-009 (mediator, Contracts hold messages only), ADR-012 (module types internal).

## Controls supported

| Control | What this story provides |
| --- | --- |
| NIST 800-171 3.4.1 / 3.4.2 (baseline configuration) | Module boundaries and layering are executable checks in `make verify` and CI, not conventions. A change that breaks one fails the build. |
| NIST 800-171 3.1.1 / 3.1.2 (limit access to authorised functions) | Modules cannot compile against or register each other's implementation types (only the `IModule` class is public, ADR-012), so cross-module access can only go through Contracts messages and, from S1-3, the mediator pipeline where authorisation is applied. |
| NIST 800-171 3.13.2 (architectural design that promotes security) | One DbContext per module, in the module's own schema; no EF Core in Contracts; controllers never reach DbContext or Models. |

## How the rules are proven

No shipped module exists yet: the scaffolder arrives in S1-4 and Access in S1-6. So each module rule runs twice:

- on the **shipped modules** (`Crm.Host` `HostModules.All`, empty today), which must be clean; and
- on two **test-only fixture modules** in `tests/Fixtures/Modules`: **Sample**, which obeys every rule, and **Rogue**, which breaks each rule once. Each `_FlagsRogueOnly` test asserts the rule reports exactly Rogue's violation and nothing for Sample.

This replaces S0-9's plant-then-remove method with permanent negative tests (13 `_FlagsRogueOnly` tests). Full list: [S1-1/architecture-tests.txt](S1-1/architecture-tests.txt).

## Tests, each seen failing first

| Test | First failure |
| --- | --- |
| `ModuleRegistryTests.Constructor_DuplicateModuleNames_Throws` | did not compile (no `Crm.Platform.Modules`) |
| `ModuleRegistryTests.Constructor_NameNotPascalCaseLetters_Throws` (4 cases) | no exception thrown |
| `ModuleSchemaTests.Schema_PascalCaseName_IsLowercasedName` | did not compile (no `Schema()`) |
| `ModuleCompositionTests.AddModules_TwoModules_RegistersEachModulesServices` | did not compile (no `AddModules`) |
| `ModuleCompositionTests.AddModules_Module_AddsItsControllersAssemblyAsApplicationPart` | no `ApplicationPartManager` registered |
| `ModuleCompositionTests.AddModules_Registry_IsResolvable` | no service for `ModuleRegistry` |
| `ModuleCompositionTests.AddModules_InternalControllerInModuleAssembly_IsDiscovered` | internal controller not discovered |
| `ModuleCompositionTests.AddModules_InternalControllerOutsideModuleAssemblies_IsNotDiscovered` | passed on first run, which proved nothing (the test assembly was not an application part). Fixed so it is one; it then failed against a planted widening of the provider and passed once that was removed |
| `Crm.Host.Tests` `Host_Started_ComposesHostModules` | registry was null (host did not call `AddModules`) |
| `Host_ModulesList_MatchesModuleProjects` | did not compile (no `HostModules`); then failed against a planted `src/Modules/Zzz` folder, which was removed |
| `ModuleImplementations_DoNotReference_OtherModuleImplementations` | stub rule reported nothing; Rogue references Sample |
| `Contracts_Assemblies_DoNotReference_EfCore` | stub rule reported nothing; Rogue.Contracts uses `EntityState` |
| `Contracts_Assemblies_ContainOnlyMessagesDtosAndConstants` | stub reported nothing (Rogue: an interface and a behaviour class). A second red: Sample's `static readonly` constant was wrongly flagged (its static constructor), fixed in the rule |
| `Module_Types_AreInternalUnlessInContracts` | stub reported nothing (Rogue: public service). Tightened after ADR-012: then failed on Rogue's public DTO |
| `Modules_DoNotResolve_OtherModulesServices` | stub reported nothing (Rogue registers Sample's module class) |
| `EachModule_HasOneDbContext_WithDefaultSchemaEqualToModuleSchema` | stub reported nothing (Rogue: two contexts, one in schema `reporting`) |
| `Controllers_DoNotReference_DbContextOrModels` (fixtures) | Rogue had no controller yet |
| `Services_DoNotReference_HttpContextOrMvcTypes` (fixtures) | Rogue had no offending service yet |
| `Models_AreInternalAndNotUsedInDtosOrControllerSignatures` | rule only checked visibility; Rogue's DTO and controller use `RogueItem` in signatures |
| `Dtos_DoNotReference_Models` | stub reported nothing |
| `Controllers_OnlyDependOnServiceInterfacesAndDtos` | stub reported nothing (Rogue controller takes concrete `RogueService`) |
| `Controllers_HaveNoTryCatch` | stub reported nothing (Rogue controller has try/catch) |
| `ModuleFolders_Follow_ControllersServicesDtosModelsPersistenceHandlers` | stub reported nothing (Rogue has `Helpers/`) |

`make verify` green on 2026-11-19: unit 20, integration 6, architecture 41, Vitest 13; no High or Critical vulnerabilities.

## Decisions

- **Split** into S1-1a (module kernel: `IModule`, `ModuleRegistry`, `AddModules`, host list), S1-1b (module boundary rules) and S1-1c (layering rules) to keep each near ~400 lines. All three are in this story.
- **ADR-012**: a public controller cannot take an internal service interface, so module controllers and DTOs are internal and the platform discovers internal controllers in module assemblies only. Only the `IModule` class is public.
- **Schema from name**: a module's schema is its PascalCase name in lower case (`Schema()`), and `ModuleRegistry` rejects names that are not PascalCase letters, so no module can choose a schema that differs from its name.
- **Controllers_HaveNoTryCatch** scans source, not IL: async methods compile to state machines that always contain a generated catch.
- **S0-9 tests renamed** to the S1-1 names (`Services_DoNotReference_HttpContextOrMvcTypes`, `Models_AreInternalAndNotUsedInDtosOrControllerSignatures`); the models test now also checks signatures.
- **Package**: `Microsoft.EntityFrameworkCore.SqlServer` 10.0.15 (latest 10.x), used by the fixtures and `DbContextModels` (ADR-010 log).

## Open

- *Done when* says the tests "pass with the Access module". Access is created in S1-6; its architecture tests running green is part of S1-6's own *Done when*.
- `Handlers_DoNotSendInsideATransaction` (ADR-009) stays a review rule.
