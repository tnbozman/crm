# S1-5 evidence: module outbox, inbox and relay

Date: 2026-11-20 (UTC). Branch `claude/story-s01-4-tdd-6nnpv7`, commits `616f2e2`..the docs commit. Design: ADR-009 rule 4, as built in ADR-015.

Split (well past the ~400-line guideline): **S1-5a** outbox and relay (`616f2e2`, `4abb6b6`, `6904999`, `15feb1c`, `1e71714`, `1319aa2`, `2d779da`); **S1-5b** inbox (`23d7710`); **S1-5c** drift check and module exchange (`df81513`, `464cdd5`).

## What each module now has

`Persistence/Messaging/` (generated, checked by `module-outbox-drift.sh`): outbox and inbox tables in the module's schema, `IOutbox` (writes in the caller's transaction), `IInbox` (run once per subscriber), a relay (per-module `sp_getapplock`, write order, retry with backoff, dead-letter, stop between messages) and a hosted service running it. The example `Item` writes `<Module>ItemCreated` (`Crm.Mediator.Contracts.<Module>`) when created.

## Controls supported

| Control | What this story provides |
| --- | --- |
| NIST 800-171 3.13.2 (architectural design) | Modules exchange data only through mediator messages; the exchange test proves an event and a query cross between two modules that do not reference each other (architecture tests run on the same copy). |
| NIST 800-171 3.3.1 / 3.3.2 (audit content, traceability) | Each outbox row keeps correlation, causation and originating user IDs, so S2's Audit module can trace an event to the action that caused it. |
| NIST 800-171 3.4.1 / 3.4.2 (baseline configuration) | Every module's outbox code is identical to the reviewed template (`make lint` fails on drift), and every module runs the same conformance suite. |
## Tests (first failure, then green)

Conformance suite `Crm.Testing.Outbox.OutboxConformanceTests`, run by every module's `<Module>OutboxConformanceTests` against its own DbContext on SQL Server:

| Test | First failure |
| --- | --- |
| `Outbox_Message_IsWrittenInSameTransactionAsEntityChange`, `Outbox_RolledBackChange_LeavesNoMessage` | did not compile (no outbox, no `OutboxMessages`, no event) |
| `Relay_PublishesPendingMessages_ThroughMediator_AndMarksProcessed` | did not compile (no relay); then published out of order when `OccurredAt` tied, fixed by the identity `Sequence` column |
| `Relay_Retries_FailedHandler_WithBackoff_ThenDeadLetters` | the subscriber's exception escaped the relay |
| `TwoRelays_ForSameModule_OnlyOneProcesses` | the second relay published both messages (expected 0) |
| `Relay_ResumesAfterRestart_WithoutLosingMessages` | the relay ignored the stop and published every message in one pass |
| `Handler_ReceivingSameMessageTwice_RunsOnce`, `Handler_FailingBeforeItsChangeIsSaved_RunsAgainOnRedelivery` | did not compile (no inbox) |

Template tests: `CreateAsync_WritesItemCreatedToTheOutbox` (did not compile: no outbox in the service) and `Host_RunsTheRelay_SoACreatedItemsEventIsPublished` (outbox row never processed: no hosted relay).

Checks: `new-module.sh` step "the outbox drift check passes on the generated module and fails on a planted change" (script missing), then passes, and fails on a planted `MaxAttempts` change and on an extra file. Step "Zzz and Yyy exchange an event and a query" (Yyy's fixture types missing). With two modules composed it also found two template defects, now fixed: `CreatedAtAction` resolved to the other module's `ItemsController` (500), and module API tests ran on a database without the other module's tables.

`make verify` green on 2026-11-20: unit 62, integration 8, architecture 46, Vitest 13, outbox drift clean; `check-module-new`: architecture 46 and generated module 26 (9 conformance) in the copy with Zzz and Yyy, module exchange 1; no High or Critical vulnerabilities.

## Not done here

- Purging processed outbox and inbox rows (retention): later story.
- A system identity for subscribers that need to send queries: subscribers currently run without a user (ADR-009 rule 6), so they rely on the event's content.
- Audit of the example `Item` (S2).
