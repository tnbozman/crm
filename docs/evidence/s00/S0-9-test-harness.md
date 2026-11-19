# S0-9 evidence: test harness

Date: 2026-11-19 (UTC). Split into three parts because it exceeded CLAUDE.md's ~400-line guideline.

| Part | Commits |
| --- | --- |
| S0-9b fake auth, `TestWebAppFactory`, Bogus builders | `e697c97` |
| S0-9c layering and persistence architecture tests | `37f9ff7` |
| S0-9a credentials from the environment | `d920692` |
| S0-9a `SqlFixture` and Respawn | `e048c93` |

## Controls supported

| Control | What this story provides |
| --- | --- |
| CLAUDE.md "no secrets in code" / NIST 800-171 3.5.10, 3.13.x hygiene | The dev SQL password is removed from `doctor.sh`, `post-create.sh` and all code. It is defined once in `docker-compose.yml` (development only) and reaches tools and tests only as `CRM_SQL_PASSWORD`. |
| CLAUDE.md "synthetic data only" | `TestUserBuilder` (Bogus, `en_AU`, `example.*` emails) is the way to make test users. |
| CLAUDE.md layering and module rules / NIST 800-171 3.4.1 (baseline) | The architecture rules are executable: controllers do not touch EF Core or Models, services do not touch HTTP or MVC types, Models are internal, actions return DTOs, EF Core is only in modules and the Migrator, and the host never migrates. |
| Test integrity | Integration tests run against real SQL Server in isolated per-class databases (no database mocks, CLAUDE.md). |

## Tests, each seen failing first

| Test | Seen failing first |
| --- | --- |
| `SqlFixture_CreatesIsolatedDatabase_AndDropsIt` (`Integration`) | did not compile (no `SqlFixture`). Then `CRM_SQL_PASSWORD is not set` before credentials were passed. Then `Could not obtain exclusive lock on database 'model'` while a VS Code MSSQL Object Explorer session held `model`. Passed once it was released |
| `Respawn_ResetsData_BetweenTests` (`Integration`) | did not compile (no `ResetAsync`) |
| `FakeAuth_UserWithRoles_IsReadByCurrentUser` | did not compile (no `TestUserBuilder` or `CreateClientFor`) |
| `Controllers_DoNotReference_DbContextOrModels`, `Services_DoNotReference_HttpContextOrMvc`, `Models_AreInternal`, `Controllers_ReturnOnlyDtoTypes`, `Projects_ReferencingEntityFrameworkCore_AreModulesOrMigrator`, `Host_DoesNotCall_Migrate` (`Architecture`) | passed on the real code (no modules yet). Each failed (6 of 6) against planted violations: a public Model, a controller returning a Model, a service taking `HttpContext`, a `.Migrate()` call in the host, and a stray project under `src/` referencing EF Core. The plants were removed |

Also: every existing `Crm.Host.Tests` test passes unedited on the shared harness (`TestHostFactory` now derives from `TestWebAppFactory<Program>`). No `crm_test_*` databases remain after the runs.

## Decisions

- **Database per test class, not per assembly** (`docs/architecture.md` §9 updated). The story requires parallel classes not to collide, and Respawn would wipe a database shared between parallel classes.
- **No migrations step yet.** The first DbContext and the Migrator arrive in S1-2, which adds it to the fixture.
- **Container rebuild needed:** containers created before `d920692` lack `CRM_SQL_USER` and `CRM_SQL_PASSWORD`, so `make doctor` and `make test-int` fail with a clear message until **Rebuild Container**.
