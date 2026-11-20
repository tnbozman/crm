# Architecture

Companion to `CLAUDE.md`. Read before Sprint 0 and whenever a story touches module boundaries.

## 1. Shape

One deployable (Azure App Service) and one Azure SQL database, built from modules (ADR-013). Each module is one class library that owns its code, its endpoints, one EF Core `DbContext` and one SQL schema; no module project references another. The Vue 3 app is built by the .NET build into `Crm.Host/wwwroot` and served by the same host.

```
src/
  Crm.Host/                  web API, composition root: references every module, serves wwwroot
  Crm.Migrator/              console app; applies every module's migrations
  Crm.SharedKernel/          common features, no endpoints or data stores: IModule and composition, ICurrentUser,
                             roles and policies, typed errors and the global exception handler, AI abstractions
  Crm.Mediator/              how modules talk (refs SharedKernel): IMediator, IEventPublisher, pipeline, envelope
    Contracts/<Module>/      every cross-module message, namespace Crm.Mediator.Contracts.<Module>
                             (query, command and event records, response DTOs, constants; no interfaces)
  Crm.Testing/               SQL fixture, fake auth, builders, log capture, test module; outbox conformance suite (S1-5)
  Modules/Crm.<Module>/      one class library per module (refs only SharedKernel and Mediator), layered by folder:
      Controllers/           ASP.NET Core controllers: HTTP only, thin
      Services/              business logic and transactions (interface + implementation)
      Dtos/                  request and response DTOs for this module's HTTP API
      Models/                EF Core entity classes (never leave the module)
      Persistence/           DbContext, IEntityTypeConfiguration classes, Migrations, outbox, inbox and relay
      Mapping/               explicit mappers between Models and DTOs
      Validation/            request validators
      Handlers/              mediator handlers: queries and commands in Crm.Mediator.Contracts.<Module>, and integration events it subscribes to
      Skills/                AI skills and tools (R6)
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

## 2a. Layering inside a module

```
HTTP request → Controller → Service → DbContext (Models) → SQL Server
                  ↑ DTOs in/out          ↑ Models in/out
```

| Layer | Responsibility | May reference | Must not |
| --- | --- | --- | --- |
| Controller | Routing, authorisation attributes, model binding, status codes, calling one service method, returning DTOs | Services (interfaces), Dtos, `ICurrentUser` | Touch `DbContext` or Models; contain business rules; catch exceptions to shape errors (the global exception handler does that) |
| Service | Business rules, orchestration, transactions, writing integration events to the module's outbox, sending other modules' queries and commands (`Crm.Mediator.Contracts`) through `IMediator` | `DbContext`, Models, Dtos, Mapping, `IMediator`, `Crm.Mediator.Contracts` | Reference `HttpContext`, controllers or ASP.NET types; return Models to controllers; call `IMediator.Publish`; inject another module's services |
| DTO | Shape of the HTTP contract: records with data annotations or validators, no behaviour | Nothing | Reference Models |
| Model | EF Core entity: state plus invariants enforced through methods; private setters, no public parameterless constructor outside EF | Nothing outside the module | Be serialised or returned from any API; be used by other modules |
| Mapper | Explicit static or injected mapping code (no reflection mapper by default) | Models, Dtos | Contain logic or queries |
| Handler | Entry point for this module's Contracts queries and commands, and for other modules' integration events; thin, delegates to services | Services (interfaces), Contracts, inbox | Contain business rules; touch another module's anything except its Contracts |

Rules:
- No generic repository. Services use the module's `DbContext` directly; this keeps queries explicit and testable against real SQL.
- Services expose an interface (`IOrganisationService`) used by controllers and handlers inside the module. Cross-module access is a message in `Crm.Mediator.Contracts.<Owner>` (`GetOrganisationSummary : IQuery<OrganisationSummary>`) sent through `IMediator` and handled in the owning module by a thin handler that delegates to its services (ADR-009). Message response records are distinct from the module's internal API DTOs.
- Queries project to DTOs (`Select`) rather than loading whole Models where the response is read-only.
- Services throw typed exceptions from `Crm.SharedKernel.Errors` (`NotFoundException` 404, `ForbiddenException` 403, `RequestValidationException` 400 with field errors; `ConflictException` 409 arrives with optimistic concurrency). One global `IExceptionHandler` (`CrmExceptionHandler`, registered by `AddCrmErrorHandling` and run by `UseExceptionHandler`) maps them to RFC 9457 problem details for every request: controllers, minimal endpoints and middleware alike. Their messages never reach the response, and they are not logged as faults. Any other exception is a bug: 500 with no detail, logged as an error. Controllers contain no try/catch.
- Controllers use `[ApiController]`, attribute routing (`[Route("api/v1/<module>/<resource>")]`), `[Authorize]` with named policies, `CancellationToken` parameters, `ActionResult<T>` or `[ProducesResponseType]` for OpenAPI accuracy.
- Models carry `OwnerTeamId`/`BusinessUnitId` where access-controlled; query filters live in the DbContext.
- Module projects register their controllers with `AddApplicationPart` (through `IModule.ControllersAssembly` and `AddModules`) so `Crm.Host` finds them without reflection scanning of the whole app.
- Every type in a module implementation is `internal` except its `IModule` class. Controllers and DTOs are internal too; `AddModules` adds a controller feature provider that discovers internal controllers in module assemblies only (ADR-012).

## 3. Module contract

Each module exposes one `IModule` implementation in its implementation project:

```csharp
public interface IModule
{
    string Name { get; }                       // "Directory"; schema is module.Schema() => "directory"
    Assembly ControllersAssembly { get; }      // registered as an application part; routes under /api/v1/<module>
    void AddServices(IServiceCollection services, IConfiguration configuration);  // services, DbContext, handlers
}
```

The Migrator finds a module's DbContexts among the services `AddServices` registers (ADR-014).

`Crm.Host` discovers modules explicitly (`HostModules.All`, not reflection scanning) so the dependency graph is visible. `Crm.Migrator` keeps its own list (`MigratorModules.All`) so it does not reference the web host; architecture tests keep both lists and both project files in step with `src/Modules`, and `make module-new` adds a new module to each (ADR-014).

**Contracts** contain: query/command interfaces (`IDirectoryQueries`), DTOs (records), integration events (`OrganisationCreated`), and constants. Nothing else.

## 4. Persistence

- One `DbContext` per module: `HasDefaultSchema("<schema>")`, `MigrationsHistoryTable("__EFMigrationsHistory", "<schema>")`, own migrations assembly.
- Primary keys are `Guid` (version 7) generated in the application.
- Every table carries `OwnerTeamId`, `BusinessUnitId` where records are access-controlled, plus `CreatedAt/By`, `UpdatedAt/By`, and a `rowversion` concurrency token.
- Custom fields: definitions in `config`; values in an `nvarchar(max)` JSON column on the owning row, validated through a `Crm.Mediator.Contracts.Config` query.
- Global query filters apply the current user's visibility scope in each DbContext. Negative tests prove another business unit cannot see the row.
- Migrations: `make migration-add module=<Module> name=<Name>` writes them to the module's `Persistence/Migrations` (internal classes). `Crm.Migrator` applies each module's migrations through the DbContexts the module registers in `AddServices` (ADR-014). The connection string is `ConnectionStrings:Crm` (`ModuleDatabase.ConnectionStringName`).
- Dev: `make migrate`, `make db-reset` (refused outside Development). Azure: `Crm.Migrator` run by the deployment identity. The runtime identity has no DDL rights; a startup test asserts the host never calls `Migrate()`.

## 5. Communication

| Need | Mechanism |
| --- | --- |
| Ask another module for data | `IMediator.Send` with the owning module's Contracts query, in-process |
| Ask another module to do something now | `IMediator.Send` with a Contracts command; its own transaction, idempotent handler, never inside the caller's transaction; prefer events |
| Tell other modules something changed | Integration event through the module's own outbox; its relay calls `IEventPublisher.Publish` after the commit |
| Cross-module screen (for example organisation overview) | Composition in `Crm.Host` sending several Contracts queries; never a cross-schema join |
| Cross-module consistency | Eventual, through events with idempotent handlers (inbox table per subscriber) |

There is no other channel between modules: no injected services from another module, no shared persistence code (ADR-009).

Mediator: `IMediator` (`Send`) and `IEventPublisher` (`Publish`) in `Crm.Mediator`, written in-house. Every `Send` runs a pipeline: correlation, structured logging (type and correlation ID only), validation of the request's data annotations, authorisation as the current user (`[RequiresPolicy]`, otherwise signed in), and audit of permission-relevant reads (S2). `Publish` sets correlation and causation from the envelope. Modules register handlers in `IModule.AddServices` with `AddRequestHandler<T>()` and `AddEventSubscriber<T>()`. Each request type has exactly one handler, in the module that owns it; startup fails otherwise.

Outbox: each module owns `outbox_messages` and `inbox_messages` in its schema, mapped in its own `DbContext`. Outbox rows are written by a `SaveChanges` hook in the same transaction as the change. A hosted relay registered by the module reads only its own outbox, takes `sp_getapplock` on a per-module resource so one instance relays, calls `IMediator.Publish`, retries with backoff, dead-letters, and marks messages done. Subscribers record processed message IDs in their own inbox in the same transaction as their change. The scaffolder generates this code; a conformance suite in `Crm.Testing` and a drift check in `make verify` keep the copies identical.

## 6. Identity and access

- ASP.NET Core Identity is used **in external-login-only mode**: `AccessDbContext` derives from `IdentityDbContext`, users are created on first Entra sign-in, `PasswordHash` is always null, no password, reset or registration endpoints exist.
- Sign-in: OpenID Connect to Entra ID (Microsoft.Identity.Web), cookie session (`SameSite=Strict`, `Secure`, `HttpOnly`), antiforgery token for state-changing calls from the SPA.
- Authorisation: Entra **app roles** map to application roles. Teams and business units are managed in the Access module and assigned to users there (avoids the Entra group-overage limit).
- `ICurrentUser` exposes user ID (GUID), roles, team IDs, business-unit IDs. Every module reads it from the Platform kernel. It is built once per scope from `IUserContext` (the request's user by default; work outside a request supplies its own principal). An authenticated principal without a GUID user ID fails closed.
- Roles (`Admin`, `Sales`, `Delivery`, `Finance`, `Auditor`, `ReadOnly`) are constants in `Crm.SharedKernel.Security.Roles`. `AddCrmAuthorization()` registers the fallback policy and one policy per role (`Policies.Admin` and so on); `AddRolePolicy` adds others. Endpoints use `[Authorize(Policy = Policies.X)]`. An architecture test requires every endpoint to name a policy unless it is on the allow-list (health, SPA shell, `/api` catch-all).
- Service-to-service callers (R4) use Entra app registrations or managed identities with JWT bearer validation. No custom API keys.
- Conditional Access (phishing-resistant MFA, compliant device) is configured in Entra and recorded as evidence in S3.

## 7. API

- MVC controllers per resource under `/api/v1/<module>/<resource>`, one controller class per resource, DTOs in and out, never EF models.
- OpenAPI generated at build; `make api-client` produces `src/web/src/api/` TypeScript types. CI fails on drift.
- RFC 9457 problem details through the global exception handler and the problem details service, request validation (data annotations for simple rules, validators for rules needing services) at the boundary, ETag/rowversion for optimistic concurrency.
- Fallback authorisation policy requires an authenticated user.
- Anonymous callers get 401 for every `/api` route except `/api/v1/health`, including routes that do not exist, so routes cannot be probed. Signed-in callers get 404 problem details for unknown routes. Opening an endpoint means naming a policy in `HostPolicies` (only `Anonymous`, used by health and the SPA shell fallback), with a test. With controllers, that is `[Authorize(Policy = HostPolicies.Anonymous)]`, not `[AllowAnonymous]` (ADR-011). The built SPA in `wwwroot` is served as public static files.
- The OpenAPI document is committed at `openapi/Crm.Host.json` and regenerated by `make api-client`.

## 8. AI assistant (built S25–S28, spiked S2)

- Microsoft Agent Framework hosted in `Crm.Assistant`. Providers behind `IChatClient`: Azure OpenAI, OpenAI, Anthropic (direct), Claude in Microsoft Foundry. Config key `Ai:Provider`.
- Production default is Azure OpenAI in an Australian region over a private endpoint with Entra authentication. Direct OpenAI and Anthropic APIs are for development with synthetic data; startup validation blocks them in Production unless `Ai:AllowExternalProviders=true` is set after approval.
- Skills: `SKILL.md` files per module (`src/Modules/<Module>/Crm.<Module>/Skills/`), loaded by role through the Agent Framework skills provider. Script execution is disabled.
- Tools: typed C# functions sending `Crm.Mediator.Contracts` queries and commands through `IMediator` as the signed-in user. Reads run freely; writes return a proposal that the user approves in the UI.
- Prompts and tool calls are audited. Records with handling markers are never sent to a model.

## 9. Testing architecture

| Layer | Tool | Database |
| --- | --- | --- |
| Models (invariants), mappers, validators | xUnit v3, Shouldly | none |
| Services | xUnit v3, Shouldly, SQL fixture, Respawn | real SQL Server, module schema |
| Controllers (HTTP behaviour) | `WebApplicationFactory`, fake auth scheme | real SQL Server |
| Persistence (configurations, migrations, filters) | xUnit v3, SQL fixture | real SQL Server, module schema |
| Architecture | NetArchTest | none |
| Frontend unit and component | Vitest, Testing Library, MSW | none |
| End to end | Playwright (few journeys) | real stack, dev login |

The SQL fixture (`Crm.Testing.SqlFixture`) reads `CRM_SQL_SERVER`, `CRM_SQL_USER` and `CRM_SQL_PASSWORD` (devcontainer: the `db` service; CI: service container). It creates a uniquely named database **per test class** (so parallel classes cannot collide under Respawn) and resets data with Respawn between tests. A module's tests apply its migrations to the fixture database (the scaffolded `<Module>Database` fixture). See `docs/testing.md`.

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
| ADR-009 | Cross-module mediator | All cross-module calls through an in-house `IMediator`; Contracts hold only messages; each module owns its outbox, inbox and relay (**accepted**, S1 kickoff) |
| ADR-010 | Package versions | Central package management; versions from the feed, logged with date (**accepted**, S0-2) |
| ADR-011 | API access defaults | Deny by default; open only via named `HostPolicies`; anonymous unknown routes get 401 (**accepted**, S0-4) |
| ADR-012 | Module type visibility | Only the `IModule` class is public in a module implementation; internal controllers discovered in module assemblies (**accepted**, S1-1) |
| ADR-013 | Project structure | Host, SharedKernel, Mediator (with every cross-module message), one library per module referencing only SharedKernel and Mediator (**accepted**, S1-3) |
| ADR-014 | Module scaffolding and migrations | `make module-new` templates; the Migrator migrates the DbContexts each module registers; internal migrations, history per schema; reset in Development only (**accepted**, S1-4) |
