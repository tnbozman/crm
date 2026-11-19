# S0-7 evidence: the .NET build builds and serves the frontend

Date: 2026-11-19 (UTC). Commits `c8944ab`..HEAD on `main`.

## Controls supported

| Control | What this story provides |
| --- | --- |
| E8 application control (partial) | One deployable: `dotnet publish` produces the host with the SPA inside it, with no second server for the frontend (CLAUDE.md). `make verify` proves a clean publish contains it. |
| NIST 800-171 3.4.1 / 3.4.2 (baseline configuration) | The frontend build is part of the .NET build, is incremental, and is reproducible from a clean state. Both are proved in `make verify`. |
| NIST 800-171 3.1.1 / 3.1.5 (deny by default) | The SPA fallback cannot swallow `/api` routes. Anonymous callers still get 401 for unknown API routes (ADR-011). Only the SPA shell and static files are public. |

## Tests (`Crm.Host.Tests`, category `Integration`, against the real built `wwwroot`)

| Test | Seen failing first |
| --- | --- |
| `Get_Root_ReturnsSpaIndexHtml` | `wwwroot/index.html not found … Run make build first`, then 401 until static files were served |
| `Get_ClientRoute_ReturnsIndexHtml` | 401 (no fallback) |
| `Get_UnknownApiRoute_Returns404ProblemDetailsNotHtml` (signed in) | 200 `index.html` once the SPA fallback existed |
| `Get_UnknownApiRoute_WithoutAuth_Returns401NotHtml` | 200 `index.html` once the SPA fallback existed |

The S0-4 unit tests `GetUnknownApiRoute_Returns404ProblemDetails` and `GetUnknownApiRoute_WithoutAuth_Returns401` also caught the SPA fallback swallowing `/api`, before the `/api/{**path}` fallback was added.

## Scripted checks (in `make verify` through `make check-spa`)

| Script | Proves | Shown to fail by |
| --- | --- | --- |
| `scripts/checks/frontend-incremental.sh` | a second `dotnet build` logs `Skipping target "BuildFrontend" because all output files are up-to-date`; touching `src/web/src/App.vue` reruns it | a `BuildFrontend` target without `Inputs`/`Outputs` |
| `scripts/checks/publish-contains-spa.sh` | after deleting `wwwroot` and `artifacts/publish`, `make publish` contains `wwwroot/index.html` and every asset it references (also run twice from fully clean `bin`/`obj`) | a publish with `-p:SkipFrontend=true` |
| `make doctor` (new check) | the HTTPS development certificate exists | before `dotnet dev-certs https` |

## Pitfall from `docs/environment.md` §4

- **Confirmed:** after a clean build the static web assets endpoint manifest is empty (`{"Version":1,"ManifestType":"Publish","Endpoints":[]}`), so `MapStaticAssets` would not serve the build output. The host uses `UseStaticFiles`.
- **Corrected:** publish is not affected. `_IncludePrePublishGeneratedContent` in `Microsoft.NET.Sdk.Publish.ComputeFiles.targets` re-globs `wwwroot/**` at publish time. The spec's `IncludeFrontendInPublish` target was therefore not added (no failing test needed it). The doc is updated.

## Demo (2026-11-19)

- `make build`: Vite writes `src/Crm.Host/wwwroot`, 0 warnings.
- `make run` on `https://localhost:5001`: `GET /` returns 200 `text/html`. `/some/client/route` returns 200 `text/html`. `/api/v1/health` returns `{"status":"ok"}`. `/api/v1/unknown` returns 401 `application/problem+json`. The hashed JS asset returns 200 `text/javascript`.
- `make publish`: `artifacts/publish/wwwroot/index.html` exists.

## Follow-ups (other stories)

- `http://localhost:5000` serves without redirecting to HTTPS. HTTPS redirection and HSTS belong to S1-8 (security headers).
- `make test-int` needs a frontend build first (it uses `SkipFrontend=true`). `make verify` builds before it tests, and a missing build fails with a clear message.

## Bug found after S0-7 (fixed during S0-8 close-out, 2026-11-19)

- `dotnet build` failed after any frontend change that renamed the hashed bundle: `DefineStaticWebAssets: No file exists for the asset … wwwroot/assets/index-<old hash>.js`. MSBuild listed the previous build's `wwwroot` files as static web assets at evaluation, then `BuildFrontend` (Vite with `emptyOutDir`) replaced them. The S0-7 incremental check only used `touch`, which keeps the hashes, so it missed this.
- Reproduced first: `frontend-incremental.sh` now edits `HomeView.vue`, builds, reverts and builds again. It failed with the error above.
- Fix: `<Content Remove="wwwroot\**" />` in `Crm.Host.csproj`, so generated files are not static web assets. The host serves them from disk, and publish still includes them (`publish-contains-spa.sh` passes).
