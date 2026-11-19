# Sprint 22 — Forecasting and targets (4 – 15 Oct 2027)

**Release**: R5 Reporting and forecasting. **Epics**: E27.

## Goal

Teams set targets, forecast with weighted pipeline and overrides, and compare forecast to actual.

## Demo

1. Set quarterly targets per team.
2. Show weighted pipeline and a manual override with reason.
3. Show weekly snapshots and slippage.
4. Drill down limited by visibility.

## Entry criteria

- S21 merged; `make verify` green on `main`.
- Forecast categories and period definitions agreed (commit, best case, pipeline).

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S22-1 Targets

*As a manager I set targets.*

Tests first:
- `Target_PerTeamPersonPeriodPipeline`
- `OverlappingPeriods_Rejected`
- `OnlyManagersCanSet`
- `Changes_Audited`

Implement: Targets in Reporting.

Done when: tested.

### S22-2 Weighted pipeline and overrides

*As a manager I forecast.*

Tests first:
- `Weighted_UsesStageProbability`
- `Override_RequiresReason_AndIsAudited`
- `Currency_ConversionRuleApplied`
- `ClosedDateShift_MovesBetweenPeriods`

Implement: Forecast calculation service with deterministic rounding.

Done when: table-driven tests.

### S22-3 Snapshots

*As a manager I see how the forecast changed.*

Tests first:
- `WeeklySnapshot_CapturedOnce`
- `Snapshot_Immutable`
- `ForecastVsActual_UsesSnapshotAtPeriodStart`
- `Scheduler_OnlyOneInstanceRuns`

Implement: Scheduled snapshot job.

Done when: tested.

### S22-4 Forecast UI

*As a manager I use the forecast screens.*

Tests first:
- Component tests for roll-up table and drill-down
- Playwright: override a deal's category and see roll-up change

Implement: `features/reporting/forecast`.

Done when: journey green.

## Compliance evidence

Override audit samples; roll-up arithmetic test output. NIST 3.3.x.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s22/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s22-forecasting-and-targets.md.
Check every entry criterion for sprint 22 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S22-1, S22-2, S22-3, S22-4.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
