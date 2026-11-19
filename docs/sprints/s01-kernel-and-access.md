# Sprint 1 — Platform kernel and Access module (14 – 25 Dec 2026)

**Release**: R0. **Epics**: E2 (identity and access foundation), E36 (access control hardening), E46 (module template). Reduced capacity expected over Christmas.

## Goal

The module system exists and is enforced by tests. A user signs in with Entra ID (dev login in the devcontainer), is created in the Access module on first sign-in, and receives application roles. Passwords do not exist anywhere.

## Demo

1. `make module-new name=Sample` creates a compliant module; architecture tests pass; delete it.
2. Dev login as "Admin" and "Viewer"; the SPA shows the user's name and roles from `/api/v1/access/me`.
3. A Viewer calling an admin endpoint gets 403; an anonymous caller gets 401.
4. Show that no password endpoints exist and the Identity tables hold no password hashes.
5. Publish an integration event in module A and see module B's handler run exactly once after a restart replay.

## Entry criteria

- S0 done; `make verify` green on `main`.
- Entra test tenant or app registration available for integration (can be deferred to S3; dev login is enough for this sprint).

## Stories

**Agreed order (kickoff, 2026-11-19; revised and renumbered the same day after ADR-009).** Stories are numbered and listed in the order they are built: S1-1 → S1-2 → … → S1-10.
- S1-2 (current user) comes before S1-3 (mediator) because the mediator pipeline authorises as the current user (`ICurrentUser`).
- S1-3 (mediator) comes before S1-4 (scaffolder) because the mediator needs no modules or database, and the scaffolder's templates then generate handlers and their registration from the start.
- S1-4 (scaffolder) comes before S1-5 (module outbox) because each module's outbox, inbox and relay are template code, and their tests need two scaffolded modules with DbContexts and migrations (with the Migrator and the SQL fixture migrations step).
- S1-8 (security headers) comes before S1-9 (frontend session) because the frontend's antiforgery header, cookie session and Playwright journeys depend on the server-side headers, cookies and antiforgery.
- S1-4 and S1-6 are expected to be split (~400-line guideline). S1-10, and possibly S1-9's Playwright work, are the candidates to slip to S2 given reduced Christmas capacity.
- Decided at kickoff: cross-module communication only through an in-house `IMediator`; Contracts hold only messages; each module owns its outbox, inbox and relay, so `Crm.Platform` stays free of EF Core and the S0-9 rule is unchanged (ADR-009). S1-1, S1-3 and S1-5 below reflect it.
- Decision still needed: the idle session limit (S1-9, business owner).

**Renumbering.** Sprint 0 evidence notes and commits made before the renumbering use the old IDs:

| New | Old | Story |
| --- | --- | --- |
| S1-1 | S1-1 | Module contract and architecture tests |
| S1-2 | S1-4 | Current user and authorisation primitives |
| S1-3 | S1-3 (first half) | Mediator |
| S1-4 | S1-2 | Module scaffolder |
| S1-5 | S1-3 (second half) | Module outbox, inbox and relay |
| S1-6 | S1-5 | Access module |
| S1-7 | S1-6 | Dev login |
| S1-8 | S1-8 | Security headers and session hardening |
| S1-9 | S1-7 | Frontend session, route guards and shell |
| S1-10 | S1-9 | Role administration API |

### ✅ S1-1 Module contract and architecture tests (E46)

Tests first (`Crm.Architecture.Tests`):
- `ModuleImplementations_DoNotReference_OtherModuleImplementations`.
- `Contracts_Assemblies_DoNotReference_EfCore`.
- `Contracts_Assemblies_ContainOnlyMessagesDtosAndConstants` (query, command and event records, response DTOs, constants; no interfaces; ADR-009).
- `Modules_DoNotResolve_OtherModulesServices` (a module's service implementations and interfaces are internal; only Contracts types cross the boundary).
- `EachModule_HasOneDbContext_WithDefaultSchemaEqualToModuleSchema` (scans compiled model).
- `Module_Types_AreInternalUnlessInContracts`.
- Layering: `Controllers_DoNotReference_DbContextOrModels`, `Controllers_OnlyDependOnServiceInterfacesAndDtos`, `Services_DoNotReference_HttpContextOrMvcTypes`, `Models_AreInternalAndNotUsedInDtosOrControllerSignatures`, `Dtos_DoNotReference_Models`, `Controllers_HaveNoTryCatch` (IL or source scan), `ModuleFolders_Follow_ControllersServicesDtosModelsPersistenceHandlers`.
- `Host_ModulesList_MatchesModuleProjects`.

Implement: `IModule` (with `ControllersAssembly`, and a hook for registering mediator handlers and the outbox relay, used from S1-3), `ModuleRegistry`, host composition with `AddApplicationPart`, test helpers to load compiled models.

Done when: tests fail before any module exists, pass with the Access module and the sample module.

Evidence: [docs/evidence/s01/S1-1-module-contract.md](../evidence/s01/S1-1-module-contract.md); ADR-012. Split into S1-1a (module kernel), S1-1b (boundary rules) and S1-1c (layering rules). No shipped module exists yet, so every rule is also run against test-only fixture modules (`tests/Fixtures/Modules`: Sample obeys every rule, Rogue breaks each once). Module controllers and DTOs are internal and the platform discovers them (ADR-012). "Pass with the Access module" is confirmed in S1-6.

### ✅ S1-2 Current user and authorisation primitives (E2, E36)

Tests first:
- `CurrentUser_FromClaims_ExposesIdRolesTeamsAndBusinessUnits`.
- `FallbackPolicy_RequiresAuthenticatedUser`.
- `RequireRole_Policy_ForbidsOtherRoles`.
- Architecture: `All_Endpoints_HaveAuthorisation_OrAreInAnonymousAllowList` (allow-list: health).

Implement: `ICurrentUser`, `IUserContext` accessor, role constants (`Admin`, `Sales`, `Delivery`, `Finance`, `Auditor`, `ReadOnly`; refine in R1), policy registration helpers.

Evidence: [docs/evidence/s01/S1-2-current-user-and-authorisation.md](../evidence/s01/S1-2-current-user-and-authorisation.md). User IDs are GUIDs; teams and business units are `crm:` claims added by Access. The allow-list also holds the SPA shell and the `/api` catch-all, by ADR-011's design.

### ✅ S1-3 Mediator (E46)

Design: ADR-009. No I/O. Needs `ICurrentUser` from S1-2.

Tests first (unit, `Crm.Platform`):
- `Send_Query_InvokesTheSingleHandler_AndReturnsItsResponse`.
- `Send_WithNoHandler_Throws` and `Startup_RequestWithNoOrTwoHandlers_FailsValidation`.
- `Publish_Event_InvokesEveryHandler`.
- `Pipeline_RunsInOrder_CorrelationLoggingValidationAuthorisation`.
- `Send_RunsInCallersScope_CurrentUserIsTheSignedInUser`.
- Architecture: `EachRequest_HasExactlyOneHandler_InOwningModule`, `Services_DoNotCall_MediatorPublish` (the S0-9 EF Core rule already keeps `Crm.Platform` free of EF Core).

Implement: in `Crm.Platform` (no EF Core): `IMediator`, `IQuery<T>`, `ICommand<T>`, `IIntegrationEvent`, `IRequestHandler<,>`, `IEventHandler<T>`, pipeline behaviours, handler registration through `IModule`, startup validation of handlers, message envelope (ID, type, occurred-at, payload JSON, correlation ID, causation ID, originating user ID).

Done when: test-assembly handlers answer a query and receive an event through the mediator; pipeline order and the authorisation negative test pass; S1-4's templates can register handlers.

Evidence: [docs/evidence/s01/S1-3-mediator.md](../evidence/s01/S1-3-mediator.md); ADR-009 updated. Split into S1-3a (core), S1-3b (pipeline and host) and S1-3c (architecture rules). `Publish` is on a separate `IEventPublisher`; subscribers are `IEventSubscriber<T>`; requests name policies with `[RequiresPolicy]`; `IModule` gained `ContractsAssembly`. Modules register handlers in `AddServices` with `AddRequestHandler<T>()` and `AddEventSubscriber<T>()`.

**Added scope (2026-11-19, at the developer's direction): global exception handler.** Typed exceptions were mapped only by an MVC filter, so the same exception thrown outside a controller action became a 500. Replaced by one `IExceptionHandler` (`CrmExceptionHandler`, `AddCrmErrorHandling`) covering every request; the filter is removed; expected 4xx are not logged as faults. Exception use across the code base reviewed; no further changes needed. Evidence: [docs/evidence/s01/S1-3-global-exception-handler.md](../evidence/s01/S1-3-global-exception-handler.md).

### S1-4 Module scaffolder (E46)

Tests first: `scripts/checks/new-module.sh` runs `make module-new name=Zzz` in a temp copy, then `dotnet build` and `make test-arch` succeed; the generated module has Contracts, implementation, test project, DbContext, initial migration, endpoints stub, `Skills/` folder and web feature folder.

Implement: `scripts/new-module.sh` with templates (Controllers, Services, Dtos, Models, Persistence, Mapping, Validation, Handlers folders each with one example class and tests; handler registration from S1-3; S1-5 adds the outbox, inbox and relay to Persistence) under `scripts/templates/module/`; adds projects to the solution, host module list and Migrator.

Done when: the scaffolder is used to create Access in S1-6 (dog-food).

### S1-5 Module outbox, inbox and relay (E46)

Design: ADR-009. Each module owns this code, generated from the scaffolder templates (S1-4). Needs two scaffolded modules with migrations.

Tests first (persistence, real SQL; written once as a conformance suite in `Crm.Platform.Testing` and run by each module's test project against its own `DbContext`):
- `Outbox_Message_IsWrittenInSameTransactionAsEntityChange` (roll back → no message).
- `Relay_PublishesPendingMessages_ThroughMediator_AndMarksProcessed`.
- `Relay_Retries_FailedHandler_WithBackoff_ThenDeadLetters`.
- `Handler_ReceivingSameMessageTwice_RunsOnce` (inbox in the subscriber's schema).
- `TwoRelays_ForSameModule_OnlyOneProcesses` (`sp_getapplock`, per-module resource).
- `Relay_ResumesAfterRestart_WithoutLosingMessages`.
- `scripts/checks/module-outbox-drift.sh` (in `make verify`) fails on a planted change to one module's outbox code.

Implement: in `scripts/templates/module/Persistence/`: outbox and inbox entities and configuration, `SaveChanges` hook, `IOutbox` (module-internal), inbox guard, hosted relay publishing through `IMediator`. The conformance suite in `Crm.Platform.Testing`. Regenerate the sample modules from the template.

Done when: two sample modules exchange a query and an event through the mediator in an API-level test, with neither referencing the other's implementation.

### S1-6 Access module with ASP.NET Core Identity, external login only (E2)

Tests first:
- `FirstExternalSignIn_CreatesUser_WithEntraObjectIdAndNoPasswordHash`.
- `SecondSignIn_UpdatesDisplayNameAndEmail_DoesNotDuplicate`.
- `DisabledUser_IsRejected_OnSignIn`.
- `RolesFromEntraAppRoles_MapToApplicationRoles` (claims `roles`).
- `PasswordEndpoints_DoNotExist` (call `/Account/Login`, `/register`, `/forgotPassword`, `/manage`, Identity API routes → 404).
- `IdentityOptions_AllowNoPasswordSignIn` and `Schema_HasNoUsableCredentialColumns` (PasswordHash always null across the table after any test run).
- Migrations: `AccessMigrations_ApplyToEmptyDatabase` and `NoPendingModelChanges`.

Implement: `User` model and `AccessDbContext : IdentityDbContext`, `UsersController`, `IUserService`, DTOs and mappers;  in schema `access`; `SignInManager` external login only; Microsoft.Identity.Web OIDC registration (config-driven, validated at startup); cookie session; `/api/v1/access/me`; user disable/enable by admin (stub UI later); tenant ID check (only your tenant; reject guests unless configured).

Done when: tests green; ADR-004 updated with what Identity features are intentionally unused.

### S1-7 Dev login (E2)

Tests first:
- `DevLogin_InDevelopmentWithFlag_SignsInSelectedPersona`.
- `DevLogin_InAnyOtherEnvironment_FailsStartupValidation` (Production, Staging, Test-Prod).
- `DevLogin_Endpoint_NotMapped_WhenDisabled`.

Implement: personas (Admin, Sales, Delivery, Finance, Auditor, ReadOnly) seeded in dev; `make dev` uses them; Playwright and API tests use the same mechanism.

### S1-8 Security headers and session hardening (E36)

Tests first (API):
- `Responses_IncludeSecurityHeaders` (CSP with nonce or hashes, HSTS, X-Content-Type-Options, Referrer-Policy, frame-ancestors none, Permissions-Policy).
- `Cookies_AreSecureHttpOnlySameSiteStrict`.
- `StateChangingRequest_WithoutAntiforgeryToken_Returns400or403`.
- `Cors_IsDisabled_ForUnknownOrigins`.

Implement: middleware and Vite-compatible CSP for dev.

### S1-9 Frontend session, route guards and shell (E2, E46)

Tests first (Vitest):
- `SessionStore_LoadsMe_AndExposesRoles`.
- `RouterGuard_RedirectsUnauthenticated_ToSignIn`.
- `Nav_HidesItems_WhenRoleMissing`.
- `Antiforgery_Header_IsSentOnMutations`.
- Playwright: dev login as Admin, see the shell; as ReadOnly, admin menu absent.

Implement: session Pinia store, guards, app shell with user menu, sign-out, session-expiry handling (idle 15 minutes with warning; confirm limit with business owner).

### S1-10 Role administration API (E2)

Tests first: admin can list users and disable a user; non-admin cannot; every change emits an integration event `UserDisabled` (consumed by Audit in S2); optimistic concurrency conflict returns 409.

## Compliance evidence

`docs/evidence/s01/`: architecture test output, password-endpoint negative test results, security header scan (`curl -I` output), role matrix draft.
Matrix rows: NIST 3.1.1, 3.1.2, 3.1.5 (least privilege), 3.5.1–3.5.3 (identification and authentication via Entra), 3.13.15 (session authenticity), E8 MFA and restrict admin privileges (application part).

## Definition of done

Stories ticked; `make verify` green; module scaffolder used for Access; architecture tests active in CI; no passwords possible; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s01-kernel-and-access.md.
Check entry criteria (is S0 merged and make verify green?). Follow the agreed story order in the Stories section (S1-1, S1-2, S1-3, S1-4, S1-5, S1-6, S1-7, S1-8, S1-9, S1-10).
Start with S1-1: restate it, list the first three tests, write the first failing test, run it, then proceed with TDD.
```

## Retrospective

_(append notes here)_
