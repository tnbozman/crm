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

### S1-1 Module contract and architecture tests (E46)

Tests first (`Crm.Architecture.Tests`):
- `ModuleImplementations_DoNotReference_OtherModuleImplementations`.
- `Contracts_Assemblies_DoNotReference_EfCore`.
- `Contracts_Assemblies_ContainOnlyInterfacesRecordsEventsAndConstants`.
- `EachModule_HasOneDbContext_WithDefaultSchemaEqualToModuleSchema` (scans compiled model).
- `Module_Types_AreInternalUnlessInContracts`.
- `Host_ModulesList_MatchesModuleProjects`.

Implement: `IModule`, `ModuleRegistry`, host composition, test helpers to load compiled models.

Done when: tests fail before any module exists, pass with the Access module and the sample module.

### S1-2 Module scaffolder (E46)

Tests first: `scripts/checks/new-module.sh` runs `make module-new name=Zzz` in a temp copy, then `dotnet build` and `make test-arch` succeed; the generated module has Contracts, implementation, test project, DbContext, initial migration, endpoints stub, `Skills/` folder and web feature folder.

Implement: `scripts/new-module.sh` with templates under `scripts/templates/module/`; adds projects to the solution, host module list and Migrator.

Done when: the scaffolder is used to create Access in S1-5 (dog-food).

### S1-3 Domain events, outbox and inbox (E46)

Tests first (persistence, real SQL):
- `Outbox_Message_IsWrittenInSameTransactionAsEntityChange` (roll back → no message).
- `Dispatcher_PublishesPendingMessages_ToSubscribers_AndMarksProcessed`.
- `Dispatcher_Retries_FailedHandler_WithBackoff_ThenDeadLetters`.
- `Handler_ReceivingSameMessageTwice_RunsOnce` (inbox).
- `TwoDispatchers_OnlyOneProcesses` (`sp_getapplock`).
- `Dispatcher_ResumesAfterRestart_WithoutLosingMessages`.

Implement: `IIntegrationEvent`, `IOutbox`, `OutboxInterceptor` or `SaveChanges` hook per module context, `IEventHandler<T>`, `InboxGuard`, hosted `OutboxDispatcher` in `Crm.Platform`. Message envelope: ID, type, occurred-at, payload JSON, correlation ID, causation ID.

Done when: two sample modules exchange an event in an API-level test.

### S1-4 Current user and authorisation primitives (E2, E36)

Tests first:
- `CurrentUser_FromClaims_ExposesIdRolesTeamsAndBusinessUnits`.
- `FallbackPolicy_RequiresAuthenticatedUser`.
- `RequireRole_Policy_ForbidsOtherRoles`.
- Architecture: `All_Endpoints_HaveAuthorisation_OrAreInAnonymousAllowList` (allow-list: health).

Implement: `ICurrentUser`, `IUserContext` accessor, role constants (`Admin`, `Sales`, `Delivery`, `Finance`, `Auditor`, `ReadOnly`; refine in R1), policy registration helpers.

### S1-5 Access module with ASP.NET Core Identity, external login only (E2)

Tests first:
- `FirstExternalSignIn_CreatesUser_WithEntraObjectIdAndNoPasswordHash`.
- `SecondSignIn_UpdatesDisplayNameAndEmail_DoesNotDuplicate`.
- `DisabledUser_IsRejected_OnSignIn`.
- `RolesFromEntraAppRoles_MapToApplicationRoles` (claims `roles`).
- `PasswordEndpoints_DoNotExist` (call `/Account/Login`, `/register`, `/forgotPassword`, `/manage`, Identity API routes → 404).
- `IdentityOptions_AllowNoPasswordSignIn` and `Schema_HasNoUsableCredentialColumns` (PasswordHash always null across the table after any test run).
- Migrations: `AccessMigrations_ApplyToEmptyDatabase` and `NoPendingModelChanges`.

Implement: `AccessDbContext : IdentityDbContext` in schema `access`; `SignInManager` external login only; Microsoft.Identity.Web OIDC registration (config-driven, validated at startup); cookie session; `/api/v1/access/me`; user disable/enable by admin (stub UI later); tenant ID check (only your tenant; reject guests unless configured).

Done when: tests green; ADR-004 updated with what Identity features are intentionally unused.

### S1-6 Dev login (E2)

Tests first:
- `DevLogin_InDevelopmentWithFlag_SignsInSelectedPersona`.
- `DevLogin_InAnyOtherEnvironment_FailsStartupValidation` (Production, Staging, Test-Prod).
- `DevLogin_Endpoint_NotMapped_WhenDisabled`.

Implement: personas (Admin, Sales, Delivery, Finance, Auditor, ReadOnly) seeded in dev; `make dev` uses them; Playwright and API tests use the same mechanism.

### S1-7 Frontend session, route guards and shell (E2, E46)

Tests first (Vitest):
- `SessionStore_LoadsMe_AndExposesRoles`.
- `RouterGuard_RedirectsUnauthenticated_ToSignIn`.
- `Nav_HidesItems_WhenRoleMissing`.
- `Antiforgery_Header_IsSentOnMutations`.
- Playwright: dev login as Admin, see the shell; as ReadOnly, admin menu absent.

Implement: session Pinia store, guards, app shell with user menu, sign-out, session-expiry handling (idle 15 minutes with warning; confirm limit with business owner).

### S1-8 Security headers and session hardening (E36)

Tests first (API):
- `Responses_IncludeSecurityHeaders` (CSP with nonce or hashes, HSTS, X-Content-Type-Options, Referrer-Policy, frame-ancestors none, Permissions-Policy).
- `Cookies_AreSecureHttpOnlySameSiteStrict`.
- `StateChangingRequest_WithoutAntiforgeryToken_Returns400or403`.
- `Cors_IsDisabled_ForUnknownOrigins`.

Implement: middleware and Vite-compatible CSP for dev.

### S1-9 Role administration API (E2)

Tests first: admin can list users and disable a user; non-admin cannot; every change emits an integration event `UserDisabled` (consumed by Audit in S2); optimistic concurrency conflict returns 409.

## Compliance evidence

`docs/evidence/s01/`: architecture test output, password-endpoint negative test results, security header scan (`curl -I` output), role matrix draft.
Matrix rows: NIST 3.1.1, 3.1.2, 3.1.5 (least privilege), 3.5.1–3.5.3 (identification and authentication via Entra), 3.13.15 (session authenticity), E8 MFA and restrict admin privileges (application part).

## Definition of done

Stories ticked; `make verify` green; module scaffolder used for Access; architecture tests active in CI; no passwords possible; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s01-kernel-and-access.md.
Check entry criteria (is S0 merged and make verify green?). Order the stories (I expect S1-1, S1-3, S1-2, S1-4, S1-5, S1-6, S1-7, S1-8, S1-9).
Start with S1-1: restate it, list the first three tests, write the first failing test, run it, then proceed with TDD.
```

## Retrospective

_(append notes here)_
