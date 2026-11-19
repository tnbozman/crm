# S1-3 evidence: mediator

Date: 2026-11-19 (UTC). Branch `sprint-1`, commits `fdb943d`..`e74020e` plus the docs commit that ticks the story. Design: ADR-009 (updated with the names as built).

## Controls supported

| Control | What this story provides |
| --- | --- |
| NIST 800-171 3.1.1 / 3.1.2 (authorised users and functions) | Every cross-module request is authorised as the current user before its handler runs. A request names its policy with `[RequiresPolicy]`; without one the caller must still be signed in (fail closed). Negative tests: a Sales user sending a Finance-only query and an anonymous caller are both refused, and the handler never runs. |
| NIST 800-171 3.3.1 / 3.3.2 (audit records, traceable to users) | Each scope gets a correlation ID; envelopes carry correlation, causation and originating user IDs; subscribers inherit the correlation and see the delivered message as causation. |
| CLAUDE.md logging rule / NIST 3.3.1 | The pipeline logs request type, correlation ID and duration only. A test sends a request containing a marker string and asserts no log line contains it. |
| NIST 800-171 3.14.x / CLAUDE.md input validation | Requests are validated (data annotations) before authorisation and the handler; failures are 400 problem details with field errors. |
| NIST 800-171 3.4.1 (baseline configuration) | Startup fails when a module's request has no handler or several; architecture tests require each request's handler to live in its owning module, and forbid services, controllers and handlers depending on `IEventPublisher`. |

## Tests, each seen failing first

| Test | First failure |
| --- | --- |
| `Send_Query_InvokesTheSingleHandler_AndReturnsItsResponse` | did not compile (no `Crm.Platform.Messaging`) |
| `Send_WithNoHandler_Throws` | DI's generic message, not the request-naming one |
| `Send_RunsInCallersScope_CurrentUserIsTheSignedInUser` | passed on first run (mediator already scoped); failed with the mediator planted as a singleton, then restored |
| `MessageEnvelope_ForEvent_CarriesIdsTypeAndPayload` | did not compile (no `IIntegrationEvent`, `MessageEnvelope`) |
| `Publish_Event_InvokesEveryHandler` | did not compile (no `IEventPublisher`, `AddEventSubscriber`) |
| `Startup_RequestWithNoHandler_FailsValidation`, `Startup_RequestWithTwoHandlers_FailsValidation` | no exception from `AddModules` |
| `Send_WithBehaviours_RunsThemInRegistrationOrderAroundTheHandler` | did not compile (no `IRequestBehaviour`) |
| `Send_WithoutCorrelation_AssignsOneSharedByTheScope` | did not compile (no `IMessageContext`) |
| `Send_Logs_RequestTypeAndCorrelation_NeverThePayload` | no log lines |
| `Send_InvalidRequest_ThrowsRequestValidationException_WithoutCallingTheHandler` | did not compile, then no exception |
| `Send_RequestRequiringFinance_AsSales_ThrowsForbidden_WithoutCallingTheHandler` | no exception (attribute and exception existed, no behaviour) |
| `Send_RequestRequiringFinance_AsFinance_ReachesTheHandler`, `Send_RequestWithoutPolicy_AsAnonymous_ThrowsForbidden` | passed on first run; each failed against a planted fault (an open default policy; a policy nobody can meet), then restored |
| `Pipeline_RunsInOrder_CorrelationLoggingValidationAuthorisation` | passed on first run; failed with validation and authorisation swapped, then restored |
| `Publish_Event_SubscriberSeesEnvelopeCorrelation_AndMessageAsCausation` | context empty |
| `ForbiddenException_IsConvertedTo403ProblemDetails`, `RequestValidationException_IsConvertedTo400ProblemDetails_WithFieldErrors` (HTTP) | 500 |
| `Host_Started_ProvidesTheMediatorPerScope` | mediator not registered |
| `EachRequest_HasExactlyOneHandler_InOwningModule` (+ `_FlagsRogueOnly`) | stub rule reported nothing (Rogue: an unhandled query, and a handler for Sample's query) |
| `Services_DoNotCall_MediatorPublish` (+ `_FlagsRogueOnly`) | stub rule reported nothing (Rogue: a service taking `IEventPublisher`) |

`make verify` green on 2026-11-19: unit 54, integration 6, architecture 47, Vitest 13; no High or Critical vulnerabilities.

## Decisions

- **Split** into S1-3a (core: send, publish, envelope, startup validation), S1-3b (pipeline and host) and S1-3c (architecture rules).
- **`IEventPublisher` separate from `IMediator`**: the same class implements both, but only the outbox relay (S1-5) should depend on `IEventPublisher`, so `Services_DoNotCall_MediatorPublish` is a dependency rule over `Services`, `Controllers` and `Handlers` namespaces, broader than the story's "services".
- **`IEventSubscriber<TEvent>`** instead of `IEventHandler<T>`: the analyzer (CA1711) reserves the `EventHandler` suffix.
- **`IModule.ContractsAssembly`** added so startup validation knows each module's requests; the architecture tests now use it instead of a naming convention.
- **Authorisation**: `[RequiresPolicy(Policies.X)]` on the request; otherwise signed in. Validation runs before authorisation, the order ADR-009 sets.
- **New typed exceptions** `ForbiddenException` (403) and `RequestValidationException` (400 with field errors), mapped by the existing filter; messages are not copied into responses.
- **Package**: `Microsoft.Extensions.TimeProvider.Testing` 10.11.0 for tests (latest 10.x; ADR-010 log).

## Open

- Audit of permission-relevant reads joins the pipeline in S2.
- `Publish` logs nothing yet; event logging comes with the relay (S1-5).
