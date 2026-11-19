# Sprint 9 — Deals and pipelines (5 – 16 Apr 2027)

**Release**: R2 Deals and bids. **Epics**: E10.

## Goal

Deals move through configurable pipelines and stages, with history, visibility rules and a board and list UI.

## Demo

1. Admin defines a pipeline with stages, probabilities and required fields.
2. Create a deal and move it across stages; a required field blocks a move.
3. Mark a deal won and lost with reasons.
4. Show the Kanban board and its keyboard-accessible alternative.

## Entry criteria

- S8 merged; `make verify` green on `main`.
- R1 live and stable; hypercare issues triaged.
- Business owner has agreed pipelines (professional services, product, grants) and stage names.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S9-1 Pipeline and stage definitions

*As an Admin I configure pipelines.*

Tests first:
- `CreatePipeline_WithOrderedStages_Persists`
- `DeleteStage_WithDeals_Rejected`
- `ReorderStages_KeepsHistoryIntact`
- `StageProbability_MustBeBetween0And100`
- `RequiredFieldsPerStage_AreValidatedAgainstConfigProperties`

Implement: Stage and pipeline definitions in `Crm.Config` with Contracts `IPipelines`.

Done when: admin API and tests green.

### S9-2 Deal aggregate

*As a user I create and edit deals.*

Tests first:
- `Create_Deal_RequiresOwnerPipelineAndStage`
- `MoveStage_MissingRequiredField_ReturnsFieldErrors`
- `MarkWon_SetsClosedDate_AndRejectsFurtherStageMoves`
- `MarkLost_RequiresReason`
- `Reopen_AllowedForAdminOnly`
- `ConcurrentEdit_StaleETag_Conflicts`
- Organisation and person references validated through `IDirectoryQueries`, stored as IDs only

Implement: `Crm.Deals` (schema `deals`): name, owner, pipeline, stage, money value and currency, expected close, source, status, loss reason, JSON values.

Done when: API CRUD and transitions tested.

### S9-3 Stage history

*As a manager I see how long deals spend in each stage.*

Tests first:
- `StageChange_AppendsHistoryEntry_WithDuration`
- `History_SurvivesStageDefinitionRename`
- `Velocity_Projection_MatchesHistory`

Implement: History table in `deals` plus `DealStageChanged` events.

Done when: history API tested.

### S9-4 Visibility

*As a user I only see deals I am allowed to see.*

Tests first:
- Two business units × list, detail, search, board → leakage tests
- `OwnerTeam_CanEdit_OthersRead_PerPolicy`
- `ValueField_HiddenForRolesWithoutFinanceView`

Implement: Global filters and field-level masking in Deals.

Done when: negative tests green.

### S9-5 Deal board and list UI

*As a user I manage deals visually.*

Tests first:
- Vitest: board columns from pipeline definitions, drag emits stage change, error toast on blocked move
- Keyboard alternative: move via menu; axe check
- Playwright: create deal, drag to next stage, see history

Implement: `features/deals/` with OpenVue components; composition with Directory names via Contracts-backed API fields.

Done when: Playwright journey green.

### S9-6 Deals Contracts and timeline events

*As another module I react to deal changes.*

Tests first:
- `DealCreated/StageChanged/Won/Lost` events published through the outbox
- Activities timeline subscriber test shows entries

Implement: `IDealQueries` (by IDs, by organisation role later) and events.

Done when: consumer tests green.

## Compliance evidence

Visibility matrix output; stage-history audit sample. NIST 3.1.1, 3.1.5, 3.3.1.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s09/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s09-deals-and-pipelines.md.
Check every entry criterion for sprint 9 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S9-1, S9-2, S9-3, S9-4, S9-6, S9-5.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
