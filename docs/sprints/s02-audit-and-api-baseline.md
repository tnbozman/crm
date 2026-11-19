# Sprint 2 — Audit, API baseline and AI spike (28 Dec 2026 – 8 Jan 2027)

**Release**: R0. **Epics**: E3 (audit and logging), E37 (monitoring, detection), E46 (API and AI foundations). Holiday sprint: keep scope to the must-haves; the AI spike and HubSpot spike are the first to drop.

## Goal

Every data change in any module is recorded in a tamper-resistant audit log written by a separate database identity. APIs follow one convention (versioning, problem details, validation, concurrency). Structured logs are ready for central collection. The AI provider seam works against a fake and, with keys, against Claude and OpenAI.

## Demo

1. Change a user's status; see the audit entry (who, what, before/after, correlation ID) in the Audit API as an Auditor.
2. Show that the runtime SQL identity cannot update or delete from `audit` tables (test output).
3. Show a request's logs sharing a correlation ID across API, outbox handler and audit.
4. `make ai-smoke` returns a one-line reply from each configured provider (skipped without keys).

## Entry criteria

- S1 merged. Module scaffolder and outbox working.

## Stories

### S2-1 Audit module (E3)

Tests first (persistence, real SQL):
- `Audit_Append_StoresEntryWithActorTimeCorrelationAndModule`.
- `AuditTable_RejectsUpdateAndDelete_ForRuntimeLogin` (create a restricted SQL login in the fixture mirroring production permissions).
- `AuditEntries_CannotBeReadByNonAuditorRole` (API).
- `Audit_IsIdempotent_ForDuplicateEvents` (inbox).
- Migrations tests as standard.

Implement: `Crm.Audit` with `audit.audit_entries` (ID, occurred-at, actor ID, actor name snapshot, module, entity type, entity ID, action, changes JSON, correlation ID, source IP, result), insert-only grants documented in `infra/sql/permissions.sql`, hash chaining column (`prev_hash`, `hash`) so tampering is detectable, `IAuditWriter` in Contracts, query API for Auditor role with paging and filters.

### S2-2 Automatic change auditing for modules (E3)

Tests first:
- `SaveChanges_InAnyModuleContext_EmitsAuditEvent_WithBeforeAndAfter` (use a test entity in the sample module).
- `SensitiveFields_AreMaskedInAuditPayload` (attribute `[AuditMask]`).
- `ReadOfProtectedRecord_EmitsAccessAudit` (opt-in attribute for later probity and finance records).
- `AuditFailure_DoesNotFailTheBusinessTransaction_ButRaisesAlert` or the opposite; choose, write the ADR, test the choice (default: audit via outbox in the same transaction so both succeed or both fail).

Implement: `AuditInterceptor` in the platform kernel writing an audit integration event to the module outbox; Audit module consumes it.

### S2-3 Authentication and administrative event audit (E3, E36)

Tests first: sign-in success, sign-in failure, sign-out, user disabled, role change each produce an audit entry with the correct action name.

### S2-4 Structured logging and correlation (E3, E37)

Tests first:
- `EveryRequest_HasCorrelationId_InResponseHeaderAndLogScope`.
- `LogOutput_DoesNotContain_TokensCookiesOrEmailBodies` (scrubber test with seeded secrets).
- `OutboxHandler_Logs_WithOriginatingCorrelationId`.

Implement: Serilog or built-in logging with OpenTelemetry; sinks configurable; Application Insights / Azure Monitor exporter behind config; `X-Correlation-Id` header; log retention settings documented for S3.

### S2-5 API conventions (E46)

Tests first:
- `Validation_Failure_ReturnsProblemDetails400_WithFieldErrors`.
- `ConcurrentUpdate_WithStaleETag_Returns412or409`.
- `List_Endpoint_PagesWithCursor_AndCapsPageSize`.
- `Unhandled_Exception_ReturnsProblemDetails500_WithoutStackTrace`.
- `Rate_Limit_Returns429` (per-user limiter).
- OpenAPI snapshot test: contract changes show up in review.

Implement: exception filter mapping typed exceptions (`NotFoundException`, `ConflictException`, `ValidationException`, `ForbiddenException`) to problem details, validation filter, paging contract (`PagedResult<T>` DTO), ETag handling, rate limiter, OpenAPI polish, regenerate TS client. Add a base `CrmControllerBase` with route and policy conventions, and an architecture test that all controllers derive from it.

### S2-6 Health, readiness and alerts hook (E37)

Tests first: `/api/v1/health` anonymous returns minimal status; `/api/v1/health/ready` requires an authenticated admin or a platform probe identity and reports database and outbox lag; outbox lag above threshold turns readiness degraded.

### S2-7 AI provider seam spike (E46, ADR-008)

Tests first:
- `AiOptions_Provider_IsValidatedAtStartup` (unknown provider, missing endpoint, external provider in Production without approval flag → startup fails).
- `ChatClientFactory_ReturnsAzureOpenAi_ByDefaultInProduction`.
- `FakeChatClient_ReturnsScriptedReplies_ForAgentTests` (used by all later AI tests).
- `AiSmoke_Claude_ReturnsText` and `AiSmoke_OpenAi_ReturnsText` (category `AiSmoke`; skipped when the API key env var is absent).
- `NoApiKeys_InRepoOrConfigFiles` (scan).

Implement: `Crm.Platform.Ai` with `IChatClient` factory for Azure OpenAI, OpenAI, Anthropic; one spike endpoint available in Development only; `make ai-smoke`; document the packages actually installed and their versions in ADR-008.

### S2-8 HubSpot API reference spike (E46) — optional

Time-box one day. Produce `docs/hubspot-mapping.md`: the fields and association model you want to be import-compatible with (objects, standard properties, association labels, pipeline and stage concepts), based on the current API reference. No code.

## Compliance evidence

`docs/evidence/s02/`: audit immutability test output, log sample showing correlation, the permissions script, ADR on audit failure behaviour.
Matrix rows: NIST 3.3.1–3.3.9 (audit and accountability), 3.1.7, E8 restrict admin privileges (separation of audit writer), centralised logging prerequisites.

## Definition of done

Stories ticked (S2-8 optional); `make verify` green; audit trail visible in the demo; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s02-audit-and-api-baseline.md.
Verify S1 is complete. Plan the order (S2-1, S2-2, S2-3, S2-4, S2-5, S2-6, S2-7, S2-8).
For S2-1, write the SQL-permission test first: it must fail because the restricted login does not exist yet.
```

## Retrospective

_(append notes here)_
