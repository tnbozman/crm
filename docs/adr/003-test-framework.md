# ADR-003: Test framework and runner

- Status: Accepted (2026-11-19, S0-2)
- Supersedes: the default in `docs/architecture.md` §10

## Context

The plan defaults to xUnit v3, Shouldly and NSubstitute. On the .NET 10 SDK, xunit.v3 4.x runs on Microsoft.Testing.Platform (MTP). `dotnet test` in its classic VSTest mode refuses to run MTP test projects (`error: Testing with VSTest target is no longer supported`). The only way to keep VSTest is to pin older packages or override package targets.

## Decision

- **xunit.v3** for tests, **Shouldly** for assertions. **NSubstitute** stays the mocking choice; it is added to `Directory.Packages.props` with the first story that mocks an external boundary.
- `global.json` sets `"test": { "runner": "Microsoft.Testing.Platform" }`, so `dotnet test` uses MTP.
- Test projects reference `xunit.v3` only. They do not reference `Microsoft.NET.Test.Sdk` or `xunit.runner.visualstudio` (VSTest only).

## Consequences

- MTP `dotnet test` syntax: `--project <path>` or `--solution Crm.slnx`, not a positional path.
- Trait filters use xUnit's MTP options, not VSTest `--filter` expressions:
  - `make test-unit`: `--filter-not-trait "Category=Integration" --filter-not-trait "Category=Architecture"`
  - `make test-int`: `--filter-trait "Category=Integration"`
  - `make test-arch`: `--filter-trait "Category=Architecture"`
  
  The Makefile spec in `docs/environment.md` §3 is updated in S0-3.
- When a filter matches no tests in a project, MTP exits with code 8 ("zero tests ran"). S0-3 must handle this, for example with `--minimum-expected-tests 0` or by narrowing to test projects, and must not hide real failures.
- Coverage moves from VSTest `--collect:"XPlat Code Coverage"` to an MTP coverage extension. Choose it when the `coverage` target is implemented.
- VS Code C# Dev Kit Test Explorer supports MTP (checked in S0-8).
