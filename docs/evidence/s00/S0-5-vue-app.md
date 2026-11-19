# S0-5 evidence: Vue app, frontend quality gates, generated API client

Date: 2026-11-19 (UTC). Commits `e6df8d2`..HEAD on `main`.

## Controls supported

| Control | What this story provides |
| --- | --- |
| E8 patch applications / NIST 800-171 3.14.1 | Exact npm pins plus a committed lockfile. pnpm's 24-hour release-age policy is kept on with no exclusions (ADR-002), which reduces exposure to freshly published malicious releases. |
| NIST 800-171 3.4.1 / 3.4.2 (baseline configuration) | Strict TypeScript, ESLint with `no-explicit-any` as an error, Prettier, and `vue-tsc`, all enforced by `make lint` and `make verify`. |
| CLAUDE.md "never hand-write API types" | `src/web/src/api/schema.d.ts` is generated from `openapi/Crm.Host.json`. `apiFetch` accepts only GET paths present in it. `make api-client-check` fails on drift. |
| Accessibility | An axe check runs on the app shell (CLAUDE.md: one per new screen). |

## Tests (`src/web/src/App.spec.ts`, Vitest + Testing Library + MSW)

| Test | Seen failing first |
| --- | --- |
| `App › shows the health status returned by the API` | `App.vue` missing. The corrected test was also run against a shell that never calls the API, and failed |
| `App › shows an error state when the API fails` | no `role="alert"`, unhandled `ApiError` |
| `App › has no axe violations` | first failed on duplicate `main` landmarks because test renders leaked (harness fixed: explicit `cleanup()`). A planted unnamed `<button>` was reported as `button-name`, then removed |

## Temporary checks (planted, then removed)

| Plant | Result |
| --- | --- |
| `ref<any>` in `health-store.ts` | `pnpm lint` exit 1, `no-explicit-any` |
| `: any` in `HealthStatus.vue` | `pnpm lint` exit 1 |
| a failing Vitest test | `make test-web` exit 0 before wiring (stub), exit 2 after |
| `any` again | `make lint` exit 0 before wiring, exit 2 after |
| stale staged `schema.d.ts` | `make api-client-check` exit 2 |
| a stub for already-ticked S0-2 | `make-targets.sh` check D fails |

## Done-when results (2026-11-19)

`pnpm lint`, `pnpm format:check`, `pnpm typecheck`, `pnpm test --run` and `pnpm build` all exit 0. `make api-client-check` exits 0. `make verify` exits 0 (see the sprint log in the commit history).

## Test-harness findings

- MSW 3 renamed `onUnhandledRequest` to `onUnhandledFrame`. The old name was ignored silently, so unmocked requests reached the network. Type checking caught it.
- With Vitest globals off, Testing Library does not clean up automatically. Setup calls `cleanup()` explicitly.
