# Sprint 6 — Relationships, notes, tasks and timeline (22 Feb – 5 Mar 2027)

**Release**: R1 Core CRM. **Epics**: E7, E8.

## Goal

Organisations relate to each other, users record notes and tasks against any record, and every record has a timeline built from events.

## Demo

1. Create a partner relationship between two organisations; show it from both sides.
2. Add a note with a mention; the mentioned user receives an in-app notification stub.
3. Create a recurring task, complete it and see the next instance.
4. Open an organisation timeline: creation, notes, task events and link changes in order, even after replaying events twice.

## Entry criteria

- S5 merged; `make verify` green on `main`.
- Directory Contracts events available (S5-7).
- Allowed rich-text subset agreed (bold, italic, lists, links).

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S6-1 Organisation relationships

*As a user I record how organisations relate.*

Tests first:
- `Create_Relationship_IsDirectional_WithLabelPerSide`
- `Create_Duplicate_Rejected`
- `Relationship_Types_ComeFromConfigOptionList`
- `Delete_SoftDeletes_KeepsHistory`
- `Visibility_RelationshipHiddenIfEitherOrganisationHidden`

Implement: Relationship entity in Directory with type, direction and dates. Query endpoint returns both directions.

Done when: tested through API.

### S6-2 Notes

*As a user I write notes against any record.*

Tests first:
- `Note_StripsScriptAndUnsafeHtml` (corpus of XSS payloads)
- `Note_Mentions_ResolveUsersThroughAccessContracts`
- `Note_Edit_CreatesAuditEntryWithPreviousBody`
- `Note_OnInvisibleRecord_Rejected`
- `Note_Body_Limit_Enforced`

Implement: `Crm.Activities` (schema `activities`). Attachment target is (record type, record ID) validated through a registry of target types supplied by modules.

Done when: tests green; sanitiser library chosen and noted in an ADR.

### S6-3 Tasks

*As a user I manage my tasks and assign tasks to others.*

Tests first:
- `Task_StateMachine_AllowsOnlyDefinedTransitions`
- `Assign_ToOtherUser_RequiresPermission`
- `Overdue_IsComputedFromDueDateInUserTimeZone`
- `Recurrence_Weekly_CreatesNextOnCompletion`
- `Task_OnInvisibleRecord_Rejected`

Implement: Task entity with owner, assignee, due, status, priority, link to record, simple recurrence rules.

Done when: API and domain tests green.

### S6-4 Timeline projection

*As a user I see a unified timeline per record.*

Tests first:
- `Timeline_OrdersByOccurredAt_NotArrivalOrder`
- `Timeline_DuplicateEvent_AppearsOnce`
- `Timeline_Rebuild_FromEventLog_MatchesIncremental`
- `Timeline_RespectsVisibilityOfSourceRecord`
- `Timeline_MergedRecord_RedirectsToWinner`

Implement: Activities subscribes to Directory events and its own; writes `timeline_entries` with idempotent inbox. No cross-schema joins.

Done when: rebuild command available through `Crm.Migrator --rebuild-projection activities.timeline`.

### S6-5 Manual interaction logging

*As a user I log calls, meetings and emails I had.*

Tests first:
- `LogInteraction_MatchesParticipantsByEmail`
- `LogInteraction_UnknownParticipant_OffersCreatePerson`
- `Interaction_AppearsOnAllParticipantTimelines`

Implement: Interaction entity (type, date, participants, summary). Automated capture comes in S19.

Done when: timeline shows interactions for each participant.

### S6-6 Activity screens

*As a user I use notes, tasks and the timeline in the UI.*

Tests first:
- Component tests: notes panel with sanitised rendering, `My tasks` filters, timeline virtual list, record header summary
- Playwright: add note, create and complete task, see timeline update
- axe checks

Implement: `features/activities/` and shared timeline component embedded in Directory detail pages through a slot, not a module import.

Done when: Playwright journey green.

## Compliance evidence

XSS test corpus results; timeline replay test output. NIST 3.14.x input validation, 3.3.x audit.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s06/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s06-relationships-and-activities.md.
Check every entry criterion for sprint 6 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S6-1, S6-2, S6-3, S6-4, S6-5, S6-6.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
