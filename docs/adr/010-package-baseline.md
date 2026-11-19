# ADR-010: Central package management and version baseline

- Status: Accepted (2026-11-19, S0-2)

## Decision

- Every NuGet version is pinned once in `Directory.Packages.props` (`ManagePackageVersionsCentrally=true`). Project files carry `PackageReference` without `Version`. `Crm.Architecture.Tests` enforces both rules.
- ASP.NET Core and EF Core packages follow the target framework's major version (10.x for net10.0), even when a newer major is the feed's latest stable.
- Packages are added when a story first needs them, not ahead of time. Versions are the latest stable on nuget.org at the time, checked with `dotnet package search <id> --exact-match`, never guessed.
- Each addition or bump appends a row to the log below.

## Baseline log

| Date (UTC) | Package | Version | Feed query | Story |
| --- | --- | --- | --- | --- |
| 2026-11-19 | xunit.v3 | 4.0.3 | latest stable on nuget.org | S0-2 |
| 2026-11-19 | Shouldly | 4.3.0 | latest stable on nuget.org | S0-2 |
| 2026-11-19 | Microsoft.Testing.Extensions.CodeCoverage | 18.12.0 | latest stable on nuget.org | S0-3 |
| 2026-11-19 | Microsoft.AspNetCore.OpenApi | 10.0.15 | latest 10.x; 11.0.0 targets .NET 11 | S0-4 |
| 2026-11-19 | Microsoft.Extensions.ApiDescription.Server | 10.0.15 | latest 10.x; 11.0.0 targets .NET 11 | S0-4 |
| 2026-11-19 | Microsoft.AspNetCore.Mvc.Testing | 10.0.15 | latest 10.x; 11.0.0 targets .NET 11 | S0-4 |

Also pinned outside `Directory.Packages.props`:

| Date (UTC) | Item | Version | Where |
| --- | --- | --- | --- |
| 2026-11-19 | .NET SDK | 10.0.100, `rollForward: latestFeature` (10.0.404 installed) | `global.json` |
| 2026-11-19 | dotnet-ef, reportgenerator | 10.0.8, 5.5.10 | `.config/dotnet-tools.json` (S0-1) |

Considered and not adopted: `Microsoft.NET.Test.Sdk` 18.12.0 and `xunit.runner.visualstudio` 4.0.1, which only VSTest needs (see ADR-003).
