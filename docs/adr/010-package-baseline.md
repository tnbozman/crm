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
| 2026-11-19 | Microsoft.Data.SqlClient | 7.1.2 | latest stable on nuget.org | S0-9 |
| 2026-11-19 | Respawn | 7.0.0 | latest stable on nuget.org (new major; API checked by compiling) | S0-9 |
| 2026-11-19 | xunit.v3.extensibility.core | 4.0.3 | matches xunit.v3 | S0-9 |
| 2026-11-19 | Bogus | 35.6.5 | latest stable on nuget.org | S0-9 |
| 2026-11-19 | NetArchTest.Rules | 1.3.2 | latest stable on nuget.org | S0-9 |

Also pinned outside `Directory.Packages.props`:

| Date (UTC) | Item | Version | Where |
| --- | --- | --- | --- |
| 2026-11-19 | .NET SDK | 10.0.100, `rollForward: latestFeature` (10.0.404 installed) | `global.json` |
| 2026-11-19 | dotnet-ef, reportgenerator | 10.0.8, 5.5.10 | `.config/dotnet-tools.json` (S0-1) |

### npm (`src/web/package.json`, ADR-002)

Queried with `pnpm view <pkg> version` on 2026-11-19. Each version is the latest stable that has cleared pnpm's release-age window, unless noted. Story: S0-5.

| Package | Version | Note |
| --- | --- | --- |
| vue / pinia / vue-router | 3.5.45 / 4.1.0 / 5.5.1 | |
| vite / @vitejs/plugin-vue | 8.3.8 / 6.0.11 | |
| typescript | 6.0.3 | 7.0.2 is the latest, but typescript-eslint 8.73 supports TypeScript below 6.1.0 only. 6.0.3 is the newest in range; openapi-typescript (peer ^5) was checked to work with it |
| vue-tsc / @vue/tsconfig / @types/node | 3.3.14 / 0.9.1 / 26.6.6 | |
| tailwindcss / @tailwindcss/vite | 4.3.5 / 4.3.5 | eslint-plugin-tailwindcss not added: it does not support Tailwind 4 |
| vitest | 5.1.0 | 5.1.1 was inside the release-age window |
| jsdom | 31.0.1 | |
| @testing-library/vue | 8.1.0 | |
| @testing-library/jest-dom | 7.0.1 | 8.0.0 was inside the release-age window |
| msw | 3.0.7 | v3 option is `onUnhandledFrame` (was `onUnhandledRequest`) |
| axe-core | 4.14.1 | used directly; `vitest-axe` 0.1.0 (2022) looks unmaintained |
| openapi-typescript | 7.13.0 | |
| eslint / eslint-plugin-vue | 10.15.0 / 10.12.1 | |
| @vue/eslint-config-typescript / @vue/eslint-config-prettier | 14.9.0 / 10.2.0 | |
| prettier / prettier-plugin-tailwindcss | 3.10.1 / 0.8.1 | |
| jiti | 2.7.0 | loads `eslint.config.ts` |
| openvue / @openvue/core | 1.1.1 / 1.1.1 | S0-6; `@openuxkit/*` 1.0.x–1.1.0 pinned by lockfile; bumps need a reviewed package diff (ADR-001) |

Considered and not adopted: `Microsoft.NET.Test.Sdk` 18.12.0 and `xunit.runner.visualstudio` 4.0.1, which only VSTest needs (see ADR-003).
