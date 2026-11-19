# S0-11 evidence: host on MVC controllers

Date: 2026-11-19 (UTC). Commits `bafdc14`..HEAD on `main`. Follow-up to the revised S0-4 (`docs/architecture.md` §2a, CLAUDE.md layering rules).

## Controls supported

| Control | What this story provides |
| --- | --- |
| NIST 800-171 3.1.1 / 3.1.2 / 3.1.5 (deny by default, least privilege) | The anonymous allow-list is now an executable check. Two architecture tests read the live endpoint table: only `HealthController` and the SPA fallback carry `HostPolicies.Anonymous`, and no endpoint carries `[AllowAnonymous]` (ADR-011). |
| NIST 800-171 3.14.1 / CLAUDE.md "never leak stack traces" | Typed service exceptions become RFC 9457 problem details through one platform filter. Exception messages are not copied into responses. Unknown exceptions still return a 500 with no detail in Production. |

## Tests, each seen failing first

| Test | Category | Seen failing first |
| --- | --- | --- |
| `HealthController_IsOnlyEndpointUsingAnonymousPolicy` | Architecture | health was the minimal route `/api/v1/health`, not a controller action |
| `Endpoints_DoNotUseAllowAnonymous` | Architecture | passed on arrival (guard) |
| `ServiceException_IsConvertedToProblemDetails_ByExceptionFilter` | unit | 500 (exception unhandled) before `ProblemDetailsExceptionFilter` |

**Unchanged and green:** every S0-4 and S0-7 test (`HealthEndpointTests`, `ApiAuthorisationTests`, `ErrorHandlingTests`, `OpenApiDocumentTests`, `SpaHostingTests`). `git diff d073946 -- <those files>` is empty.

## Design notes

- `Crm.Host/Controllers/HealthController.cs` and `Crm.Host/Dtos/HealthResponse.cs` follow §2a. The host has no services or models yet.
- `Crm.Platform/Errors/`: `NotFoundException` and `ProblemDetailsExceptionFilter`. Only `NotFoundException` exists, because only it has a test. `Conflict`, `Validation` and `Forbidden` join the filter with the first story that throws them.
- The `IApiEndpoints` test seam is removed. `TestOnlyController` (test assembly) is added to the test host with `AddApplicationPart`.
- OpenAPI regenerated: the tag changes from `Crm.Host` to `Health`, and MVC's default formatters also list `text/plain` and `text/json` for `HealthResponse`. The generated client still has the `application/json` body that `apiFetch` uses.
