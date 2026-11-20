# S1-4 evidence: module scaffolder

Date: 2026-11-20 (UTC). Branch `claude/story-s01-4-tdd-6nnpv7`, commits `04215d0`..the docs commit. Design: ADR-014 (with ADR-012 and ADR-013).

Split (the story passed the ~400-line guideline): **S1-4a** scaffolder and generated module (`04215d0`, `8f3fa5b`, `bdb86bb`); **S1-4b** Migrator, `make migrate` and `make db-reset` (`63b3a85`, `8fd851e`, then refactor and docs). The make targets stubbed `TODO(S1-4)` (`module-new`, `migration-add`, `migrate`, `db-reset`) are all implemented, as `make-targets.sh` requires once S1-4 is ticked.

## What `make module-new name=Zzz` generates

| Path | Content |
| --- | --- |
| `src/Modules/Crm.Zzz/` | one library referencing only `Crm.SharedKernel` and `Crm.Mediator`: `ZzzModule` (the only public type), Controllers, Services, Dtos, Models, Persistence (`ZzzDbContext` in schema `zzz`, configuration, design-time factory, `Migrations/<timestamp>_Initial`), Mapping, Validation, Handlers, `Skills/` |
| `src/Crm.Mediator/Contracts/Zzz/` | example query `GetZzzItemSummary` and response record |
| `tests/Modules/Zzz/Crm.Zzz.Tests/` | model, mapping, validation and pending-model-changes unit tests; service, controller (positive and negative authorisation), handler and migrations tests on real SQL Server |
| `src/web/src/features/zzz/` | web feature folder |
| `Crm.slnx`, `Crm.Host`, `Crm.Migrator` | project references, `HostModules.All` and `MigratorModules.All` entries |

## Controls supported

| Control | What this story provides |
| --- | --- |
| NIST 800-171 3.4.1 / 3.4.2 (baseline configuration) | Every module starts from the same reviewed template, and CI proves the template meets the architecture rules (`make check-module-new` in `make verify`). |
| NIST 800-171 3.1.1 / 3.1.5 (access control, least privilege) | Generated endpoints carry a named policy; generated tests include the 403 (wrong role) and 401 (anonymous) cases. |
| NIST 800-171 3.4.5 (access restrictions for change) | Migrations run only through `Crm.Migrator` with the deployment identity; the host never migrates. Reset (drop database) is refused outside Development (`RunAsync_ResetOutsideDevelopment_IsRefused`). |

## Tests (first failure, then green)

| Test | First failure |
| --- | --- |
| `scripts/checks/new-module.sh` (`make check-module-new`) | 26 checks failed: `make module-new` was the `TODO(S1-4)` stub, so invalid names were accepted and nothing was generated |
| generated `Crm.Zzz.Tests` (16 tests) | did not compile: the module had only its project file and `IModule` class |
| `ModuleFolders_Follow_…` on the scaffolded module | flagged `src/Modules/Crm.Zzz/Crm`: dotnet-ef put the first snapshot under a namespace-named folder when given `--namespace` (fixed in `migration-add.sh`) |
| `ProductionAssemblies_AreDiscovered` | listed only the skeleton; requirement changed (module libraries are production assemblies), now expects the shipped modules too |
| generated code format check | `CHARSET` error: dotnet-ef writes a byte order mark (stripped in `migration-add.sh`) |
| `Migrator_ModulesList_MatchesModuleProjects`, `Migrator_References_EveryModuleProject` | did not compile (no `MigratorModules`), then failed for the scaffolded Zzz until the scaffolder wired the Migrator |
| `MigrationRunnerTests` (4), `MigrationRunnerDatabaseTests` (2) | did not compile (no `MigrationRunner`, no `ModuleDatabase`) |
| `new-module.sh` step "make db-reset and make migrate" | `TODO(S1-4)` stubs: Zzz not migrated, `zzz.Items` missing |

`make verify` green on 2026-11-20: unit 62, integration 8, architecture 46, Vitest 13; `check-module-new`: all steps pass (architecture 46 and generated module 16 in the scaffolded copy, `make db-reset`/`make migrate` migrated Zzz); no High or Critical vulnerabilities.

## Not done here

- Audit events for the example `Item` create: the Audit module arrives in S2; real modules emit them.
- Outbox, inbox and relay templates: S1-5 adds them to `Persistence/`.
- Dog-food (Access created with the scaffolder): confirmed in S1-6.
