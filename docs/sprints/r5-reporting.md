# R5 — Reporting and forecasting (S21–S24, 20 Sep – 12 Nov 2027)

**Outcome**: leadership gets reliable dashboards, forecasts against targets, data-quality reports and scheduled summaries, all respecting visibility rules, at 100k+ records.

Use the **refine prompt** before each sprint. Common rules from `CLAUDE.md` apply.

## S21 — Dashboards (E26), 20 Sep – 1 Oct

- **S21-1 Reporting projections**: pipeline by stage, win rate, cycle time, activity volume, programme status; built from events, rebuildable from Contracts queries. Tests first: projection correctness against fixtures, idempotent replay, visibility scope on every row.
- **S21-2 Dashboard definitions**: a fixed library of widgets (no free-form query builder), user-configurable layout, filters.
- **S21-3 Charts** in Vue with accessible tables behind each chart; export to CSV with the same permission and audit rules.
- **S21-4 Performance**: dashboards load under target at 100k deals/records; caching with invalidation by event.

## S22 — Forecasting and targets (E27), 4 – 15 Oct

- **S22-1 Targets** per team, person, period, pipeline.
- **S22-2 Forecast categories and weighted pipeline**: probability from stage with manual override and reason; snapshots at period start and weekly.
- **S22-3 Forecast vs actual and slippage reports**; commit/best-case/pipeline roll-ups. Tests first: roll-up arithmetic, currency rules, closed-date shifts.
- **S22-4 Forecast UI** with drill-down limited by visibility; override audit.

## S23 — Data quality and scheduled reporting (E29, E30), 18 – 29 Oct

- **S23-1 Data quality rules**: missing owner, stale deals, duplicate candidates, orphaned relationships, invalid email domains; scorecard and fix lists. Tests first for each rule.
- **S23-2 Duplicate management queue** using Directory merge (S5-6).
- **S23-3 Scheduled report definitions** (weekly pipeline summary, overdue tasks, bid calendar) delivered by email to the recipient only with their own visibility applied. Tests first: reports rendered per recipient; no cross-user leakage; unsubscribe.
- **S23-4 Report run history and failure alerts.**

## S24 — Performance and hardening (1 – 12 Nov), buffer sprint

- **S24-1 Load test** at 100k organisations/people and 50k deals with concurrent users; record results in `docs/evidence/r5/`.
- **S24-2 Index and query review**, SQL Query Store analysis, projection rebuild time targets.
- **S24-3 Resilience**: outbox backlog drills, dispatcher failover, restore with projections rebuilt.
- **S24-4 Carry-over buffer** for slipped stories from R4/R5.
- **S24-5 R5 compliance refresh**: matrix, pen-test delta, vulnerability review.

## R5 release gate

Visibility leakage tests across dashboards, exports and scheduled reports green; performance targets met; evidence exported.
