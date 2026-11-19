# ADR-013: Project structure: Host, Shared, Mediator, and one library per module

- Status: Accepted (2026-11-19, during S1-3, at the developer's direction). Supersedes the project layout in ADR-009 (per-module Contracts projects, mediator in `Crm.Platform`); ADR-009's communication rules stand.

## Context

S1-1 to S1-3 built each module as two projects (`Crm.X.Contracts`, `Crm.X`) with a kernel `Crm.Platform`, and allowed a module to reference another module's Contracts project. Boundaries were enforced by reflection over compiled assemblies. The intended design is simpler: a module is one library that no other module references, which a reader (and a test) can check from project files alone.

## Decision

```
src/
  Crm.Host/                 web API (composition root). References Crm.SharedKernel, Crm.Mediator and every module.
  Crm.SharedKernel/               common features: IModule and composition, current user, roles and policies,
                            typed errors and the global exception handler. No endpoints, no data stores.
  Crm.Mediator/             how modules talk: IMediator, IEventPublisher, pipeline, envelope, and every
                            cross-module message under Crm.Mediator.Contracts.<Module>. References Crm.SharedKernel.
  Crm.Testing/              test support (SQL fixture, fake auth, builders, log capture). Tests only.
  Crm.Migrator/             applies every module's migrations.
  Modules/Crm.<Module>/     one class library per module: its endpoints (controllers), services, models,
                            its DbContext in its own schema, its outbox and inbox, its mediator handlers.
```

1. **A module project references only `Crm.SharedKernel` and `Crm.Mediator`** (plus packages). It never references another module, `Crm.Host` or `Crm.Migrator`. `Crm.Host` references every module.
2. **Cross-module messages live in `Crm.Mediator`**, one folder and namespace per owning module: `Crm.Mediator.Contracts.Directory.GetOrganisationSummary`. They are records, enums and constants only (ADR-009 rule 2). Changing a module's messages is a change to `Crm.Mediator`, reviewed as such.
3. **The owning module is the namespace**: a request in `Crm.Mediator.Contracts.<Module>` has exactly one handler, in `Crm.<Module>`. Checked at startup by `AddMediator(registry)` and by architecture tests. `IModule.ContractsAssembly` is removed.
4. **`Crm.SharedKernel` and `Crm.Mediator` have no endpoints and no data stores**: no controllers, no EF Core (the S0-9 rule), no outbox. Each module keeps its own outbox, inbox and relay (ADR-009 rule 4).
5. Unchanged: ADR-009 (mediator only between modules; only the relay publishes), ADR-012 (everything in a module is internal except its `IModule` class).

## Enforcement

- `ModuleProjects_ReferenceOnlySharedAndMediator` reads every module `.csproj`.
- `Host_References_EveryModuleProject` and `Host_ModulesList_MatchesModuleProjects`.
- `Contracts_ContainOnlyMessagesDtosAndConstants` and `EachRequest_HasExactlyOneHandler_InOwningModule`, by namespace.
- `Projects_ReferencingEntityFrameworkCore_AreModulesOrMigrator` (S0-9) keeps `Crm.SharedKernel` and `Crm.Mediator` free of data stores; `Controllers_` rules keep endpoints in modules and the host.

## Consequences

- The boundary is visible in project files; a stray reference fails one simple test.
- `Crm.Mediator` grows with every module's messages and is rebuilt when any of them changes. Messages are small records, so the cost is low; ownership is by namespace and enforced by test.
- Modules no longer need a second project, so the scaffolder (S1-4) generates one project per module.
- Removed rules: `Contracts_Assemblies_DoNotReference_EfCore` (the S0-9 rule covers `Crm.Mediator`) and `Modules_DoNotResolve_OtherModulesServices` (a module cannot name another module's types once it cannot reference it).

## Alternatives rejected

- **Per-module Contracts projects** (ADR-009 as first written): a module project file references another module's project, which this structure rules out.
- **A separate `Crm.Contracts` library**: keeps message records out of the mediator, at the cost of a fourth shared project that every module references anyway.
