# Sprint 17 — Rules engine (26 Jul – 6 Aug 2027)

**Release**: R4 Automation and integration. **Epics**: E21.

## Goal

Admins configure rules that react to events and perform a fixed set of safe actions, with simulation and full audit.

## Demo

1. Create a rule: when a deal moves to Proposal, create a task for the bid manager.
2. Simulate the rule against a historical event with no side effects.
3. Trigger a loop and show it stopped by protection.
4. Show the run history and the audit of automation actions.

## Entry criteria

- S16 merged; `make verify` green on `main`.
- Three to five real rules agreed with the business to drive acceptance tests.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S17-1 Rule model

*As an Admin I define rules without code.*

Tests first:
- `Rule_Json_ValidatedAgainstSchema`
- `UnknownActionOrTrigger_Rejected`
- `RuleVersion_IncrementsOnEdit_OldVersionsKept`
- `NonAdmin_CannotEditRules`

Implement: `Crm.Automation` (schema `automation`): trigger (event type + filter), conditions (field compare, role, value threshold), actions (create task, notify, set field, request approval, call webhook). No scripting.

Done when: CRUD API tested.

### S17-2 Evaluation engine

*As the system I evaluate rules reliably.*

Tests first:
- `Event_MatchingRule_ExecutesActionOnce`
- `DuplicateEvent_DoesNotExecuteTwice`
- `FailingAction_RetriedThenDeadLettered`
- `ChainDepthExceeded_StopsAndAlerts`
- `ActionCauseTracking_PreventsSelfTrigger`
- `RuleDisabled_Ignored`

Implement: Inbox-based consumer, deterministic evaluator, action executors that call module Contracts only.

Done when: tests green.

### S17-3 Simulation

*As an Admin I test a rule safely.*

Tests first:
- `Simulate_ReturnsPlannedActions_WithNoSideEffects`
- `Simulate_UsesHistoricalEventPayload`
- `Simulate_RespectsAdminVisibility`

Implement: Dry-run mode in the executor pipeline.

Done when: tested.

### S17-4 Automation identity and audit

*As the security officer I see what automation did.*

Tests first:
- `ActionsRunAs_AutomationPrincipal_WithLimitedRights`
- `AuditEntry_IncludesRuleIdVersionAndTriggeringEvent`
- `AutomationPrincipal_CannotDeleteOrChangeRoles`

Implement: Automation principal registered in Access; limits enforced by policy.

Done when: tested.

### S17-5 Admin UI

*As an Admin I manage rules.*

Tests first:
- Component tests for editor validation and run history
- Playwright: create, simulate, enable, view run

Implement: `features/automation/`.

Done when: journey green.

## Compliance evidence

Automation audit samples; loop-protection test. NIST 3.1.5, 3.3.x, 3.4.x.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s17/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s17-rules-engine.md.
Check every entry criterion for sprint 17 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S17-1, S17-2, S17-4, S17-3, S17-5.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
