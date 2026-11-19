# Architecture

Companion to `CLAUDE.md`. Read before Sprint 0 and whenever a story touches module boundaries.

## 1. Shape

One deployable (Azure App Service) and one Azure SQL database, built from modules. Each module owns its code, one EF Core `DbContext` and one SQL schema. The Vue 3 app is built by the .NET build into `Crm.Host/wwwroot` and served by the same host.

```
src/
  Crm.Host/                  composition root, endpoints mapping, serves wwwroot
  Crm.Migrator/              console app; applies every module's migrations
  Crm.Platform/              kernel: IModule, event bus, outbox/inbox, ICurrentUser, auth, AI abstractions
  Crm.Platform.Testing/      SQL fixture, fake auth, builders, event-bus test double
  Modules/<Module>/
    Crm.<Module>.Contracts/  public API of the module
    Crm.<Module>/            internal implementation (internal visibility by default)
  web/                       Vue 3 app (Vite)
tests/
  Crm.Architecture.Tests/
  Crm.Host.Tests/            cross-module API tests
  Modules/<Module>/Crm.<Module>.Tests/
infra/                       Bicep
docs/
```

## 2. Modules

| Module | Schema | Owns | Sprint |
| --- | --- | --- | --- |
| Access | `access` | Identity (external login only), app roles, teams, business units, visibility scopes | S1 |
| Audit | `audit` | Append-only change and access log | S2 |
| Config | `config` | Property definitions, validations, pipeline and stage definitions, custom object types | S4 |
| Directory | `directory` | Organisations, people, links, relationships | S5 |
| Activities | `activities` | Notes, tasks, notifications | S6 |
| Reporting | `reporting` | Search, dashboard and forecast projections | S7, S21 |
| Deals | `deals` | Deals, deal roles, bids, stakeholders | S9 |
| Commerce | `commerce` | Products, price books, quotes | S13 |
| Programmes | `programmes` | Grants, research programmes, partners | S15 |
| Probity | `probity` | Conflict declarations and reviews | S15 |
| Automation | `automation` | Rules, webhooks, hand-offs | S17 |
| Assistant | `assistant` | Skills, conversations, approvals | S25 |

## 3. Module contract

Each module exposes one `IModule` implementation in its implementation project:

```csharp
public interface IModule
{
    string Name { get; }                       // "Directory"
    string Schema { get; }                     // "directory"
    void AddServices(IServiceCollection services, IConfiguration config);
    void MapEndpoints(IEndpointRouteBuilder routes);   // under /api/v1/<module>
    IEnumerable<Type> DbContextTypes { get; }
}
```

`Crm.Host` discovers modules explicitly (a list in `Program.cs`, not reflection scanning) so the dependency graph is visible. `Crm.Migrator` uses the same list.

**Contracts** contain: query/command interfaces (`IDirectoryQueries`), DTOs (records), integration events (`OrganisationCreated`), and constants. Nothing else.

## 4. Persistence

- One `DbContext` per module: `HasDefaultSchema("<schema>")`, `MigrationsHistoryTable("__EFMigrationsHistory", "<schema>")`, own migrations assembly.
- Primary keys are `Guid` (version 7) generated in the application.
- Every table carries `OwnerTeamId`, `BusinessUnitId` where records are access-controlled, plus `CreatedAt/By`, `UpdatedAt/By`, and a `rowversion` concurrency token.
- Custom fields: definitions in `config`; values in an `nvarchar(max)` JSON column on the owning row, validated through `Crm.Config.Contracts`.
- Global query filters apply the current user's visibility scope in each DbContext. Negative tests prove another business unit cannot see the row.
- Dev: `make db-reset`. Azure: `Crm.Migrator` run by the deployment identity. The runtime identity has no DDL rights; a startup test asserts the host never calls `Migrate()`.

## 5. Communication

| Need | Mechanism |
| --- | --- |
| Ask another module for data | Contracts query interface, in-process |
| Tell other modules something changed | Integration event through the module's outbox |
| Cross-module screen (for example organisation overview) | Composition in `Crm.Host` calling several Contracts interfaces; never a cross-schema join |
| Cross-module consistency | Eventual, through events with idempotent handlers (inbox table per subscriber) |

Outbox: each module has `outbox_messages` in its schema, written in the same transaction as the change. A hosted dispatcher in `Crm.Platform` polls all modules' outboxes, takes `sp_getapplock` so only one instance dispatches, publishes to in-process handlers, and marks messages done. Handlers record processed message IDs in an `inbox_messages` table in the subscriber's schema.

## 6. Identity and access

- ASP.NET Core Identity is used **in external-login-only mode**: `AccessDbContext` derives from `IdentityDbContext`, users are created on first Entra sign-in, `PasswordHash` is always null, no password, reset or registration endpoints exist.
- Sign-in: OpenID Connect to Entra ID (Microsoft.Identity.Web), cookie session (`SameSite=Strict`, `Secure`, `HttpOnly`), antiforgery token for state-changing calls from the SPA.
- Authorisation: Entra **app roles** map to application roles. Teams and business units are managed in the Access module and assigned to users there (avoids the Entra group-overage limit).
- `ICurrentUser` exposes user ID, roles, team IDs, business-unit IDs. Every module reads it from the Platform kernel.
- Service-to-service callers (R4) use Entra app registrations or managed identities with JWT bearer validation. No custom API keys.
- Conditional Access (phishing-resistant MFA, compliant device) is configured in Entra and recorded as evidence in S3.

## 7. API

- Minimal APIs grouped per module under `/api/v1/<module>`.
- OpenAPI generated at build; `make api-client` produces `src/web/src/api/` TypeScript types. CI fails on drift.
- RFC 9457 problem details, FluentValidation (or built-in validation) at the boundary, ETag/rowversion for optimistic concurrency.
- Fallback authorisation policy requires an authenticated user.
- Anonymous callers get 401 for every `/api` route except `/api/v1/health`, including routes that do not exist, so routes cannot be probed. Signed-in callers get 404 problem details for unknown routes. Opening an endpoint means naming a policy in `HostPolicies` (only `Anonymous`, used by health and the SPA shell fallback), with a test. The built SPA in `wwwroot` is served as public static files.
- The OpenAPI document is committed at `openapi/Crm.Host.json` and regenerated by `make api-client`.

## 8. AI assistant (built S25–S28, spiked S2)

- Microsoft Agent Framework hosted in `Crm.Assistant`. Providers behind `IChatClient`: Azure OpenAI, OpenAI, Anthropic (direct), Claude in Microsoft Foundry. Config key `Ai:Provider`.
- Production default is Azure OpenAI in an Australian region over a private endpoint with Entra authentication. Direct OpenAI and Anthropic APIs are for development with synthetic data; startup validation blocks them in Production unless `Ai:AllowExternalProviders=true` is set after approval.
- Skills: `SKILL.md` files per module (`src/Modules/<Module>/Crm.<Module>/Skills/`), loaded by role through the Agent Framework skills provider. Script execution is disabled.
- Tools: typed C# functions calling module Contracts as the signed-in user. Reads run freely; writes return a proposal that the user approves in the UI.
- Prompts and tool calls are audited. Records with handling markers are never sent to a model.

## 9. Testing architecture

| Layer | Tool | Database |
| --- | --- | --- |
| Domain and application | xUnit v3, Shouldly, NSubstitute | none |
| Persistence | xUnit v3, SQL fixture, Respawn | real SQL Server, module schema |
| API | `WebApplicationFactory`, fake auth scheme | real SQL Server |
| Architecture | NetArchTest | none |
| Frontend unit and component | Vitest, Testing Library, MSW | none |
| End to end | Playwright (few journeys) | real stack, dev login |

The SQL fixture reads `CRM_SQL_SERVER` (devcontainer: the `db` service; CI: service container), creates a uniquely named database per test assembly, applies migrations through `Crm.Migrator`, and uses Respawn between tests. Testcontainers is an acceptable alternative but needs Docker access from inside the devcontainer.

## 10. Decisions to confirm (record outcomes in `docs/adr/`)

| # | Decision | Default in this plan |
| --- | --- | --- |
| ADR-001 | UI library | OpenVue 1.1.1 (MIT PrimeVue 4 fork), unstyled + Tailwind pass-through; only `src/web/src/ui` imports it (**accepted**, S0-6) |
| ADR-002 | Package manager | pnpm, exact pins, release-age policy kept on (**accepted**, S0-5) |
| ADR-003 | Test framework | xUnit v3 + Shouldly + NSubstitute on Microsoft.Testing.Platform (**accepted**, S0-2) |
| ADR-004 | Identity | ASP.NET Core Identity, external login only |
| ADR-005 | Migrations in production | `Crm.Migrator` run by the deployment identity (not bundles) |
| ADR-006 | Search | SQL `LIKE` over normalised columns for MVP; evaluate full-text in S7 |
| ADR-007 | PDF generation (quotes) | Decide in S14; check licence terms first |
| ADR-008 | AI providers in production | Azure OpenAI (AU region); direct APIs development-only |
| ADR-009 | Mediator library | None; small in-house dispatcher |
| ADR-010 | Package versions | Central package management; versions from the feed, logged with date (**accepted**, S0-2) |
| ADR-011 | API access defaults | Deny by default; open only via named `HostPolicies`; anonymous unknown routes get 401 (**accepted**, S0-4) |
