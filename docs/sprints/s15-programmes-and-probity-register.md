# Sprint 15 — Programmes, grants and the probity register (28 Jun – 9 Jul 2027)

**Release**: R3 Products, research and probity. **Epics**: E18, E19.

## Goal

Grants and research programmes are tracked with milestones and partners, and staff can declare interests that reviewers assess.

## Demo

1. Create a grant programme with milestones, budget and a university partner.
2. Milestone dates drive reminders.
3. A user declares an interest in an organisation; a reviewer approves it.
4. Show that other users cannot see the declaration.

## Entry criteria

- S14 merged; `make verify` green on `main`.
- Probity policy owner nominated; declaration form content agreed.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S15-1 Programme and grant tracking

*As a programme manager I track a grant.*

Tests first:
- `Milestones_MustFallWithinProgrammeDates`
- `BudgetTotals_EqualSumOfLines`
- `MatchedFunding_Calculated`
- `PartnerRole_UniquePerProgramme`
- `LinkedDeals_ByIdOnly`

Implement: `Crm.Programmes` (schema `programmes`).

Done when: API tested.

### S15-2 University partners

*As a programme manager I keep partner agreement information.*

Tests first:
- `PartnerAgreement_StoresTermsSummaryAndDocumentLinks`
- `IpTermsFields_MaskedForNonProgrammeRoles`

Implement: Partner agreement summary fields and links (no document content).

Done when: tested.

### S15-3 Declarations

*As a staff member I declare interests.*

Tests first:
- `Declaration_VisibleOnlyToDeclarerReviewerAndAuditor`
- `Declaration_Approve_ByReviewerNotSelf`
- `Declaration_ExpiresAndPromptsRenewal`
- `Declaration_ChangeAudited`

Implement: `Crm.Probity` (schema `probity`) with declaration entity and workflow.

Done when: tested.

### S15-4 Probity checks API

*As other modules I ask Probity for a verdict.*

Tests first:
- `Evaluate_NoDeclarations_Clear`
- `Evaluate_DeclaredInterestInParticipant_ReturnsBlockingWithReasonCodes`
- `Evaluate_DoesNotLeakDeclarationDetailsToCaller`
- `Evaluate_IsAudited`

Implement: `IProbityChecks.Evaluate(subject, context)` with clear/advisory/blocking and reason codes; replaces the S10 stub through DI.

Done when: S10 conflict tests still green.

### S15-5 Programme and declaration UI

*As a user I manage programmes and declarations.*

Tests first:
- Component tests for programme dashboard, milestone calendar and declaration form
- Playwright journeys for both

Implement: `features/programmes/`, `features/probity/`.

Done when: journeys green.

## Compliance evidence

Probity access-restriction tests and audit samples; policy mapping. Supports conflict-of-interest governance.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s15/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s15-programmes-and-probity-register.md.
Check every entry criterion for sprint 15 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S15-1, S15-2, S15-3, S15-4, S15-5.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
