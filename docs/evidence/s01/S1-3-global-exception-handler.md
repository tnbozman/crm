# S1-3 added scope evidence: global exception handler

Date: 2026-11-19 (UTC). Branch `sprint-1`, commits `7251397`..`5404958` plus the docs commit. Added to S1-3 at the developer's direction after review found typed exceptions were mapped only inside MVC.

## The gap

`ProblemDetailsExceptionFilter` was an MVC exception filter. It mapped `NotFoundException`, `ForbiddenException` and `RequestValidationException` only when they were thrown by a controller action. Thrown anywhere else (while a result is written, in middleware, in a minimal endpoint) they became a 500 logged as an unhandled error. Sign-in (S1-6) and security middleware (S1-8) will add code outside controllers.

## Change

- `CrmExceptionHandler` (`IExceptionHandler`, in `Crm.Platform.Errors`), registered by `AddCrmErrorHandling()` and run by the host's existing `UseExceptionHandler()`, maps the three typed exceptions for every request. Unknown exceptions are not handled, so the host's default stays: 500 problem details, no detail, logged as an error.
- `ProblemDetailsExceptionFilter` removed. Every existing filter test passes through the handler unedited (renamed `ExceptionHandlerTests`; one test name changed from `_ByExceptionFilter` to `_ByExceptionHandler`).
- `CapturingLoggerProvider` moved to `Crm.Platform.Testing.Logging` and records levels; `CreateClientFor` can capture the host's logs.

## Controls supported

| Control | What this change provides |
| --- | --- |
| NIST 800-171 3.14.1 / CLAUDE.md "never leak stack traces" | One handler for every request; typed exception messages never reach the response; unknown exceptions return 500 without detail. |
| NIST 800-171 3.3.1 / 3.3.5 (audit review, correlation) | Expected 4xx outcomes are not logged as faults, so error alerts (S3) fire on real faults; unexpected exceptions are logged as errors. |

## Tests, each seen failing first

| Test | First failure |
| --- | --- |
| `ForbiddenException_ThrownOutsideMvcFilters_IsConvertedTo403ProblemDetails` | 500 (the filter does not see exceptions from result execution) |
| Existing 404/403/400/500 tests | unchanged; pass through the handler once the filter was removed |
| `ExpectedException_IsNotLoggedAsAWarningOrError` | first run failed on an unrelated Data Protection startup warning (test too broad; narrowed to exception-handling categories and the exception's name). Then failed, with two 403 tests, when the handler was planted to leave `ForbiddenException` unhandled; restored |
| `UnexpectedException_IsLoggedAsAnError` | passes: guards the 500 path's logging |

## Review of exception use across the code base

| Site | Verdict |
| --- | --- |
| `ModuleRegistry`, `RequestHandlerValidation`: `InvalidOperationException` at startup | Right: misconfiguration stops the host. |
| `Mediator` (no handler), `CurrentUser` (no GUID user ID): `InvalidOperationException` at runtime | Right: bugs or sign-in misconfiguration; fail closed as a 500, logged as an error. Startup validation makes the first unreachable in practice. |
| `ValidationBehaviour`, `AuthorisationBehaviour`: `RequestValidationException`, `ForbiddenException` | Right: expected outcomes, mapped to 400 and 403. |
| Three constructors on each typed exception | Required by CA1032 for public exceptions; kept. |

No further refactor: an abstract base for expected errors was considered and rejected, because it would not remove the CA1032 constructors and only the handler uses the classification. CLAUDE.md now says when to use each kind of exception.
