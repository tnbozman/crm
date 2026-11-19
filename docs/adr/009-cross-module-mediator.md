# ADR-009: Cross-module communication through an in-house mediator

- Status: Accepted (2026-11-19, Sprint 1 kickoff, before S1-1). Names refined when built in S1-3.

## Context

The modular monolith must keep modules independent: no shared persistence code, no cross-schema access and no compile-time dependency on another module's implementation. The original plan had two channels between modules: synchronous calls through interfaces in each module's Contracts project (`IDirectoryQueries`), and integration events through a transactional outbox dispatched by a hosted service in `Crm.Platform`.

That outbox design needed EF Core in `Crm.Platform` (a shared interceptor, shared outbox and inbox entity configuration, and a dispatcher reading every module's tables). It conflicts with `Projects_ReferencingEntityFrameworkCore_AreModulesOrMigrator` (S0-9), and it is persistence code shared across modules.

## Decision

1. **All cross-module interaction goes through `IMediator`** in `Crm.Platform`:
   - **Queries**: `Send(IQuery<TResponse>)`. This is the normal way to read another module's data.
   - **Commands**: `Send(ICommand<TResponse>)`, used only when the user needs the result at once. Otherwise prefer events. A cross-module command runs in its own transaction: it is never sent inside the caller's transaction, and its handler is idempotent.
   - **Integration events**: `IEventPublisher.Publish(MessageEnvelope)`, called **only by a module's outbox relay**, after the commit. `IEventPublisher` is a separate interface from `IMediator` (one class implements both), so the rule is a plain dependency check. Subscribers implement `IEventSubscriber<TEvent>` (CA1711 reserves the `EventHandler` suffix).
2. **Contracts projects hold only message types**: query, command and event records, response DTOs and constants. They hold no interfaces and no implementation. A request type has exactly one handler, and that handler lives in the module that owns the Contracts project.
3. **Inside a module the mediator is not used.** Controller → service → `DbContext` stays direct (CLAUDE.md layering rules). Cross-module handlers are thin and delegate to the module's own services.
4. **Each module owns its outbox, inbox and relay**, in its own project, schema and `DbContext`:
   - `outbox_messages` and `inbox_messages` tables in the module's schema.
   - A `SaveChanges` hook writes outbox rows in the same transaction as the change.
   - A hosted relay registered by the module reads only that module's outbox through its own `DbContext`, takes `sp_getapplock` on a per-module resource name, calls `IEventPublisher.Publish`, retries with backoff and dead-letters.
   - A subscriber records the message ID in its own inbox in the same transaction as its own change, so a redelivered message runs once.
   - The scaffolder (S1-4, extended in S1-5) generates this code from `scripts/templates/module/`. No project shares it at runtime.
5. **`Crm.Platform` has no EF Core and no persistence code.** It holds `IMediator`, `IEventPublisher`, the message interfaces, the envelope (ID, type, occurred-at, payload JSON, correlation ID, causation ID, originating user ID), the pipeline and handler registration through `IModule`. The S0-9 rule stays unchanged.
6. **The mediator pipeline is where cross-cutting rules apply to every cross-module call**, in this order: correlation and causation IDs, structured logging (IDs only, never payloads), request validation, authorisation as the current user, and audit of permission-relevant reads (from S2). The pipeline wraps `Send`; a request names its policy with `[RequiresPolicy(Policies.X)]`, and a request without one still requires a signed-in user (fail closed). Failures surface as `RequestValidationException` (400) and `ForbiddenException` (403). Queries and commands run in the caller's DI scope, so `ICurrentUser` is the signed-in user. `Publish` sets the scope's correlation ID from the envelope and its causation ID to the delivered message's ID. Events published by a relay carry the originating user ID in the envelope; handlers run as the system identity and use that ID for audit only.
7. **The mediator is written in-house** (about 200 lines: `Send`, `Publish`, the pipeline and handler lookup). No third-party mediator.

## Enforcement

Architecture tests (S1-1 unless noted):

- `ModuleImplementations_DoNotReference_OtherModuleImplementations`.
- `Contracts_Assemblies_ContainOnlyMessagesDtosAndConstants` (no interfaces, no classes with behaviour).
- `Modules_DoNotResolve_OtherModulesServices`: a module's registered services are internal, so only message types cross the boundary.
- `Projects_ReferencingEntityFrameworkCore_AreModulesOrMigrator` (S0-9, unchanged).
- `EachRequest_HasExactlyOneHandler_InOwningModule` (S1-3), also checked at startup so a missing handler stops the host.
- `Services_DoNotCall_MediatorPublish` (S1-3): nothing in a `Services`, `Controllers` or `Handlers` namespace depends on `IEventPublisher`; only a module's relay publishes.
- Startup validation in `AddModules` (S1-3): each request in a module's `IModule.ContractsAssembly` has exactly one registered handler.
- `Handlers_DoNotSendInsideATransaction` is a review rule; an automated check is added if a reliable one can be written.

Copied outbox code is kept consistent by:

- a shared conformance suite in `Crm.Platform.Testing` (test code only, never shipped) that each module's test project runs against its own `DbContext`: same-transaction write, publish and mark processed, retry then dead-letter, inbox run-once, single relay under `sp_getapplock`, resume after restart;
- a drift check in `make verify` (`scripts/checks/module-outbox-drift.sh`) comparing each module's outbox files with the template.

## Consequences

- Modules depend only on message shapes, so a module could later move out of process with limited change.
- Every cross-module call passes one pipeline, which gives uniform authorisation, logging and audit evidence (NIST 3.1.1, 3.1.2, 3.3.1).
- "Go to definition" stops at the request type. Handlers are found by convention (`<Request>Handler` in the owning module).
- Each module carries about 150 lines of outbox, inbox and relay code generated from the template. A template fix must be applied to existing modules; the drift check makes that visible.
- Later sprint text that says "through Contracts" means "by sending a Contracts message through the mediator". AI tools (S25–S28) send queries and commands through the mediator as the signed-in user.

## Alternatives rejected

- **Interfaces in Contracts for synchronous calls** (the original plan): calls are easy to trace, but each interface is a separate place to enforce authorisation and audit, and module implementations are bound to each other's interface shapes.
- **EF Core in `Crm.Platform`, with one shared outbox dispatcher**: least code, but it shares persistence code across modules, puts EF Core in every project that references the kernel (Contracts included), and breaks the S0-9 rule.
- **A separate `Crm.Platform.Persistence` project with the shared outbox**: keeps the kernel free of EF Core, but is still persistence code shared across modules.
- **Mediator only, with no outbox**: an event is lost if the process stops between commit and publish, and making the sender's and receiver's changes atomic would need a distributed transaction.
- **MediatR**: commercial licence for business use since v13.
- **`Mediator` (martinothamar, MIT, source-generated)**: a reasonable fallback if compile-time handler checks are wanted later; not needed for `Send`, `Publish` and a short pipeline, and it adds a dependency to manage under Essential Eight application control.
