# Testing

How tests are organised and run. Rules (TDD, no database mocks, one behaviour per test) are in `CLAUDE.md`; the test pyramid is in `docs/architecture.md` §9.

## Running

| Command | Runs | Needs |
| --- | --- | --- |
| `make test-unit` | everything not tagged `Integration` or `Architecture` | nothing |
| `make test-int` | `Category=Integration` | SQL Server, and a frontend build for the SPA hosting tests (`make build`) |
| `make test-arch` | `Category=Architecture` | nothing |
| `make test-web` | Vitest | nothing |
| `make test` | all of the above | |
| `make coverage` | backend tests with coverage, report in `artifacts/coverage/report` | as `make test-int` |

Tests run on Microsoft.Testing.Platform (ADR-003). Filter by trait with `dotnet test --solution Crm.slnx --filter-trait "Category=Integration"`. VS Code Test Explorer (C# Dev Kit, Vitest Explorer) runs and debugs them too.

Tag a test class with a category from `Crm.Testing.TestCategories`: `Integration`, `Architecture` or `AiSmoke`.

## SQL Server: `SqlFixture`

`Crm.Testing.SqlFixture` gives each test class its own database:

```csharp
[Trait("Category", TestCategories.Integration)]
public sealed class OrganisationServiceTests(SqlFixture sql) : IClassFixture<SqlFixture>
{
    [Fact]
    public async Task Create_ValidOrganisation_IsStored()
    {
        await sql.ResetAsync();            // empty every table (Respawn), keep the schema
        // ... use sql.ConnectionString
    }
}
```

- It creates `crm_test_<guid>` on initialise and drops it on dispose. Use **one fixture per test class** (`IClassFixture<SqlFixture>`). Classes then run in parallel without colliding. Respawn would wipe a database shared between parallel classes, so the fixture is per class rather than per assembly.
- `ResetAsync()` deletes all rows (Respawn), ignores `__EFMigrationsHistory`, and picks up tables created after the fixture started.
- Migrations are not applied yet: the first module DbContext and `Crm.Migrator` arrive in S1-4, which will add a migrations step to the fixture.
- The connection comes from the environment, never from code:

| Variable | Devcontainer | CI (S0-10) |
| --- | --- | --- |
| `CRM_SQL_SERVER` | `db` (`devcontainer.json`) | the service container |
| `CRM_SQL_USER` | `sa` (`docker-compose.yml`) | `sa` |
| `CRM_SQL_PASSWORD` | the development-only password, defined once in `docker-compose.yml` | a CI secret or generated value |

  If `CRM_SQL_PASSWORD` is missing (for example, a container created before S0-9), the fixture fails with `CRM_SQL_PASSWORD is not set`. Rebuild the devcontainer.

### Troubleshooting: "Could not obtain exclusive lock on database 'model'"

`CREATE DATABASE` copies `model`, so it waits for any session that is using `model`. The usual culprit is the VS Code **MSSQL extension's Object Explorer** after browsing the system databases (`program_name = sqltools-ObjectExplorer`). Disconnect it, or run **Developer: Reload Window**. To see who holds it:

```sql
SELECT session_id, program_name FROM sys.dm_exec_sessions WHERE database_id = DB_ID('model');
```

## API tests: `TestWebAppFactory` and fake auth

- `TestWebAppFactory<Program>` is the real host. A plain `CreateClient()` keeps the host's deny-by-default authentication, so use it for negative (anonymous) tests.
- `CreateClientFor(user, environment = "Development")` signs every request in as `user` through `FakeAuthHandler` (claims: name identifier as a GUID, name, email, roles, and the CRM's `crm:team` and `crm:business_unit` claims).
- `TestUserBuilder` makes synthetic users with Bogus: `new TestUserBuilder().WithRoles(Roles.Sales).WithTeams(teamId).WithBusinessUnits(unitId).Build()`. Never use real people or classified or CUI data (CLAUDE.md).
- Test-only endpoints are controllers in the test assembly, added with `AddApplicationPart` (see `tests/Crm.Host.Tests/TestOnlyController.cs`).
- Code reads the user through `ICurrentUser` (S1-2), never claims or `HttpContext`. Fake auth stays the way tests sign in.
- Every permission rule needs a negative test: sign in as a user without the role and assert 403 (see `ApiAuthorisationTests`).

## Architecture tests

`tests/Crm.Architecture.Tests` scans every production `Crm.*` assembly it references (add new module projects to its `.csproj`): solution layout, build gates, central package management, layering (controllers, services, models, DTOs), EF Core only in modules and the Migrator, and no `Migrate` calls in the host.

Module rules (S1-1, ADR-013) run on two sets: the shipped modules (`Crm.Host` `HostModules.All`, messages in `Crm.Mediator.Contracts`, projects in `src/Modules`), which must be clean, and two test-only fixture modules in `tests/Fixtures/Modules` (messages in the stand-in `tests/Fixtures/Fixture.Contracts`). **Sample** obeys every rule; **Rogue** breaks each rule once. Each rule has a `_FlagsRogueOnly` test asserting it reports exactly Rogue's violation, so every rule is proven to fail as well as pass. When you add a module rule, add its violation to Rogue (see `tests/Fixtures/README.md`). The rules live in `ModuleRules` (module project references, message content, visibility, DbContext and schema, handler ownership) and `LayeringRules` (controllers, services, models, DTOs, try/catch, folders). `DbContextModels.Load` builds a module's EF model without a database. `tests/Crm.Host.Tests/EndpointPolicyTests` checks the anonymous allow-list (ADR-011) against the live endpoint table.

## Frontend

Vitest, Testing Library and MSW in `src/web` (`make test-web`). Every new screen gets an axe check (`src/web/src/test/axe.ts`). Unmocked requests fail the test (`onUnhandledFrame: 'error'`).
