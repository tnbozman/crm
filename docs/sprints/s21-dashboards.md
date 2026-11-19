# Sprint 21 — Dashboards (20 Sep – 1 Oct 2027)

**Release**: R5 Reporting and forecasting. **Epics**: E26.

## Goal

Leadership and teams see accurate dashboards built from projections, respecting visibility rules.

## Demo

1. Open the pipeline dashboard with filters by team and period.
2. Drill from a chart into the list behind it, limited by visibility.
3. Export the table behind a chart.
4. Show dashboard load time at volume.

## Entry criteria

- S20 merged; `make verify` green on `main`.
- Dashboard widget list agreed (pipeline by stage, win rate, cycle time, activity volume, programme status).

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S21-1 Reporting projections

*As the system I keep read models current.*

Tests first:
- `PipelineByStage_MatchesFixtureExpectation`
- `WinRate_ComputedPerPeriod`
- `CycleTime_FromStageHistory`
- `Projection_ReplayIdempotent`
- `Projection_Rebuild_MatchesIncremental`
- `EveryRow_CarriesVisibilityScope`

Implement: Projection tables in Reporting from Deals, Activities and Programmes events.

Done when: fixture-based tests.

### S21-2 Dashboard definitions and widgets

*As a user I choose and arrange widgets.*

Tests first:
- `Layout_SavedPerUser`
- `Filters_AppliedToAllWidgets`
- `UnknownWidget_Rejected`

Implement: Fixed widget library, no free-form query builder.

Done when: tested.

### S21-3 Charts and accessible tables

*As a user I see charts with accessible equivalents.*

Tests first:
- Component tests: each chart renders from data and has a table alternative
- axe checks
- `Export_CsvOfWidget_RespectsVisibilityAndMasks`

Implement: Chart components in Vue; shared export path from S7.

Done when: tested.

### S21-4 Volume performance and caching

*As a user I get fast dashboards.*

Tests first:
- Benchmark on 50k deals under threshold
- `Cache_InvalidatedByRelevantEvent`
- `Cache_PartitionedByVisibilityScope`

Implement: Caching by scope and event invalidation.

Done when: numbers recorded.

## Compliance evidence

Visibility tests for every widget; performance numbers. NIST 3.1.1, 3.1.3.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s21/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s21-dashboards.md.
Check every entry criterion for sprint 21 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S21-1, S21-2, S21-3, S21-4.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
