# S0-4 evidence: host, deny-by-default authorisation, problem details, OpenAPI

Date: 2026-11-19 (UTC). Commits `73d3931`..HEAD on `main`.

## Controls supported

| Control | What this story provides |
| --- | --- |
| NIST 800-171 3.1.1 / 3.1.2 (authorised access, limit to permitted functions) | A fallback authorisation policy requires an authenticated user for every endpoint. Opening an endpoint needs a named policy (`HostPolicies.Anonymous`, used by health only). |
| NIST 800-171 3.1.5 (least privilege), deny by default | Anonymous requests to unknown `/api` routes get 401, not 404, so the API surface cannot be enumerated without signing in. |
| NIST 800-171 3.14.1 / CLAUDE.md "never leak stack traces" | In Production an unhandled exception returns RFC 9457 problem details with no exception type, message or stack. |
| E8 user application hardening (partial) | Every error response is `application/problem+json`. Security headers follow in S1-8. |

## Tests (`tests/Crm.Host.Tests`, WebApplicationFactory, no database)

| Test | Proves |
| --- | --- |
| `GetHealth_Anonymous_Returns200WithStatusOk` | the only anonymous API works |
| `GetAnyOtherApiRoute_WithoutAuth_Returns401` | the fallback policy covers an endpoint with no explicit policy (throwaway endpoint registered only in the test host) |
| `GetUnknownApiRoute_WithoutAuth_Returns401` | negative: anonymous callers cannot probe routes |
| `GetUnknownApiRoute_Returns404ProblemDetails` | a signed-in caller gets a 404 problem details response |
| `GetFailingEndpoint_InProduction_Returns500ProblemDetailsWithoutStackTrace` | no stack trace, exception type or message leaks |
| `OpenApiDocument_Generated_DescribesHealthEndpoint` | the v1 document includes health |

Each was seen failing first for the stated reason (404; 200 instead of 401; empty body with no content type; exception escaping; missing OpenAPI service). The exception is `GetUnknownApiRoute_WithoutAuth_Returns401`, which passed on arrival because of the fallback policy and is kept as the negative test.

## Other verification

- `make api-client` writes `openapi/Crm.Host.json` and fails if the document is missing or lacks `/api/v1/health`. Regenerating gives no diff. A plain `make build` does not generate it.
- `make api-client-check` (part of `make verify`) fails when the committed `openapi/Crm.Host.json` differs from the regenerated one. Seen failing with a planted stale title, and passing once the document was restored.
- Real host smoke test (`dotnet run`, HTTP on loopback): `GET /api/v1/health` returns 200 `{"status":"ok"}`. `GET /api/v1/nope` without a sign-in returns 401 `application/problem+json`.

## Interim measures (tracked)

- The authentication scheme `NoSignIn` never authenticates anyone. It exists so that the fallback policy answers 401 instead of failing. Entra ID replaces it in S1-5. It accepts no credentials and holds no secrets.
- `IApiEndpoints` (internal, visible to `Crm.Host.Tests`) lets the test host add throwaway endpoints. `IModule.MapEndpoints` replaces it in S1-1.
- The signed-in test scheme lives in `Crm.Host.Tests` until S0-9 moves it to `Crm.Platform.Testing` as `FakeAuthHandler`.
