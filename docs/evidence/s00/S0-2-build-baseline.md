# S0-2 evidence: build baseline and central package versions

Date: 2026-11-19 (UTC). Commits `028b3ab`..HEAD on `main`.

## Controls supported

| Control | What this story provides |
| --- | --- |
| NIST 800-171 3.4.1 / 3.4.2 (baseline configuration, security configuration settings) | Build security settings declared once in `Directory.Build.props` (nullable, warnings as errors, recommended analyzers, code style enforced in build, deterministic) and asserted by `Crm.Architecture.Tests.BuildConfigurationTests`. |
| NIST 800-171 3.14.1 (flaw remediation) / E8 patch applications (tooling) | One place to pin and bump every NuGet version (`Directory.Packages.props`, ADR-010). `PackageManagementTests` blocks project-local versions. Versions are taken from the feed and logged with dates. |
| E39 supply chain | `nuget.config` clears sources and uses nuget.org only (S0-1). SDK pinned in `global.json`. Packages added only when a story needs them. |

## Verification performed

| Check | Result |
| --- | --- |
| `dotnet test --solution Crm.slnx` | 8 passed, 0 failed (architecture tests: solution layout, 5 quality gates, 2 CPM rules) |
| `dotnet build Crm.slnx` Debug and Release from clean `bin/obj` | 0 warnings, 0 errors |
| `dotnet format Crm.slnx --verify-no-changes` | exit 0 |
| Deliberate warning probe in `Crm.Platform` (unused local, null to non-nullable, null dereference) | Build failed with CS0219, CS8600, CS8602 as errors; probe removed |
| Package versions queried with `dotnet package search <id> --exact-match` | xunit.v3 4.0.3, Shouldly 4.3.0 (latest stable on 2026-11-19) |

## Exceptions

- `CA1707` (no underscores in identifiers) is turned off for `tests/**/*.cs` only, because the mandated test naming is `Method_Scenario_ExpectedResult`. It stays on for production code.
- `make verify` did not exist yet (Makefile targets arrive in S0-3), so the backend parts of verify were run directly, as shown above.
