# S0-8 evidence: dev loop and VS Code

Date: 2026-11-19 (UTC). Commits `b583b10`..HEAD on `main`.

## Controls supported

| Control | What this story provides |
| --- | --- |
| NIST 800-171 3.4.1 / 3.4.2 (baseline configuration) | The developer workstation configuration (`.vscode`, `make dev`) is versioned and checked by `scripts/checks/vscode-config.sh` in `make lint`. All tasks go through `make`, the single interface. |
| CLAUDE.md "no secrets in config" | The MSSQL profile `crm-dev` keeps `savePassword: false`. No passwords are in `.vscode`. |

## Automated checks (`scripts/checks/vscode-config.sh`, run by `make lint`)

| Check | Seen failing first |
| --- | --- |
| A. every `.vscode/*.json` is valid JSON | passes (no comments in the files) |
| B. every `preLaunchTask` names a task | passes |
| C. every task runs exactly one `make` target listed by `make help` | `web dev` ran `make build-backend && pnpm --dir src/web dev` (now `make dev-web`). A planted `pnpm` task failed `make lint` (exit 2) once wired; before wiring it passed (exit 0) |
| D. every compound member exists | passes |
| E. `vitest.rootConfig` points at an existing config | `.vscode/settings.json` had `vitest.root`, which Vitest Explorer 1.52 does not define (silently ignored) |
| F. every `launchSettingsProfile` exists in `launchSettings.json` | passes once G was fixed |
| G. the Vite `/api` proxy targets a URL of the `Host (.NET)` launch profile | `Host (.NET)` had no launch profile (the debugged host would listen on the default URLs, not 5001), and there was no proxy |

## Smoke observations (2026-11-19)

Taken read-only against a `Full stack` debug session started in VS Code (host process from `bin/Debug/net10.0/Crm.Host.dll` under `vsdbg`, Vite on 5173):

- `https://localhost:5001/api/v1/health` returns `{"status":"ok"}`. `http://localhost:5000/api/v1/health` returns `{"status":"ok"}`.
- `http://localhost:5173/` returns 200 `text/html` from Vite.
- `http://localhost:5173/api/v1/health` returns `{"status":"ok"}` through the proxy.
- `http://localhost:5173/api/v1/nope` returns 401 `application/problem+json` through the proxy (deny by default holds in the dev loop).

## `make dev` bug found and fixed (`make check-dev`)

- The spec's recipe `dotnet watch --project … -p:SkipFrontend=true` never starts the host. `dotnet watch` parses `-p` as `--project` (`❌ Cannot specify both '--project' and '-p' options`). Vite kept running, so `make dev` looked alive while every `/api` call through the proxy failed with `ECONNREFUSED 127.0.0.1:5001`.
- `scripts/checks/dev-smoke.sh` (`make check-dev`) failed on it after 120 s, then passed after the fix. The fix passes `SkipFrontend=true` as an environment variable, which MSBuild reads as a property, and adds `wait -n` with an `EXIT` trap so `make dev` stops when either process dies. The check is not in `make verify` because it needs ports 5001 and 5173.
- `dotnet watch` ignores TERM and INT when it has no terminal, so the smoke script escalates to KILL for its process group. In a terminal, Ctrl+C stops `make dev` normally.

## Demo step 4 (partly automated, 2026-11-19)

- Under `make dev`, editing the health response in `Program.cs` changed `GET http://localhost:5173/api/v1/health` from `{"status":"ok"}` to `{"status":"ok-hot-reload"}` with no restart (`dotnet watch 🔥 Hot reload enabled`). The edit was reverted.
- Vue HMR needs a connected browser (Vite logs `hmr update` only then), so it is on the checklist below.

## `make verify` (2026-11-19)

Exit 0: lint (including `vscode-config.sh` and `make-targets.sh`), build, 6 unit, 4 integration and 8 architecture tests, 13 Vitest tests, `api-client-check`, and `check-spa`.

## Demo step 5: performed 2026-11-19 by the developer; all passed (noted in the sprint log)

Performed in VS Code by the developer:

- [x] Run and debug the host (`Host (.NET)` or `Full stack`), and hit a breakpoint in `Program.cs` (for example, the health lambda).
- [x] Test Explorer (C# Dev Kit): run `GetHealth_Anonymous_Returns200WithStatusOk` and debug it to a breakpoint.
- [x] Test Explorer (Vitest): run `App › shows the health status returned by the API` and debug it to a breakpoint.
- [x] MSSQL extension: connect with profile `crm-dev` (enter the SA password when prompted) and run `SELECT @@VERSION`.
- [x] `make dev`: edit a Vue component and see HMR in the browser (C# hot reload was checked automatically, above).

Finding: Vitest Explorer's activation event is `workspaceContains:**/*{vite,vitest}*.config*`, which VS Code checks only when a window opens. This window predated `src/web/vite.config.ts` (S0-5), so the extension never activated (it had no log folder) until **Developer: Reload Window**. Fresh clones are not affected.
