# ADR-011: API access defaults

- Status: Accepted (2026-11-19, S0-4; confirmed in review; SPA shell added in S0-7; controllers in S0-11)

## Decision

1. **Deny by default.** A fallback authorisation policy requires an authenticated user for every endpoint.
2. **Open only through a named policy.** An endpoint that does not need a signed-in user names a policy from `HostPolicies`: `[Authorize(Policy = HostPolicies.Anonymous)]` on a controller, or `.RequireAuthorization(HostPolicies.Anonymous)` on the one remaining minimal endpoint, the SPA fallback. It never uses `[AllowAnonymous]` or `.AllowAnonymous()`. Today the open endpoints are `HealthController` (`GET /api/v1/health`) and the SPA shell fallback (`MapFallbackToFile("index.html")`). Static files in `wwwroot` are served before authentication: they are the public build of the SPA and hold no data. The architecture tests `HealthController_IsOnlyEndpointUsingAnonymousPolicy` and `Endpoints_DoNotUseAllowAnonymous` read the live endpoint table and enforce this allow-list (S0-11). A new opening needs its own test and an update to that allow-list.
3. **No route probing.** An anonymous request to any `/api` route other than health gets 401 problem details, whether or not the route exists. Signed-in callers get 404 problem details for unknown routes. Unknown `/api` routes never fall through to the SPA: `MapFallback("/api/{**path}")` answers them, and it carries no named policy (S0-7).

## Consequences

- Tests for unknown routes (including S0-7's `Get_UnknownApiRoute_Returns404ProblemDetailsNotHtml`) use a signed-in test client.
- The `Anonymous` policy is `RequireAssertion(_ => true)`, so authentication middleware still runs for health. That is cheap and harmless.
- Until Entra ID sign-in (S1-5), the only authentication scheme is `NoSignIn`, which never authenticates and answers 401.
- Test hosts add throwaway endpoints with a test-only controller registered through `AddApplicationPart` (S0-11). Modules register theirs the same way (`IModule` with a controllers assembly, S1-1).

## Alternatives rejected

- `.AllowAnonymous()`: idiomatic, but openings are harder to find and allow-list, and it departs from CLAUDE.md's wording.
- An anonymous `/api/{**path}` catch-all returning 404: simpler, but tells unauthenticated callers which routes exist.
