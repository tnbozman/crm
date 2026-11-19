# S1-3 added scope evidence: project structure (ADR-013)

Date: 2026-11-19 (UTC). Branch `sprint-1`, commits `329e6aa`..`742d032` plus the docs commit. Added at the developer's direction: the intended modular monolith is a web API host, one library per module that no other module references, a shared kernel and a mediator library.

## Change

| Before | After |
| --- | --- |
| `Crm.Platform` (kernel) | `Crm.SharedKernel` (module composition, current user, roles and policies, errors, exception handler) and `Crm.Mediator` (mediator, pipeline, envelope, messages) |
| `Crm.<Module>.Contracts` + `Crm.<Module>` per module; modules could reference other modules' Contracts | one library `src/Modules/Crm.<Module>` per module, referencing only `Crm.SharedKernel` and `Crm.Mediator` |
| messages in each module's Contracts | every message in `Crm.Mediator.Contracts.<Module>` |
| `IModule.ContractsAssembly`; validation in `AddModules` | validation over the contracts namespace in `AddMediator(registry)`, called after `AddModules` |
| `Crm.Platform.Testing` | `Crm.Testing` (also holds `TestModule`, `TestUserContext`) |
| reflection rule: implementations do not reference each other | project-file rule: module `.csproj` references only `Crm.SharedKernel` and `Crm.Mediator`; host `.csproj` references every module |

Name: `Crm.SharedKernel`, not `Crm.Shared`, because the analyzer (CA1716, warnings as errors) rejects `Shared`, a Visual Basic keyword, in namespaces. A scoped suppression was proposed and refused by the tool permission check (CLAUDE.md: never disable a lint rule); the developer chose the rename.

## Controls supported

| Control | What this change provides |
| --- | --- |
| NIST 800-171 3.4.1 / 3.4.2 (baseline configuration) | Module isolation is checkable from project files: a module referencing another module fails `ModuleProjects_ReferenceOnlySharedKernelAndMediator`. |
| NIST 800-171 3.13.2 (architectural design) | Modules cannot compile against each other; the only channel is `Crm.Mediator`, whose pipeline authorises as the current user (S1-3). |

## Tests

| Test | Evidence |
| --- | --- |
| Stage 2 (split and rename) | No behaviour change: every test passed after the move with only namespace, project-name and expected-assembly-list edits (the requirement changed). |
| `Startup_RequestWithNoHandler_FailsValidation`, `Startup_RequestWithTwoHandlers_FailsValidation`, `Startup_EveryRequestHasOneHandler_Succeeds` | rewritten against a test contracts namespace; first failure: did not compile (no `AddMediator(registry, assembly, namespace)`) |
| `ModuleProjects_ReferenceOnlySharedKernelAndMediator` (+ `_FlagsRogueOnly`) | stub rule reported nothing; Rogue's project references Sample |
| `Host_ModulesList_MatchesModuleProjects`, `Host_References_EveryModuleProject` | pass with no modules; both failed against a planted `src/Modules/Crm.Zzz/Crm.Zzz.csproj`, which was removed |
| `Contracts_ContainOnlyMessagesDtosAndConstants`, `EachRequest_HasExactlyOneHandler_InOwningModule`, layering rules | unchanged in intent; now read messages by namespace and the fixtures' new paths |

Removed (made redundant by the structure, ADR-013): `ModuleImplementations_DoNotReference_OtherModuleImplementations` (replaced by the project-file rule), `Contracts_Assemblies_DoNotReference_EfCore` (the S0-9 EF Core rule covers `Crm.Mediator`), `Modules_DoNotResolve_OtherModulesServices` (a module cannot name another module's types). Renamed: `Module_Types_AreInternalUnlessInContracts` to `Module_Types_AreInternal_ExceptTheModuleClass`.

`make verify` green on 2026-11-19: unit 58, integration 6, architecture 44, Vitest 13; no High or Critical vulnerabilities.

## Not changed

S1-1 to S1-3 evidence notes describe the code as it was then and keep the old names. ADR-009's communication rules, ADR-012 and the global exception handler are unchanged apart from names.
