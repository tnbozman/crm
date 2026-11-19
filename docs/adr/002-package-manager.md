# ADR-002: Frontend package manager

- Status: Accepted (2026-11-19, S0-5)
- Supersedes: the default in `docs/architecture.md` §10

## Decision

- **pnpm**, pinned through `"packageManager": "pnpm@12.21.0"` in `src/web/package.json` (Corepack enforces it). Installs in CI and post-create use `--frozen-lockfile`.
- **Exact versions.** Every dependency is pinned without `^` or `~`, and `pnpm-lock.yaml` is committed.
- **Keep pnpm's release-age policy.** pnpm 12 refuses versions published within its `minimumReleaseAge` window (24 hours by default). We keep it on and never add `minimumReleaseAgeExclude` entries. "Latest stable" in CLAUDE.md therefore means the latest stable release that has cleared the window.

## Why

A freshly published compromised release is the most common npm supply-chain attack, and most are pulled within hours. A 24-hour delay costs little. During S0-5 pnpm silently wrote exclusions when exact fresh versions were requested (vitest 5.1.1, published the day before; @testing-library/jest-dom 8.0.0, published four hours before). We removed the exclusions and used vitest 5.1.0 and jest-dom 7.0.1.

## Consequences

- When a critical fix is younger than the window, document the exception in a commit and an evidence note before adding a temporary exclusion. Remove the exclusion when the window passes.
- S0-10's `pnpm audit` gate and Dependabot or Renovate must be configured to respect the same delay.
