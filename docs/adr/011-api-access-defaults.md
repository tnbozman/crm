# ADR-011: API access defaults

- Status: Accepted (2026-11-19, S0-4; confirmed in review)

## Decision

1. **Deny by default.** A fallback authorisation policy requires an authenticated user for every endpoint.
2. **Open only through a named policy.** An endpoint that does not need a signed-in user names a policy from `HostPolicies` (for example `.RequireAuthorization(HostPolicies.Anonymous)`). It does not use `.AllowAnonymous()`. Today only `/api/v1/health` is open. Each opening needs a test, and an architecture test will later allow-list them.
3. **No route probing.** An anonymous request to any `/api` route other than health gets 401 problem details, whether or not the route exists. Signed-in callers get 404 problem details for unknown routes.

## Consequences

- Tests for unknown routes (including S0-7's `Get_UnknownApiRoute_Returns404ProblemDetailsNotHtml`) use a signed-in test client.
- The `Anonymous` policy is `RequireAssertion(_ => true)`, so authentication middleware still runs for health. That is cheap and harmless.
- Until Entra ID sign-in (S1-5), the only authentication scheme is `NoSignIn`, which never authenticates and answers 401.
- Test hosts add throwaway endpoints through the internal `IApiEndpoints` seam until `IModule.MapEndpoints` replaces it (S1-1).

## Alternatives rejected

- `.AllowAnonymous()`: idiomatic, but openings are harder to find and allow-list, and it departs from CLAUDE.md's wording.
- An anonymous `/api/{**path}` catch-all returning 404: simpler, but tells unauthenticated callers which routes exist.
