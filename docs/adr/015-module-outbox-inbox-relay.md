# ADR-015: Module outbox, inbox and relay as built

- Status: Accepted (2026-11-20, S1-5). Implements ADR-009 rule 4 within ADR-013 and ADR-014.

## Context

ADR-009 gives each module its own outbox, inbox and relay, generated from the template and kept consistent by a shared conformance suite and a drift check. S1-5 builds them. Three constraints shaped the details: `Crm.Testing` is under `src/` and may not reference EF Core (`Projects_ReferencingEntityFrameworkCore_AreModulesOrMigrator`); module types are internal (ADR-012); and modules' messages live in the production `Crm.Mediator`, whose startup validation requires a handler for each request.

## Decision

1. **Code lives in `Persistence/Messaging/`** of each module, generated from `scripts/templates/module/src/Modules/Crm.__Module__/Persistence/Messaging/`: `OutboxMessage`, `InboxMessage`, their configurations, `IOutbox`/`Outbox`, `IInbox`/`Inbox`, `<Module>OutboxRelay` and `<Module>OutboxRelayService`. Tables `<schema>.OutboxMessages` and `<schema>.InboxMessages` (PascalCase, like every other table).
2. **Same transaction without an interceptor.** `IOutbox.Enqueue` adds the outbox row to the module's DbContext, so the service's own `SaveChanges` (or explicit transaction) commits it with the change. The envelope carries the scope's correlation and causation IDs and the signed-in user's ID.
3. **Relay.** Each pass opens the DbContext's connection and takes `sp_getapplock` on `crm:outbox-relay:<schema>` (exclusive, session owner, no wait); a second relay for the module returns at once. It publishes due messages in write order (an identity `Sequence` column), each in its own DI scope through `IEventPublisher`, and saves after each message. A failure records the exception type (never its message), retries after 5 s doubling to at most 1 hour, and dead-letters after 10 attempts. The relay stops between messages when the host stops. A hosted `BackgroundService` runs passes, waiting 1 s when idle and logging and retrying if the database is unavailable.
4. **Inbox.** A subscriber calls `IInbox.TryReceiveAsync(envelope, subscriberName)` first and returns when it is false; otherwise the inbox row is added to the DbContext and committed by the subscriber's own save, so a delivery that fails before saving runs again. Key: (MessageId, Subscriber).
5. **Conformance suite without EF Core.** `Crm.Testing.Outbox.OutboxConformanceTests` is an abstract xUnit class; each module's test project derives from it and supplies an `IOutboxHarness` over its own DbContext. The suite owns the clock (`FakeTimeProvider`) and the publisher (`RecordingEventPublisher`).
6. **Drift check.** `scripts/checks/module-outbox-drift.sh` (in `make lint`) diffs each module's `Persistence/Messaging` with the template after substituting the module name, and rejects files the template does not have.
7. **The two sample modules exist only in the scaffold check.** `new-module.sh` scaffolds Zzz and Yyy in its temporary copy, adds the fixture in `scripts/checks/module-exchange` (Yyy subscribes to `ZzzItemCreated` and sends `GetZzzItemSummary`), and runs an API-level test against a database migrated by `make db-reset`. Committed sample modules would put their messages in the production `Crm.Mediator` and need handlers in the shipped host.
8. **Module API tests use a database with every module migrated** (`<Module>HostDatabase`, through `Crm.Migrator`'s `MigrationRunner`, now public), because the real host composes every module and their relays and subscribers need their tables. Persistence tests keep the module-only database.

## Consequences

- Delivery is at least once; subscribers must use the inbox. Handlers run as the system: a subscriber that needs another module's data cannot send a query as a user (the mediator requires a signed-in user), so it relies on the event's content until a system identity is designed.
- A template change to `Persistence/Messaging` must be copied into every module in the same change, or `make lint` fails.
- Outbox and inbox rows are never purged yet; retention is a later story.
