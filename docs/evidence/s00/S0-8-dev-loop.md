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

## Demo step 5: to be performed in VS Code and noted in the sprint log

This needs a person in VS Code. Pending at the time of writing:

- [ ] Run and debug the host (`Host (.NET)` or `Full stack`), and hit a breakpoint in `Program.cs` (for example, the health lambda).
- [ ] Test Explorer (C# Dev Kit): run `GetHealth_Anonymous_Returns200WithStatusOk` and debug it to a breakpoint.
- [ ] Test Explorer (Vitest): run `App › shows the health status returned by the API` and debug it to a breakpoint.
- [ ] MSSQL extension: connect with profile `crm-dev` (enter the SA password when prompted) and run `SELECT @@VERSION`.
- [ ] `make dev`: edit a Vue component (HMR) and a C# file (`dotnet watch` hot reload).
