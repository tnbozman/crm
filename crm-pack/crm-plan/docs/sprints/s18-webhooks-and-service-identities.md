# Sprint 18 — Webhooks and service identities (9 – 20 Aug 2027)

**Release**: R4 Automation and integration. **Epics**: E22.

## Goal

Other internal systems can receive signed events and call the CRM using Entra-issued identities, with no API keys.

## Demo

1. A webhook fires with a valid signature and a retry after a forced failure.
2. A request to a private-range address is blocked.
3. A service principal calls the API with an app role; a wrong audience or missing role is rejected.
4. Admin disables a service identity.

## Entry criteria

- S17 merged; `make verify` green on `main`.
- Consumers identified; Entra app registrations approach agreed.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S18-1 Outbound webhooks

*As an integrator I receive CRM events.*

Tests first:
- `Signature_VerifiesWithSharedSecret`
- `Replay_DetectedByTimestampAndNonce`
- `Retry_BackoffThenDeadLetter`
- `DestinationHost_NotInAllowList_Rejected`
- `PrivateRanges_AndMetadataEndpoints_Blocked` (SSRF)
- `Payload_ExcludesHandlingMarkedAndMaskedFields`

Implement: Webhook subscriptions in Automation; secrets in Key Vault references; fixed payload shapes per event.

Done when: tests green.

### S18-2 Service callers

*As an integrator I call the API as a service.*

Tests first:
- `ServiceToken_WrongAudience_401`
- `ServiceToken_MissingAppRole_403`
- `UserToken_OnServiceEndpoint_Rejected`
- `ServiceCaller_CannotExceedItsRole`
- `ExpiredToken_401`

Implement: JWT bearer scheme alongside the cookie scheme with separate policies.

Done when: tested with locally issued test tokens.

### S18-3 Service identity administration

*As an Admin I govern service identities.*

Tests first:
- `List_ShowsScopesAndLastUsed`
- `Disable_BlocksImmediately`
- `UnusedIdentity_RaisesAlertAfterThreshold`
- `OverPrivilegedIdentity_Flagged`

Implement: Registry in Access synced from configuration; alerts via logging.

Done when: tested.

### S18-4 Rate limits and quotas

*As the operator I protect the system.*

Tests first:
- `PerIdentityLimit_Returns429WithRetryAfter`
- `LimitsConfigurable`

Implement: Rate limiter partitions by identity.

Done when: tested.

### S18-5 API documentation

*As an integrator I read accurate docs.*

Tests first:
- OpenAPI snapshot test
- Docs page lists only endpoints the caller's role can use

Implement: Generated docs page behind authentication.

Done when: page reviewed.

## Compliance evidence

SSRF test results; service identity inventory. NIST 3.1.20, 3.5.x, 3.13.x.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s18/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s18-webhooks-and-service-identities.md.
Check every entry criterion for sprint 18 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S18-2, S18-3, S18-4, S18-1, S18-5.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
