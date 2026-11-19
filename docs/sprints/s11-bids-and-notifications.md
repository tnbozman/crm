# Sprint 11 — Bid management and notifications (3 – 14 May 2027)

**Release**: R2 Deals and bids. **Epics**: E12, E14.

## Goal

Bids have a structured workspace with go/no-go decision, key dates and checklists. Users receive reliable notifications and reminders.

## Demo

1. Open a bid workspace on a deal, record a go/no-go with approvers.
2. Key dates produce reminders and a calendar view.
3. A notification email arrives in Mailpit with a link only, no sensitive content.
4. A user changes their notification preferences.

## Entry criteria

- S10 merged; `make verify` green on `main`.
- Mail relay or Graph send permission decision made (ADR).
- Mailpit added to the devcontainer compose file.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S11-1 Bid workspace

*As a bid manager I run a bid.*

Tests first:
- `GoNoGo_RequiresConfiguredApprovers`
- `GoNoGo_ByNonApprover_Rejected`
- `KeyDates_MustBeInLogicalOrder`
- `ChecklistTemplate_Instantiated_PerBid`
- `Bid_VisibleOnlyWithDealVisibility`

Implement: Bid aggregate in Deals with decision record, dates, checklist items, templates in Config.

Done when: API tested.

### S11-2 Document links

*As a user I link bid documents held in SharePoint.*

Tests first:
- `Link_HostNotInAllowList_Rejected`
- `Link_IsStoredNotFetched` (no outbound call made)
- `Link_Opened_IsNotLoggedWithQueryString`

Implement: Allowed-domain list in Config; links with title and type.

Done when: UI shows link list.

### S11-3 Notification service

*As a user I get notified about things that matter.*

Tests first:
- `Notification_Created_FromEvent_OncePerRecipient`
- `Retry_DoesNotDuplicateEmail`
- `Preferences_Respected_PerType`
- `EmailBody_ContainsLinkAndTitleOnly`
- `Digest_GroupsMultipleNotifications`
- `Email_SendFailure_RetriesWithBackoff_ThenDeadLetters`

Implement: Notifications in Activities: in-app inbox plus email through an `IEmailSender` (SMTP to Mailpit in dev; Graph or relay in Azure).

Done when: Mailpit assertions in integration tests.

### S11-4 Reminders and escalations

*As a user I am reminded before dates pass.*

Tests first:
- `TaskDueSoon_CreatesReminder_OnceAtConfiguredOffset`
- `BidMilestone_CreatesReminder_ForBidManager`
- `Scheduler_OnlyOneInstanceRuns` (applock)
- `Escalation_AfterOverdueDays_NotifiesManager`

Implement: Platform scheduler hosted service using the lease from S1-5.

Done when: tests green.

### S11-5 Bid calendar and preferences UI

*As a user I see dates and manage preferences.*

Tests first:
- Component tests for calendar list and preferences form
- Playwright: change preference, verify no email created

Implement: Read-only calendar view of milestones; preferences page.

Done when: journey green.

## Compliance evidence

Notification content review (no sensitive data in email); scheduler lease test output. NIST 3.1.3, 3.13.x for email content.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s11/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s11-bids-and-notifications.md.
Check every entry criterion for sprint 11 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S11-3, S11-4, S11-1, S11-2, S11-5.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
