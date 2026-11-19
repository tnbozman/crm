# Sprint 26 — Query skills (29 Nov – 10 Dec 2027)

**Release**: R6 AI assistant and assurance. **Epics**: E43.

## Goal

Principles: the assistant runs in-process as the signed-in user and can do nothing the user cannot already do in the UI; tools are typed C# functions calling module Contracts (no free SQL, HTTP, shell or script execution); writes are proposals the user approves; handling-marked and out-of-scope data never reach the model; production uses Azure OpenAI in an Australian region unless ADR-008 is amended; prompts, tool calls and approvals are audited. This sprint adds read-only skills across modules.

## Demo

1. Ask for my open deals closing this quarter; the answer cites records.
2. Ask who the competitors are on a deal.
3. Ask for overdue tasks and last notes on an organisation.
4. A user in another business unit gets no hidden data from any question.

## Entry criteria

- S25 merged; `make verify` green on `main`.
- Skill content reviewed by the module owners.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S26-1 Directory skill

*As a user I find organisations and people by chat.*

Tests first:
- `FindOrganisation_ByNameOrAlias`
- `GetRelationships`
- `HiddenRecord_NotReturned`

Implement: `SKILL.md` and tools in Directory.

Done when: tests with scripted model.

### S26-2 Deals skill

*As a user I ask about my pipeline.*

Tests first:
- `MyOpenDeals_FilteredByCloseDate`
- `ParticipantsByRole`
- `UpcomingBidMilestones`
- `MaskedValue_NotReturnedWithoutRole`

Implement: Skill and tools in Deals.

Done when: tested.

### S26-3 Activities skill

*As a user I query tasks and notes.*

Tests first:
- `MyOverdueTasks`
- `RecentNotes_OnRecord`
- `TimelineSummary_UsesTimelineOnly`

Implement: Skill and tools in Activities.

Done when: tested.

### S26-4 Commerce and Programmes skills

*As a user I ask about quotes and grants.*

Tests first:
- `QuoteStatus`
- `ProgrammeMilestones`
- `CostFields_NeverReturned`
- `ProbityDetails_NeverReturned`

Implement: Read-only tools.

Done when: tested.

### S26-5 Reporting skill

*As a manager I ask for forecast and dashboard figures.*

Tests first:
- `ForecastForTeam_ReturnsProjectionNumbers`
- `Figures_MatchDashboardWidgets`

Implement: Tools call Reporting Contracts.

Done when: tested.

### S26-6 Answer grounding

*As a user I can check where answers came from.*

Tests first:
- `UI_ShowsToolResultsAlongsideAnswer`
- `NumbersInAnswer_ComeFromToolResults` (scripted fixtures)
- `Citations_LinkToRecords`

Implement: Response format with tool-result panel.

Done when: tested.

### S26-7 Visibility matrix for tools

*As the security officer I prove no leakage via AI.*

Tests first:
- Generated matrix: every tool × role × business unit × handling-marked data
- `PromptAndResult_Audited`

Implement: Matrix test generator reuses the S12 pattern.

Done when: matrix green.

## Compliance evidence

AI visibility matrix output and audit samples. NIST 3.1.x, 3.3.x.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s26/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s26-query-skills.md.
Check every entry criterion for sprint 26 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S26-1, S26-2, S26-3, S26-4, S26-5, S26-6, S26-7.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
