# Sprint 27 — Manage skills with approval (13 – 24 Dec 2027)

**Release**: R6 AI assistant and assurance. **Epics**: E44.

## Goal

Principles: the assistant runs in-process as the signed-in user and can do nothing the user cannot already do in the UI; tools are typed C# functions calling module Contracts (no free SQL, HTTP, shell or script execution); writes are proposals the user approves; handling-marked and out-of-scope data never reach the model; production uses Azure OpenAI in an Australian region unless ADR-008 is amended; prompts, tool calls and approvals are audited. This sprint adds write capabilities through explicit approval. Holiday sprint: keep the write tool list short.

## Demo

1. Ask the assistant to log a note and create a task; it returns proposals only.
2. Approve one and reject another.
3. Show a replay and a tampered proposal rejected.
4. Show the linked audit trail.

## Entry criteria

- S26 merged; `make verify` green on `main`.
- List of permitted write tools agreed (start narrow).

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S27-1 Proposal model

*As the system I describe changes without making them.*

Tests first:
- `WriteTool_ReturnsProposal_NoSideEffects`
- `Proposal_HasBeforeAfterPreview`
- `Proposal_Hash_CoversTargetAndArguments`

Implement: Structured proposal contract.

Done when: tested.

### S27-2 Approval API and UI

*As a user I approve or reject.*

Tests first:
- `Approve_ExecutesAsUser_ThroughContracts`
- `Approve_ByOtherUser_Rejected`
- `Proposal_Expires`
- `Replay_Rejected`
- `TamperedProposal_Rejected`
- `Edit_BeforeApproval_ChangesHash`

Implement: Approvals table, endpoints and dialog.

Done when: tested.

### S27-3 Initial write tools

*As a user I do simple work by chat.*

Tests first:
- Create note, create task, update deal stage, add participant, log interaction: each with permission, validation, probity gate and audit tests
- `StageMove_BlockedByProbity_ReturnedAsProposalError`

Implement: Tools call existing commands through Contracts so all rules apply.

Done when: tested.

### S27-4 Exclusions

*As the security officer I keep dangerous actions out.*

Tests first:
- `NoDeleteMergeRoleChangeOrExportTools_Registered` (architecture test over tool registry)

Implement: Registry allow-list.

Done when: tested.

### S27-5 Audit linkage

*As the security officer I trace assistant actions.*

Tests first:
- `Proposal_Approval_Execution_AreLinkedByCorrelationId`
- `AuditEntries_MarkedAssistantOriginated`

Implement: Audit enrichment.

Done when: tested.

## Compliance evidence

Approval audit chain samples. NIST 3.1.x, 3.3.x.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s27/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s27-manage-skills-with-approval.md.
Check every entry criterion for sprint 27 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S27-1, S27-2, S27-4, S27-3, S27-5.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
