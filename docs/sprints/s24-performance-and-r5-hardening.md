# Sprint 24 — Performance, resilience and R5 hardening (1 – 12 Nov 2027)

**Release**: R5 Reporting and forecasting. **Epics**: NFRs, E38, E40.

## Goal

The system meets volume and resilience targets and R5 is evidenced. Keep slack for carry-over from earlier sprints.

## Demo

1. Show load-test results at 100k records and 50k deals with concurrent users.
2. Pause the dispatcher, build a backlog, resume and show recovery.
3. Restore and rebuild projections.
4. Present the R5 evidence pack.

## Entry criteria

- S23 merged; `make verify` green on `main`.
- Carry-over list from S17–S23 reviewed and prioritised.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S24-1 Load test

*As the operator I know the limits.*

Tests first:
- Scripted scenario (k6 or NBomber) for the top ten journeys
- Thresholds committed; nightly job reports trends

Implement: Load scripts and a dedicated test environment run.

Done when: results in `docs/evidence/r5/`.

### S24-2 Query and index review

*As a developer I remove slow queries.*

Tests first:
- Query Store review recorded
- `Benchmarks` regressions fail the nightly job

Implement: Indexes via migrations with tests.

Done when: targets met.

### S24-3 Resilience drills

*As the operator I trust recovery.*

Tests first:
- `OutboxBacklog_Drains_AfterDispatcherRestart`
- `DispatcherFailover_SecondInstanceTakesOver`
- `Restore_ThenRebuildProjections_MatchesProduction`

Implement: Runbook updates.

Done when: drills recorded.

### S24-4 Carry-over

*As the team I finish what slipped.*

Tests first:
- Each carried story keeps its original test list; add none without discussion

Implement: Use the sprint plan to track items.

Done when: carry-over list empty or consciously deferred.

### S24-5 R5 compliance refresh

*As the security officer I refresh assurance.*

Tests first:
- Pen-test delta; vulnerability review; matrix updated

Implement: Evidence refresh.

Done when: evidence exported.

## Compliance evidence

Load-test output, drill reports. NIST 3.12.x, 3.14.x; E8 backups.

## Release gate

- [ ] Visibility-leakage tests green across dashboards, exports and scheduled reports
- [ ] Performance targets met
- [ ] Evidence exported to docs/evidence/r5/

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s24/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s24-performance-and-r5-hardening.md.
Check every entry criterion for sprint 24 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S24-4 first (carry-over), then S24-1, S24-2, S24-3, S24-5.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
