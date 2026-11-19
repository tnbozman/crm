# Sprint 28 — AI safety and quality (27 Dec 2027 – 7 Jan 2028)

**Release**: R6 AI assistant and assurance. **Epics**: E45.

## Goal

Principles: the assistant runs in-process as the signed-in user and can do nothing the user cannot already do in the UI; tools are typed C# functions calling module Contracts (no free SQL, HTTP, shell or script execution); writes are proposals the user approves; handling-marked and out-of-scope data never reach the model; production uses Azure OpenAI in an Australian region unless ADR-008 is amended; prompts, tool calls and approvals are audited. Holiday sprint; the evaluation suite is the priority.

## Demo

1. Run the golden-question evaluation and show results over time.
2. Run the red-team suite: injection, argument fuzzing, escalation, exfiltration.
3. Trip the kill switch and per-user budget.
4. Show model and prompt version recorded in audit.

## Entry criteria

- S27 merged; `make verify` green on `main`.
- Live-model evaluation keys available in a non-production environment.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S28-1 Evaluation suite

*As the owner I know quality does not regress.*

Tests first:
- Golden set of questions with scripted fixtures run deterministically against the fake model
- `make ai-eval` runs optional live-model evaluation and stores results
- Threshold failures fail CI for the deterministic set

Implement: Evaluation harness and report.

Done when: results tracked.

### S28-2 Red-team tests

*As the security officer I probe the assistant.*

Tests first:
- Injection via notes, deal names, imported CSV text
- Tool-argument fuzzing
- Privilege escalation attempts (call tools of other roles)
- Exfiltration via links or webhook actions
- All blocked and logged

Implement: Test corpus in repo.

Done when: suite green.

### S28-3 Cost and rate controls

*As the operator I control spend.*

Tests first:
- `PerUserBudget_Enforced`
- `GlobalKillSwitch_DisablesAssistant`
- `ProviderOutage_Degrades_Gracefully`

Implement: Limits in configuration and admin toggle.

Done when: tested.

### S28-4 Change control for prompts and models

*As the owner I control what changes.*

Tests first:
- `SystemPrompt_AndSkills_Versioned`
- `ModelId_RecordedInAudit`
- `ChangeWithoutEvalRun_FailsRelease`

Implement: Version stamps and release check.

Done when: tested.

### S28-5 User guidance

*As a user I understand limits.*

Tests first:
- Help page lists what the assistant sees and can do
- Report-a-bad-answer action stores context minus sensitive data

Implement: UI help and feedback endpoint.

Done when: reviewed.

## Compliance evidence

Red-team results, evaluation history, change-control record. NIST 3.12.x, 3.14.x.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s28/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s28-ai-safety-and-quality.md.
Check every entry criterion for sprint 28 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S28-1, S28-2, S28-3, S28-4, S28-5.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
