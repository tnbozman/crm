# Sprint 16 — Probity gate, business-unit separation and R3 hardening (12 – 23 Jul 2027)

**Release**: R3 Products, research and probity. **Epics**: E19, E20.

## Goal

Probity verdicts gate deals and quotes, business units are separated end to end, and R3 is hardened and evidenced.

## Demo

1. A blocking probity result stops a stage move; an authorised override records reason and approver.
2. Two business units cannot see each other's records anywhere, including search and exports.
3. Show the isolation test matrix run.
4. Show R3 evidence pack.

## Entry criteria

- S15 merged; `make verify` green on `main`.
- Business-unit list and cross-unit sharing rules agreed with the business owner.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S16-1 Probity gate

*As a manager I cannot proceed past a probity block without an override.*

Tests first:
- `StageMove_BlockedByProbity_Returns409WithReasonCodes`
- `Override_RequiresApproverAndReason`
- `Override_ApproverCannotBeRequester`
- `QuoteSend_BlockedByProbity`
- `OverrideAndBlock_BothAudited`

Implement: Deals and Commerce call `IProbityChecks` at transition points.

Done when: tested.

### S16-2 Business-unit separation

*As the security officer I prove isolation.*

Tests first:
- Generated matrix: two units × every list, detail, search, export, timeline and notification endpoint
- `CrossUnitShare_RequiresApproval_AndExpires`
- `UserInTwoUnits_SeesBoth_NoMore`
- `ReportingProjections_CarryUnitScope`
- `AiTools_RespectUnitScope` (fake agent run; real tools arrive in S26)

Implement: Unit scope column and filter in all modules; central policy class.

Done when: matrix green.

### S16-3 Projection and export leakage tests

*As the security officer I check indirect paths.*

Tests first:
- `SearchSnippet_DoesNotRevealHiddenRecordNames`
- `ExportCounts_DoNotRevealHiddenRows`
- `NotificationText_DoesNotRevealHiddenRecord`
- `Error_Messages_DoNotConfirmExistenceOfHiddenRecords`

Implement: Fix findings found by tests.

Done when: no leakage paths.

### S16-4 R3 hardening

*As the operator I confirm performance and security.*

Tests first:
- Benchmark thresholds extended for quotes and programmes
- Pen-test delta scheduled; findings test-first fixed
- Data reconciliation of imported programmes

Implement: Indexes, evidence refresh, compliance matrix update.

Done when: evidence in `docs/evidence/r3/`.

## Compliance evidence

Isolation matrix output; override audit samples. NIST 3.1.1, 3.1.3, 3.1.5, 3.3.x.

## Release gate

- [ ] Business-unit isolation tests green across all modules
- [ ] Probity overrides audited
- [ ] Pen-test delta clean of highs
- [ ] Evidence exported to docs/evidence/r3/

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s16/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s16-probity-gate-and-business-units.md.
Check every entry criterion for sprint 16 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S16-1, S16-2, S16-3, S16-4.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
