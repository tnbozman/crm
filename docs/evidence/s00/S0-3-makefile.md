# S0-3 evidence: Makefile as the single interface

Date: 2026-11-19 (UTC). Commits `736a41b`..HEAD on `main`.

## Controls supported

| Control | What this story provides |
| --- | --- |
| NIST 800-171 3.4.1 / 3.4.2 (baseline configuration) | One documented entry point (`make`) for build, test, lint and verify. `make verify` is the gate CI will run (S0-10). |
| NIST 800-171 3.14.1 (flaw remediation) | `make verify` fails on any failing test in any category, so regressions cannot pass the gate quietly. |

## Automated checks (`scripts/checks/make-targets.sh`, run by `make lint` and so by `make verify`)

- A. Every `make <target>` in the CLAUDE.md *Commands* block appears in `make help`.
- B. Every target in the `docs/environment.md` §3 `.PHONY` list appears in `make help`.
- C. Every stub is `$(call todo,S<n>-<n>,<message>)`, which names the story that will implement it.

Each check was seen to fail first: 12 targets missing (A), 11 missing (B), and two planted bad stubs (C). Removing `dev` made `make lint` fail with exit 2.

## Temporary verification: verify fails when a test fails

Planted one failing test per category, then removed them:

| Target | Exit | Failure reported |
| --- | --- | --- |
| `make test-unit` | 2 | planted unit failure only |
| `make test-int` | 2 | planted integration failure only |
| `make test-arch` | 2 | planted architecture failure only |
| `make verify` | 2 | stopped at `test-unit` |
| `make verify` after removal | 0 | |

## Target inventory (each run once on 2026-11-19)

| Status | Targets |
| --- | --- |
| Runs, exit 0 | help, doctor, setup (backend part), hooks, build, build-backend, test, test-unit, test-int, test-arch, lint, format, format-check, publish, verify, coverage, clean |
| Runs, exit 1 by design | db-up: needs the Docker host; prints why inside the devcontainer |
| Stub, exit 0 with `TODO(<story>)` | build-web, test-web, plus frontend parts of setup, lint, format and format-check (S0-5); api-client (S0-4); api-client-check (S0-5); run (S0-7); dev (S0-8); db-reset, migrate, migration-add, module-new (S1-2); test-e2e (S1-7); ai-smoke (S2-7) |

## Decisions and exceptions

- Stubs exit 0 so that `make verify` can go green before the frontend exists, and so that `post-create.sh` (which calls `make db-reset` now that `Crm.Migrator` exists) does not fail. The `TODO(<story>)` line is printed to stderr every time.
- `--ignore-exit-code 8` ignores only "zero tests ran". The planted failures above show that real failures still exit non-zero.
- The spec's `help` pattern did not allow digits, so `test-e2e` was hidden. Fixed in the Makefile and noted in `docs/environment.md` §3.
