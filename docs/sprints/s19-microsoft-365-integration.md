# Sprint 19 — Microsoft 365 integration (23 Aug – 3 Sep 2027)

**Release**: R4 Automation and integration. **Epics**: E23.

## Goal

Meetings and emails enrich the timeline with metadata only, using the least permissions the tenant allows.

## Demo

1. A fake Graph server provides meetings; matching participants get timeline entries.
2. A user opts out and nothing more is captured.
3. A throttling response is handled with backoff.
4. Admin sees a consent error surfaced clearly.

## Entry criteria

- S18 merged; `make verify` green on `main`.
- ADR on Graph permissions model approved by the security officer and the Entra admin.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S19-1 Permissions model decision

*As the security officer I approve the access model.*

Tests first:
- Documented comparison of delegated versus application permissions and application access policies
- Test that requested scopes in configuration equal the approved list

Implement: ADR and config validation at startup.

Done when: ADR signed off.

### S19-2 Activity capture

*As a user I see my meetings and emails on timelines.*

Tests first:
- `Meeting_MatchesParticipantsByEmail`
- `Capture_StoresSubjectTimeAndParticipantsOnly`
- `Duplicate_NotCreatedOnResync`
- `DeletedItem_RemovesOrMarksEntry`
- `UserOptOut_StopsCapture_AndKeepsExisting`
- `UnknownParticipant_Ignored`

Implement: Connector in Automation or Activities using a Graph client behind an interface; delta queries; fake Graph server for tests.

Done when: tests green.

### S19-3 Failure handling

*As the operator I see integration health.*

Tests first:
- `Throttle429_BacksOffWithRetryAfter`
- `ConsentMissing_RaisesAdminAlert`
- `Connector_Metrics_Emitted`

Implement: Retry policies and metrics.

Done when: tested.

### S19-4 Calendar and notification outputs

*As a user I can push milestones to my calendar.*

Tests first:
- `BidMilestone_CreatesCalendarEvent_WhenUserEnabled`
- `Disabled_CreatesNothing`
- `UpdatedMilestone_UpdatesEvent`

Implement: Optional per-user calendar write; notifications through Teams or email via rule actions.

Done when: tested against the fake server.

## Compliance evidence

Permission ADR; consent and least-privilege evidence. NIST 3.1.5, 3.13.x.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s19/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s19-microsoft-365-integration.md.
Check every entry criterion for sprint 19 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S19-1, S19-2, S19-3, S19-4.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
