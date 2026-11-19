# S0-10 evidence: CI, hooks and supply chain

Date: 2026-11-19 (UTC). Commits `5b37872`..HEAD on `main`. **Status: local work complete. The GitHub steps are pending (see the bottom of this note).**

## Controls supported

| Control | What this story provides |
| --- | --- |
| E8 patch applications / NIST 800-171 3.14.1 (flaw remediation) | `make audit` in `make verify` and CI fails on High or Critical advisories (NuGet including transitive, npm). NuGet Audit also fails restore (NU1901–NU1904 as errors). Dependabot proposes updates weekly for NuGet, npm, Actions and the devcontainer. |
| E39 supply chain / NIST 800-171 3.4.1–3.4.2 (baseline configuration) | Every action is pinned to a commit SHA. actionlint is pinned and checksum-verified (a tampered checksum is rejected). CycloneDX SBOMs record what ships. The one-day release-age cooldown is applied to Dependabot (ADR-002). |
| NIST 800-171 3.14.2–3.14.3 (malicious code, security alerts) | CodeQL for C# and TypeScript on every PR, push to `main` and weekly (`security-and-quality`). Secret scanning and push protection: pending, repository setting. |
| NIST 800-171 3.4.3 / 3.4.4 (change control) | The `.githooks` pre-commit hook (lint + unit tests) and pre-push hook (`make verify`); CI on every PR. Branch protection: pending, repository setting. |
| CLAUDE.md "no secrets" | The CI SQL Server password is generated per run and masked. Nothing is stored in the repository or in secrets. |

## Checks, each seen failing first

| Check | Seen failing first |
| --- | --- |
| `scripts/checks/ci-config.sh` (in `make lint`): `ci.yml` runs `make verify`; actions SHA-pinned; actionlint; Dependabot ecosystems; hooks | 8 failures before the files existed. Planted `actions/setup-node@v7` and an invalid expression: "not pinned" and actionlint errors. Before wiring, `make lint` passed the plant (exit 0); after, it failed (exit 2) |
| `make audit` (`scripts/checks/dependency-audit.sh`) | **Story test.** Planted `Newtonsoft.Json` 12.0.1 (GHSA-5crp-9r3c-p9vr, High) and `lodash` 4.17.20 (two High advisories). Both ecosystems failed. NuGet failed at restore (NU1903) and again through `dotnet list --vulnerable` with `NuGetAudit=false`. Plants removed |
| `make check-sbom` (`scripts/checks/sbom.sh`) | no `sbom` target |
| hooks | each commit in this story ran the real pre-commit hook (`make lint`, `make test-unit`) |

## Artefacts

- Dependency audit output (clean tree): [S0-10/dependency-audit-2026-11-19.txt](S0-10/dependency-audit-2026-11-19.txt). There are 2 moderate npm advisories, below the gate, both in test-only tooling: `glob` (GHSA-3g3m-2fjr-6pw4) and `minimatch` (GHSA-g3hv-75wg-m249), via `@testing-library/vue` → `@vue/test-utils` → `js-beautify`. Not shipped. Dependabot will propose fixes once upstream updates.
- SBOM samples (CycloneDX 1.7): [backend](S0-10/sbom-backend-sample.cdx.json) (Crm.Host: Microsoft.AspNetCore.OpenApi, Microsoft.OpenApi, Microsoft.Extensions.ApiDescription.Server) and [frontend summary](S0-10/sbom-frontend-sample.json) (81 production components, including `vue` 3.5.45 and `openvue` 1.1.1).
- Dependabot configuration: [`.github/dependabot.yml`](../../../.github/dependabot.yml).
- Pinned actions: checkout v7.0.1 `3d3c42e5…`, setup-dotnet v6.1.0 `0f4a354d…`, setup-node v7.1.0 `949feb24…`, upload-artifact v7.0.2 `cf430e03…`, codeql-action v4.39.3 `33d7b85d…` (the annotated tag dereferenced to its commit).

## Decisions

- **SQL Server in CI is started with `docker run` in a step, not a `services:` container.** A service container needs its password before any step runs, which would mean a stored secret. The step generates and masks one per run instead.
- **The audit gate is High or Critical.** NuGet Audit, however, fails restore on *any* severity (warnings as errors). That is stricter, and could block builds when a new low advisory appears. The fix then is to upgrade, or as a last resort to suppress that advisory with `NuGetAuditSuppress` and a dated note.
- **The SBOM covers what ships** (`Crm.Host` with its references, and npm `--prod`), not test tooling.

## Pending on GitHub (done-when)

- [ ] CI green on a PR. Record the run URL here.
- [ ] SBOM artifact `sbom-cyclonedx` attached to that run.
- [ ] Branch protection on `main`: require the `verify` check and one review. Save a settings export.
- [ ] Secret scanning and push protection enabled.
- [ ] CodeQL run green. Dependabot alerts and security updates enabled.
