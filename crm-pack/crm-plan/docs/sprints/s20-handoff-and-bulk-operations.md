# Sprint 20 — Finance and delivery hand-off, bulk operations (6 – 17 Sep 2027)

**Release**: R4 Automation and integration. **Epics**: E24, E25.

## Goal

Won deals hand off cleanly to finance and delivery, and users can perform safe bulk changes.

## Demo

1. Mark a deal won: a hand-off record and checklist appear and the adapter exports it.
2. A failed export retries visibly.
3. Bulk reassign owners with a preview and per-record results.
4. Show R4 evidence.

## Entry criteria

- S19 merged; `make verify` green on `main`.
- Finance system interface decided (Sage Intacct API or CSV drop); delivery hand-off fields agreed.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S20-1 Won-deal hand-off

*As Finance I get what I need when a deal is won.*

Tests first:
- `DealWon_CreatesHandoffRecord_WithRequiredFields`
- `MissingBillingContact_BlocksCompletion`
- `Export_IsIdempotent_ByHandoffId`
- `Export_Failure_RetriesAndIsVisible`
- `Export_Payload_ExcludesMaskedFields`

Implement: Hand-off aggregate in Automation; `IFinanceAdapter` interface with fake and CSV implementations.

Done when: tested with fake adapter.

### S20-2 Delivery reference

*As a delivery lead I find the project from the deal.*

Tests first:
- `DeliveryReference_StoresExternalIdAndUrl`
- `Url_AllowListed`

Implement: Link to Jira or project tool by URL and ID only.

Done when: tested.

### S20-3 Bulk operations

*As an Admin I make safe bulk changes.*

Tests first:
- `Preview_ShowsAffectedRecordsAndConflicts`
- `PermissionChecked_PerRecord`
- `FailedBatch_RollsBack_AndReportsRows`
- `LargeBatch_RunsInBackground_WithProgress`
- `Bulk_AuditedPerRecordWithBatchId`
- `Limits_Enforced`

Implement: Bulk command framework used by Directory and Deals first.

Done when: tested.

### S20-4 R4 hardening

*As the security officer I verify R4.*

Tests first:
- Pen-test delta on webhooks and service identities
- Review of every service identity's scope
- Restore drill with automation data

Implement: Evidence refresh.

Done when: evidence in `docs/evidence/r4/`.

## Compliance evidence

Hand-off audit samples; service identity review. NIST 3.1.5, 3.3.x, 3.12.x.

## Release gate

- [ ] No standing broad-permission service identity
- [ ] Webhook SSRF tests green
- [ ] Automation actions audited with rule IDs
- [ ] Evidence exported to docs/evidence/r4/

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s20/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s20-handoff-and-bulk-operations.md.
Check every entry criterion for sprint 20 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S20-1, S20-2, S20-3, S20-4.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
