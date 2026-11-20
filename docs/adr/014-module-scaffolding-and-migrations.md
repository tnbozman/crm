# ADR-014: Module scaffolding and migrations

- Status: Accepted (2026-11-20, S1-4).

## Context

ADR-013 makes each module one library with its own DbContext and schema. S1-4 needs a scaffolder that produces such a module, and `Crm.Migrator` needs to apply every module's migrations without breaking module encapsulation (every module type except its `IModule` class is internal, ADR-012).

## Decision

1. **Templates, not code generation.** `make module-new name=<Module>` runs `scripts/new-module.sh`, which copies `scripts/templates/module/` replacing `__Module__` (PascalCase) and `__module__` (schema). It generates the module library, its test project (`tests/Modules/<Module>/Crm.<Module>.Tests`), its messages folder (`src/Crm.Mediator/Contracts/<Module>`) and its web feature folder. It then adds the module to `Crm.slnx`, `Crm.Host` (project reference and `HostModules.All`) and `Crm.Migrator` (project reference and `MigratorModules.All`), and generates the `Initial` migration. Each layer has one example (`Item`) with tests; the developer replaces it.
2. **The Migrator finds DbContexts through `IModule.AddServices`.** It builds a service collection per module, calls `AddServices`, and migrates each registered `DbContext` subtype. `IModule` gains no member and DbContexts stay internal.
3. **Migrations are internal and per schema.** `make migration-add` (`scripts/migration-add.sh`) runs dotnet-ef with the module project as its own startup project (a design-time factory, no database), then makes the generated classes internal and strips the byte order mark. The history table is `<schema>.__EFMigrationsHistory`.
4. **One connection string**, `ConnectionStrings:Crm` (`ModuleDatabase.ConnectionStringName`), read when a DbContext is first created, so the host starts without one.
5. **Reset is Development only.** `Crm.Migrator --reset` (make db-reset) drops and recreates the database and is refused in any other `DOTNET_ENVIRONMENT`.
6. **Proof.** `make check-module-new` (in `make verify`) scaffolds `Zzz` in a temporary copy and requires the build, `make test-arch`, the format checks, the generated tests (unit and SQL Server) and `make db-reset`/`make migrate` to succeed.

## Consequences

- A template change is tested end to end in CI, at the cost of about three minutes in `make verify`.
- The host and the Migrator each keep a module list; architecture tests keep both in step with `src/Modules`.
- The example `Item` must be deleted (with a new migration) when a real module replaces it.

## Alternatives rejected

- **`dotnet new` template package**: adds packaging and installation steps; the shell script with `sed` needs neither.
- **Migrator references `Crm.Host`** to reuse `HostModules.All`: pulls the web host into a console tool run with deployment rights.
- **`IModule.MigrateAsync` or `InternalsVisibleTo("Crm.Migrator")` in each module**: adds migration code to the module contract, or widens every module's internals.
