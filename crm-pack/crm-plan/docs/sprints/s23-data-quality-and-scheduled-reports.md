# Sprint 23 — Data quality and scheduled reports (18 – 29 Oct 2027)

**Release**: R5 Reporting and forecasting. **Epics**: E29, E30.

## Goal

Data-quality issues surface automatically, duplicates are worked through a queue, and scheduled summaries reach users with their own visibility only.

## Demo

1. Show the data-quality scorecard and fix lists.
2. Resolve a duplicate through the merge queue.
3. Receive a weekly pipeline summary email limited to your own visibility.
4. Unsubscribe from a report.

## Entry criteria

- S22 merged; `make verify` green on `main`.
- Report list and recipients agreed.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S23-1 Data quality rules

*As an Admin I see where data is weak.*

Tests first:
- One test per rule: missing owner, stale deal, duplicate candidate, orphaned relationship, invalid email domain
- `Scorecard_CountsMatchFixtures`
- `FixList_RespectsVisibility`

Implement: Rule set in Reporting using projections.

Done when: tests green.

### S23-2 Duplicate queue

*As a steward I resolve duplicates.*

Tests first:
- `Candidate_MergeAction_UsesDirectoryMerge`
- `Dismiss_RemembersDecision`
- `Queue_Paging_AndAssignment`

Implement: Queue UI over Directory Contracts.

Done when: tested.

### S23-3 Scheduled reports

*As a user I get regular summaries.*

Tests first:
- `Report_RenderedPerRecipient_WithTheirVisibility`
- `TwoRecipients_NoCrossLeakage`
- `Unsubscribe_StopsDelivery`
- `ScheduleRunsOnce_AcrossInstances`
- `Email_ContainsNoMaskedFields`

Implement: Definitions in Reporting; delivery via notification service.

Done when: tested.

### S23-4 Run history and failure alerts

*As an Admin I know when reports fail.*

Tests first:
- `FailedRun_RecordedAndAlerts`
- `Retry_Policy`

Implement: Run history screen.

Done when: tested.

## Compliance evidence

Per-recipient leakage tests. NIST 3.1.3, 3.14.x.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s23/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s23-data-quality-and-scheduled-reports.md.
Check every entry criterion for sprint 23 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S23-1, S23-2, S23-3, S23-4.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
