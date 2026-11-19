# Sprint 0 — Walking skeleton (30 Nov – 11 Dec 2026)

**Release**: R0 Foundations. **Epics**: E1 (Azure platform, local part), E4 (build and quality pipeline), E39 (supply chain), E46 (engineering foundations).

## Goal

A fresh clone opens in the devcontainer, `make verify` is green, `make dev` shows a Vue page served through a .NET host, and one trivial end-to-end slice (a health endpoint read by the SPA) is covered by tests at every layer. Nothing business-related yet.

## Demo

1. Clone, "Reopen in Container", wait for post-create.
2. `make verify` passes.
3. `make run` → https://localhost:5001 shows the Vue shell with the API health status read from `/api/v1/health`.
4. `make dev` → edit a Vue component and a C# endpoint; both hot reload.
5. VS Code: run and debug the host, run one xUnit test and one Vitest test from Test Explorer, hit a breakpoint in each; query the dev database from the MSSQL extension.
6. `make publish` → `artifacts/publish/wwwroot/index.html` exists.

## Entry criteria

- Empty Git repository with this pack's `CLAUDE.md` and `docs/` committed.
- Docker available on the developer machine; GitHub repository and Actions enabled.

## Stories

Tick when done. Each story: write the listed tests first.

### ✅ S0-1 Repository and devcontainer (E46)

*As a developer I can open the repo in a container with every tool installed.*

Tests first (these are scripts under `scripts/checks/` run by `make doctor` and by CI):
- `doctor` fails if `dotnet --version` is not 10.x, `node`, `pnpm`, `make`, `sqlcmd` or `git` is missing.
- `doctor` fails if SQL Server on `$CRM_SQL_SERVER` does not accept a login.
- `doctor` fails if any recommended VS Code extension in `.vscode/extensions.json` differs from `devcontainer.json`.

Implement: files from `docs/environment.md` §1–2, `global.json`, `.gitignore`, `.gitattributes`, `.editorconfig`, `make doctor`.

Done when: container builds from scratch; `make doctor` green; post-create is idempotent (run twice).

### ✅ S0-2 Solution layout, Directory.Build and central packages (E46)

Tests first:
- `Crm.Architecture.Tests` project exists with a failing test "solution has Crm.Host, Crm.Platform, Crm.Platform.Testing and Crm.Migrator projects".
- Build fails on a deliberately introduced warning (verify, then remove).

Implement: `Crm.slnx`, `Directory.Build.props` (nullable, warnings as errors, analyzers), `Directory.Packages.props` with versions queried from the feed, projects per `docs/architecture.md` §1.

Done when: `dotnet build` clean with warnings-as-errors; package versions recorded with date in an ADR note.

Evidence: [docs/evidence/s00/S0-2-build-baseline.md](../evidence/s00/S0-2-build-baseline.md); ADR-003 (test runner), ADR-010 (package baseline).

### ✅ S0-3 Makefile (E46)

Tests first: `scripts/checks/make-targets.sh` asserts every target listed in `CLAUDE.md` appears in `make help`, and that `make verify` exits non-zero when a test is made to fail (temporary).

Implement: Makefile per `docs/environment.md` §3.

Done when: every target in the spec runs or is explicitly stubbed with a clear message and a TODO story ID.

Evidence: [docs/evidence/s00/S0-3-makefile.md](../evidence/s00/S0-3-makefile.md).

### ✅ S0-4 Host with health endpoint (E46)

Tests first (xUnit, `WebApplicationFactory`):
- `GetHealth_Anonymous_Returns200WithStatusOk` (health is the only anonymous API).
- `GetUnknownApiRoute_Returns404ProblemDetails`.
- `GetAnyOtherApiRoute_WithoutAuth_Returns401` (fallback authorisation policy exists from day one; use a throwaway test controller registered only in the test host).

Implement: host with MVC controllers (`HealthController`), fallback authorisation policy, exception filter producing problem details, OpenAPI document generation. Establish the folder layout from `docs/architecture.md` §2a in the Host project as the reference.

Done when: tests green; OpenAPI file emitted by `make api-client`.

Evidence: [docs/evidence/s00/S0-4-host-health.md](../evidence/s00/S0-4-host-health.md); ADR-011. Done against the original S0-4 text (minimal APIs, the same tests). The revised text above (MVC controllers, exception filter, §2a layout) is tracked as **S0-11**. Anonymous requests to unknown `/api` routes get 401 by design (ADR-011).

### ✅ S0-5 Vue 3 app and tooling (E46)

Tests first (Vitest + Testing Library + MSW):
- `App shows the health status returned by the API`.
- `App shows an error state when the API fails`.
- Lint check fails on an intentional `any` (verify then remove).

Implement: Vite + Vue 3 + TypeScript strict, Pinia, Vue Router, Tailwind CSS, ESLint flat config, Prettier (+ Tailwind plugin), `vue-tsc`, Vitest, MSW, generated API types from OpenAPI, a typed `apiFetch` wrapper.

Done when: `pnpm lint`, `format:check`, `typecheck`, `test --run`, `build` all pass; `make api-client-check` passes.

Evidence: [docs/evidence/s00/S0-5-vue-app.md](../evidence/s00/S0-5-vue-app.md); ADR-002 (pnpm and release-age policy); npm baseline in ADR-010.

### ✅ S0-6 OpenVue spike and UI foundation (E46, ADR-001)

Time-box: half a day for the spike, then implement.

Tests first:
- A component test renders a button, input and dialog from the chosen library in unstyled mode and asserts Tailwind classes are applied through pass-through.
- A test asserts an axe check passes for the sample page.

Implement: confirm the OpenVue package name, version, licence and that unstyled mode works with Tailwind; if not viable, document and switch to the in-house PrimeVue fork (see `/docs/adr/ADR-001`). Create `src/web/src/ui/` wrappers (`AppButton`, `AppInput`, `AppDialog`, `AppDataTable`) and a layout shell.

Done when: ADR-001 written with decision and evidence; wrappers tested.

Evidence: [docs/evidence/s00/S0-6-ui-foundation.md](../evidence/s00/S0-6-ui-foundation.md); decision in [ADR-001](../adr/001-ui-library.md).

### ✅ S0-7 .NET build builds and serves the frontend (E46)

Tests first (`Crm.Host.Tests`, category `Integration`):
- `Get_Root_ReturnsSpaIndexHtml` after `dotnet build` (test fails if `wwwroot/index.html` is absent).
- `Get_ClientRoute_ReturnsIndexHtml` (history-mode fallback).
- `Get_UnknownApiRoute_Returns404ProblemDetailsNotHtml`.
- `publish_output_contains_wwwroot_index` (`scripts/checks/publish-contains-spa.sh` run by `make verify`).
- Incremental build: second `dotnet build` does not re-run the frontend when inputs are unchanged (check build log).

Implement: `BuildFrontend` and `IncludeFrontendInPublish` targets, Vite `outDir`, `UseDefaultFiles/UseStaticFiles/MapFallbackToFile`, `SkipFrontend` switch, `.gitignore` for `wwwroot`.

Done when: `make build`, `make run`, `make publish` demonstrate the demo; pitfall in `docs/environment.md` §4 confirmed or corrected in the doc.

Evidence: [docs/evidence/s00/S0-7-spa-hosting.md](../evidence/s00/S0-7-spa-hosting.md). `IncludeFrontendInPublish` not needed (SDK re-globs `wwwroot` at publish); static-asset manifest pitfall confirmed.

### ✅ S0-8 Dev loop and VS Code (E46)

Tests first: `scripts/checks/vscode-config.sh` validates JSON, that every `launch.json` `preLaunchTask` exists in `tasks.json`, and that tasks call `make`.

Implement: `.vscode` files, `make dev`, compound launch, Vite proxy, MSSQL extension profile, Test Explorer working for xUnit and Vitest.

Done when: demo step 5 performed and noted in the sprint log.

Evidence: [docs/evidence/s00/S0-8-dev-loop.md](../evidence/s00/S0-8-dev-loop.md). Demo step 5 was performed on 2026-11-19 (see the sprint log).

### ✅ S0-9 Test harness (E46)

Tests first:
- `SqlFixture_CreatesIsolatedDatabase_AndDropsIt` (two parallel test classes do not collide).
- `Respawn_ResetsData_BetweenTests`.
- `FakeAuth_UserWithRoles_IsReadByCurrentUser` (placeholder until S1 adds `ICurrentUser`; keep to the auth scheme).
- Architecture test examples (layering): `Controllers_DoNotReference_DbContextOrModels`, `Services_DoNotReference_HttpContextOrMvc`, `Models_AreInternal`, `Controllers_ReturnOnlyDtoTypes`; "no project references `Microsoft.EntityFrameworkCore.*` except module projects and Migrator", "Host does not call `Migrate`" (string scan).

Implement: `Crm.Platform.Testing` with `SqlFixture`, `TestWebAppFactory`, `FakeAuthHandler`, `Builders` with Bogus; test categories `Integration`, `Architecture`, `AiSmoke`.

Done when: `make test-int` runs against the compose database; documented in `docs/testing.md`.

Evidence: [docs/evidence/s00/S0-9-test-harness.md](../evidence/s00/S0-9-test-harness.md); guide in [docs/testing.md](../testing.md). Split into 9a (SQL fixture, Respawn, credentials from the environment), 9b (fake auth, `TestWebAppFactory`, Bogus builders) and 9c (architecture tests) to stay near ~400 lines each. Database per test class instead of per assembly (see evidence). Containers created before this story need a rebuild for `CRM_SQL_PASSWORD`.

### S0-10 CI, hooks and supply chain (E4, E39)

Tests first: a workflow lint (`actionlint` in `make lint` when present) and a deliberately vulnerable package check proving the audit step fails (verify then remove).

Implement: `.github/workflows/ci.yml` running `make verify` with a SQL Server service; `dotnet list package --vulnerable --include-transitive` and `pnpm audit` gates; Dependabot (or Renovate) for NuGet, npm, Actions and the devcontainer; secret scanning and push protection; CodeQL; pinned action SHAs; SBOM generation (CycloneDX); `.githooks`.

Done when: CI green on a PR; branch protection requires CI and one review; SBOM artifact produced.

### ✅ S0-11 Host on MVC controllers (E46): follow-up to the revised S0-4

S0-4 was built against its original text (minimal APIs). The revised pack moves the API to MVC controllers with the §2a layering. Do this **before S0-9**, whose layering architecture tests need controllers to exist.

Tests first:
- The existing host tests stay green unchanged: they are HTTP-level (health, fallback 401, unknown-route 404/401, Production 500 without stack trace, OpenAPI document, SPA hosting).
- `HealthController_IsOnlyEndpointUsingAnonymousPolicy` (architecture test: the `Anonymous` policy appears only on `HealthController` and the SPA fallback).
- `ServiceException_IsConvertedToProblemDetails_ByExceptionFilter` (a throwaway controller in the test host throws `NotFoundException`, which returns 404 problem details).

Implement: `AddControllers` with `[ApiController]`, `HealthController` at `api/v1/health` with `[Authorize(Policy = HostPolicies.Anonymous)]`, the platform exception filter and typed exceptions, `Controllers/` and `Dtos/` folders in `Crm.Host` per §2a. Replace the `IApiEndpoints` test seam with a test-only controller added by `AddApplicationPart`. The OpenAPI document must still describe `/api/v1/health` (regenerate it and review the diff).

Done when: all S0-4 and S0-7 tests green without edits; ADR-011 updated for controllers; `make verify` green.

Evidence: [docs/evidence/s00/S0-11-controllers.md](../evidence/s00/S0-11-controllers.md); ADR-011 updated.

## Compliance evidence

Save under `docs/evidence/s00/`: CI run URL, dependency-audit output, branch protection screenshot or settings export, Dependabot config, SBOM sample.
Matrix rows: E8 patch applications (tooling), NIST 3.14.1 flaw remediation, 3.4.x configuration management (baseline configs), 3.13.x not yet.

## Definition of done

All stories ticked; demo performed; `make verify` green in CI on `main`; ADR-001 recorded; retrospective notes appended below.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md, docs/environment.md and docs/sprints/s00-walking-skeleton.md.
We are starting Sprint 0 in an empty repository. Check the entry criteria, then propose the order of stories
(I expect S0-1 → S0-2 → S0-3 → S0-4 → S0-5 → S0-7 → S0-6 → S0-8 → S0-9 → S0-10). Query the NuGet and npm feeds
for current stable versions rather than guessing. Do not write code until I confirm the order.
```

## Sprint log

- **2026-11-19, demo step 5 (S0-8), performed by the developer in VS Code; all parts passed.** Vue HMR under `make dev` (heading edit appeared in the browser without a reload). xUnit test debugged from Test Explorer (C# Dev Kit 3.40, Microsoft.Testing.Platform detected from `global.json`) and stopped at a breakpoint. Vitest test debugged from Test Explorer (Vitest Explorer 1.52) and stopped at a breakpoint in `health-store.ts`. MSSQL extension connected with profile `crm-dev` and ran `SELECT @@VERSION`. The host was debugged with `Full stack` and stopped at the health endpoint breakpoint. One finding: Vitest Explorer only activates if a Vite/Vitest config exists when the window opens, so a window opened before S0-5 needed **Developer: Reload Window**. Fresh clones are not affected.

## Retrospective

_(append notes here)_
