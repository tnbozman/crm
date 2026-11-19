# S1-2 evidence: current user and authorisation primitives

Date: 2026-11-19 (UTC). Branch `sprint-1`, commits `ad7aa8f`..`2369940` plus the docs commit that ticks the story.

## Controls supported

| Control | What this story provides |
| --- | --- |
| NIST 800-171 3.1.1 (limit access to authorised users) | `AddCrmAuthorization` sets a fallback policy requiring a signed-in user on every endpoint; tested to refuse anonymous and admit signed-in principals. |
| NIST 800-171 3.1.2 (limit to authorised functions) / 3.1.5 (least privilege) | One policy per application role. Each is tested to admit its role and refuse a user holding every other role; over HTTP an Admin-only endpoint returns 403 to Sales and Finance, 200 to Admin. |
| NIST 800-171 3.1.1 / ADR-011 (no unintended openings) | `All_Endpoints_HaveAuthorisation_OrAreInAnonymousAllowList` requires every endpoint to name a policy other than `Anonymous`, unless allow-listed with a reason. |
| E8 restrict administrative privileges (application part) | Admin capability is a named policy (`Policies.Admin`) bound to the Entra app role `Admin`, so it can be granted and reviewed in one place. |

## Tests, each seen failing first

| Test | First failure |
| --- | --- |
| `CurrentUser_FromClaims_ExposesIdRolesTeamsAndBusinessUnits` | did not compile (no `Crm.Platform.Security`) |
| `CurrentUser_FromUnauthenticatedPrincipal_IsAnonymous` | threw `ArgumentNullException` |
| `CurrentUser_AuthenticatedWithoutGuidUserId_Throws` (missing, not a GUID) | no `InvalidOperationException` |
| `FallbackPolicy_RequiresAuthenticatedUser` | did not compile (no `AddCrmAuthorization`) |
| `RequireRole_Policy_ForbidsOtherRoles` | policy `Role:Admin` not registered (null) |
| `RequireRole_Policy_AllowsThatRoleOnly` (6 roles) | passed on first run (role policies already existed); then failed 6 of 6 against a planted policy requiring a role nobody holds, which was removed |
| `AddCurrentUser_InScope_ReadsTheUserContextsPrincipal` | did not compile (no `IUserContext`) |
| `FakeAuth_UserWithRolesTeamsAndBusinessUnits_IsReadByCurrentUser` (HTTP) | did not compile (no `WithTeams`), then `me` returned no teams |
| `GetAdminOnly_AsSalesAndFinance_Returns403`, `GetAdminOnly_AsAdmin_Returns200` (HTTP) | 404 (no endpoint) |
| `All_Endpoints_HaveAuthorisation_OrAreInAnonymousAllowList_FlagsEndpointsWithoutAPolicy` | stub rule reported nothing; the test host's four throwaway endpoints name no policy |
| `All_Endpoints_HaveAuthorisation_OrAreInAnonymousAllowList` | passes on the real host; failed (with the test above) when one allow-list entry was misspelt, so stale entries are caught |

`make verify` green on 2026-11-19: unit 35, integration 6, architecture 43, Vitest 13; no High or Critical vulnerabilities.

## Decisions

- **User IDs are GUIDs** (`ClaimTypes.NameIdentifier`). Access (S1-6) will set this claim to the Access user's key at sign-in. An authenticated principal without one fails closed with a clear error.
- **Teams and business units are CRM claims** (`crm:team`, `crm:business_unit`), added by Access, not Entra groups (avoids group overage; architecture.md §6).
- **`IUserContext`** supplies the principal; `ICurrentUser` is read once per scope. The default reads the request's user; work outside a request (the outbox relay, S1-5) supplies its own.
- **Policy names** are `Role:<Role>` constants in `Policies`, so attributes stay compile-time constants.
- **Allow-list** for endpoints without a named policy: health and the SPA shell (anonymous, ADR-011) and the `/api` catch-all (fallback policy, so signed-in callers get 404, ADR-011). The story listed only health; the other two are there by ADR-011's design.
- **Test users** (`TestUserBuilder`) now have GUID IDs, teams and business units. `FakeAuthTests` was rewritten to read through `ICurrentUser`, as S0-9 planned ("placeholder until S1 adds ICurrentUser").
