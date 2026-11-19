# Environment specification

Reference files for Sprint 0. Claude creates these in the repo, makes them work, and then treats them as the source of truth. Anything marked **verify** must be confirmed against current docs in S0 (do not trust version numbers written here).

## 1. Repository root files

```
.devcontainer/devcontainer.json
.devcontainer/Dockerfile
.devcontainer/docker-compose.yml
.devcontainer/post-create.sh
.vscode/{extensions,settings,launch,tasks}.json
.config/dotnet-tools.json          dotnet-ef, reportgenerator
.editorconfig
.gitattributes
.gitignore
global.json                        pin .NET 10 SDK, rollForward latestFeature
nuget.config
Directory.Build.props              nullable, warnings as errors, analyzers, deterministic
Directory.Packages.props           central package management
Crm.slnx
Makefile
```

## 2. Devcontainer

The container is deliberately "fat": everything is baked in so a fresh clone is productive offline after the first build.

`.devcontainer/devcontainer.json`

```json
{
  "name": "crm",
  "dockerComposeFile": "docker-compose.yml",
  "service": "app",
  "workspaceFolder": "/workspaces/${localWorkspaceFolderBasename}",
  "features": {
    "ghcr.io/devcontainers/features/node:1": { "version": "22" },
    "ghcr.io/devcontainers/features/github-cli:1": {},
    "ghcr.io/devcontainers/features/azure-cli:1": {}
  },
  "forwardPorts": [5000, 5001, 5173, 1433, 8025],
  "remoteEnv": {
    "CRM_SQL_SERVER": "db",
    "ANTHROPIC_API_KEY": "${localEnv:ANTHROPIC_API_KEY}",
    "OPENAI_API_KEY": "${localEnv:OPENAI_API_KEY}"
  },
  "postCreateCommand": "bash .devcontainer/post-create.sh",
  "customizations": {
    "vscode": {
      "extensions": [
        "ms-dotnettools.csdevkit",
        "ms-dotnettools.csharp",
        "ms-mssql.mssql",
        "Vue.volar",
        "dbaeumer.vscode-eslint",
        "esbenp.prettier-vscode",
        "bradlc.vscode-tailwindcss",
        "vitest.explorer",
        "ms-playwright.playwright",
        "editorconfig.editorconfig",
        "ms-vscode.makefile-tools",
        "ms-azuretools.vscode-bicep",
        "redhat.vscode-yaml",
        "GitHub.vscode-github-actions",
        "Anthropic.claude-code"
      ]
    }
  }
}
```

**verify**: the Claude Code extension ID, and that the `node` feature installs corepack so `pnpm` works.

`.devcontainer/Dockerfile`

```dockerfile
FROM mcr.microsoft.com/devcontainers/dotnet:10.0

# OS tools: make, sqlcmd (pick a working install method in S0), browsers for Playwright, fonts
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get install -y --no-install-recommends make jq unzip \
    && rm -rf /var/lib/apt/lists/*

# Warm the NuGet cache so the first restore is offline-fast.
# S0: copy Directory.Packages.props and a warm-up project that references every package the plan needs
# (EF Core, SQL Server provider, Identity, Microsoft.Identity.Web, Microsoft.Extensions.AI,
# Microsoft.Agents.AI, Microsoft.Agents.AI.OpenAI, Microsoft.Agents.AI.Anthropic, Anthropic,
# xunit.v3, Shouldly, NSubstitute, Respawn, NetArchTest.Rules, Bogus, Mvc.Testing).
# Playwright: install Chromium with OS dependencies after the frontend is scaffolded.
```

**verify**: image tag `mcr.microsoft.com/devcontainers/dotnet:10.0` exists (listed in the devcontainers image README); package list matches what S0 chooses.

`.devcontainer/docker-compose.yml`

```yaml
services:
  app:
    build:
      context: ..
      dockerfile: .devcontainer/Dockerfile
    volumes:
      - ..:/workspaces/crm:cached
    command: sleep infinity
    depends_on:
      db:
        condition: service_healthy
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "Dev_Only_Passw0rd!"   # development container only
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P \"$$MSSQL_SA_PASSWORD\" -Q 'SELECT 1' || exit 1"]
      interval: 10s
      timeout: 5s
      retries: 10
volumes:
  sqldata:
```

Notes: the `workspaces/crm` path must match `workspaceFolder`; the SA password is for the local container only and never used in Azure; S11 adds a `mailpit` service (ports 1025, 8025). **verify**: the sqlcmd path in the health check for the chosen image.

`.devcontainer/post-create.sh` must: `dotnet tool restore`, `corepack enable`, `pnpm install` in `src/web`, `make hooks`, wait for the database, run `make db-reset`, print `make help`. It must be idempotent.

## 3. Makefile (spec)

Recipes use tab indentation. Names are fixed; internals can change.

> **Note (S0-2, ADR-003):** `dotnet test` runs on Microsoft.Testing.Platform. The VSTest `--filter` and `--collect` arguments below must become `--filter-trait`/`--filter-not-trait` and an MTP coverage extension when S0-3 implements these targets.

```make
.DEFAULT_GOAL := help
SHELL := /bin/bash
SLN := Crm.slnx
WEB := src/web
HOST := src/Crm.Host

.PHONY: help setup hooks build build-backend build-web run dev \
        test test-unit test-int test-arch test-web test-e2e \
        lint format format-check api-client api-client-check \
        db-up db-reset migrate migration-add module-new \
        publish verify ai-smoke coverage clean

help:            ## list targets
	@grep -E '^[a-zA-Z_-]+:.*?## ' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "%-18s %s\n", $$1, $$2}'

setup:           ## restore backend and frontend dependencies
	dotnet tool restore && dotnet restore $(SLN) && pnpm --dir $(WEB) install --frozen-lockfile

hooks:           ## install git hooks that run make verify-fast
	git config core.hooksPath .githooks

build:           ## backend build; also builds the frontend into Crm.Host/wwwroot
	dotnet build $(SLN) -c Debug

build-backend:   ## backend only (skip frontend build)
	dotnet build $(SLN) -c Debug -p:SkipFrontend=true

build-web:       ## frontend only
	pnpm --dir $(WEB) build

run:             ## run the host serving the built frontend (https://localhost:5001)
	dotnet run --project $(HOST) --launch-profile https

dev:             ## backend watch + Vite dev server with proxy
	@trap 'kill 0' INT TERM; \
	dotnet watch --project $(HOST) --launch-profile https -p:SkipFrontend=true & \
	pnpm --dir $(WEB) dev & \
	wait

test-unit:       ## tests that need no database
	dotnet test $(SLN) -p:SkipFrontend=true --filter "Category!=Integration&Category!=Architecture"

test-int:        ## tests needing SQL Server
	dotnet test $(SLN) -p:SkipFrontend=true --filter "Category=Integration"

test-arch:       ## architecture tests
	dotnet test tests/Crm.Architecture.Tests -p:SkipFrontend=true

test-web:        ## Vitest
	pnpm --dir $(WEB) test --run

test-e2e:        ## Playwright journeys against the built host
	pnpm --dir $(WEB) test:e2e

test: test-unit test-int test-arch test-web   ## everything except e2e

lint:            ## C# format check, ESLint, Prettier check, vue-tsc
	dotnet format $(SLN) --verify-no-changes
	pnpm --dir $(WEB) lint
	pnpm --dir $(WEB) format:check
	pnpm --dir $(WEB) typecheck

format:          ## apply formatting
	dotnet format $(SLN)
	pnpm --dir $(WEB) format

api-client:      ## regenerate TypeScript API types from OpenAPI
	dotnet build $(HOST) -p:SkipFrontend=true -p:GenerateOpenApi=true
	pnpm --dir $(WEB) api:generate

api-client-check: api-client   ## fail if generated client differs from committed
	git diff --exit-code -- $(WEB)/src/api

db-up:           ## start the dev database service
	docker compose -f .devcontainer/docker-compose.yml up -d db

db-reset:        ## drop, recreate and migrate the dev database
	dotnet run --project src/Crm.Migrator -- --reset --environment Development

migrate:         ## apply pending migrations
	dotnet run --project src/Crm.Migrator -- --environment Development

migration-add:   ## make migration-add module=Directory name=AddOrganisation
	@test -n "$(module)" -a -n "$(name)" || (echo "usage: make migration-add module=X name=Y" && exit 1)
	dotnet ef migrations add $(name) --context $(module)DbContext \
	  --project src/Modules/$(module)/Crm.$(module) --startup-project src/Crm.Migrator \
	  --output-dir Persistence/Migrations

module-new:     ## scaffold a module: make module-new name=Directory
	@test -n "$(name)" || (echo "usage: make module-new name=X" && exit 1)
	./scripts/new-module.sh $(name)

publish:         ## publish the host (includes wwwroot)
	dotnet publish $(HOST) -c Release -o artifacts/publish

verify: lint build test api-client-check   ## what CI runs

ai-smoke:        ## calls each configured AI provider once (skipped without keys)
	dotnet test tests/Crm.Host.Tests --filter "Category=AiSmoke"

coverage:        ## coverage report
	dotnet test $(SLN) -p:SkipFrontend=true --collect:"XPlat Code Coverage"

clean:
	dotnet clean $(SLN) && rm -rf artifacts $(HOST)/wwwroot $(WEB)/dist
```

## 4. Frontend built by the .NET build

Goal: `dotnet build` produces `src/Crm.Host/wwwroot` from `src/web`, and `dotnet publish` includes it.

`src/Crm.Host/Crm.Host.csproj` (fragment)

```xml
<PropertyGroup>
  <FrontendDir>$(MSBuildThisFileDirectory)..\web\</FrontendDir>
  <SkipFrontend Condition="'$(SkipFrontend)' == ''">false</SkipFrontend>
</PropertyGroup>

<ItemGroup>
  <FrontendInputs Include="$(FrontendDir)src\**;$(FrontendDir)index.html;$(FrontendDir)package.json;$(FrontendDir)pnpm-lock.yaml;$(FrontendDir)vite.config.ts;$(FrontendDir)tsconfig*.json" />
</ItemGroup>

<Target Name="BuildFrontend" BeforeTargets="BeforeBuild"
        Condition="'$(SkipFrontend)' != 'true'"
        Inputs="@(FrontendInputs)" Outputs="$(MSBuildProjectDirectory)\wwwroot\index.html">
  <Exec Command="pnpm install --frozen-lockfile" WorkingDirectory="$(FrontendDir)" />
  <Exec Command="pnpm run build" WorkingDirectory="$(FrontendDir)" />
</Target>

<Target Name="IncludeFrontendInPublish" AfterTargets="ComputeFilesToPublish"
        Condition="'$(SkipFrontend)' != 'true'">
  <ItemGroup>
    <FrontendDist Include="$(MSBuildProjectDirectory)\wwwroot\**" />
    <ResolvedFileToPublish Include="@(FrontendDist)"
        RelativePath="wwwroot\%(RecursiveDir)%(Filename)%(Extension)"
        CopyToPublishDirectory="PreserveNewest" ExcludeFromSingleFile="true" />
  </ItemGroup>
</Target>
```

`src/web/vite.config.ts` must set `build.outDir` to `../Crm.Host/wwwroot` with `emptyOutDir: true`, and in dev proxy `/api` and `/signin-oidc`-style paths to `https://localhost:5001`.

Host wiring in `Program.cs`:

```csharp
app.UseDefaultFiles();
app.UseStaticFiles();                 // serves files generated by the frontend build
app.MapModules();                     // /api/v1/*
app.MapFallbackToFile("index.html");  // SPA routes
```

**Pitfall to test, not assume (verify in S0)**: files created during the build may not be in the static-web-asset manifest that `MapStaticAssets` uses, because item globs are evaluated before the target runs. Use `UseStaticFiles` (reads `wwwroot` from disk) and keep the tests below.

Required tests (S0): after `make build`, `GET /` returns the SPA `index.html`; `GET /some/client/route` returns `index.html`; `GET /api/v1/unknown` returns 404 problem details (not HTML); after `make publish`, `artifacts/publish/wwwroot/index.html` exists.

## 5. VS Code

`.vscode/extensions.json` recommends the same extensions as the devcontainer.

`.vscode/settings.json`

```json
{
  "dotnet.defaultSolution": "Crm.slnx",
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": { "source.fixAll.eslint": "explicit" },
  "[typescript]": { "editor.defaultFormatter": "esbenp.prettier-vscode" },
  "[vue]": { "editor.defaultFormatter": "esbenp.prettier-vscode" },
  "[json]": { "editor.defaultFormatter": "esbenp.prettier-vscode" },
  "[jsonc]": { "editor.defaultFormatter": "esbenp.prettier-vscode" },
  "[markdown]": { "editor.defaultFormatter": "esbenp.prettier-vscode" },
  "eslint.useFlatConfig": true,
  "eslint.workingDirectories": [{ "directory": "src/web" }],
  "vitest.root": "src/web",
  "tailwindCSS.experimental.classRegex": [["class:\\s*['\"`]([^'\"`]*)['\"`]"]],
  "files.exclude": { "**/bin": true, "**/obj": true },
  "mssql.connections": [
    {
      "server": "db",
      "profileName": "crm-dev",
      "authenticationType": "SqlLogin",
      "user": "sa",
      "trustServerCertificate": true,
      "savePassword": false
    }
  ]
}
```

`.vscode/launch.json` — configurations:

- `Host (.NET)`: `coreclr`, `launch`, program is the built `Crm.Host.dll`, `preLaunchTask: make build-backend`, `env: ASPNETCORE_ENVIRONMENT=Development`, `serverReadyAction` opens the URL.
- `Web (Vue, Chrome)`: `chrome` type, `url: http://localhost:5173`, `webRoot: ${workspaceFolder}/src/web`, `preLaunchTask: web dev`.
- Compound `Full stack` starts both.
- `Attach to Migrator` for migration debugging.

`.vscode/tasks.json` — tasks that call make: `build`, `test`, `test-web`, `lint`, `format`, and a background `web dev` task with a problem matcher so the debugger waits for Vite.

Test running: C# Dev Kit Test Explorer runs and debugs xUnit tests; Vitest Explorer runs the frontend tests; Playwright extension runs e2e. Integration tests appear in Test Explorer and need the `db` service running (it is part of the devcontainer).

## 6. Quality gates in the repo

- `Directory.Build.props`: `Nullable=enable`, `TreatWarningsAsErrors=true`, `AnalysisMode=Recommended`, `EnforceCodeStyleInBuild=true`.
- `.editorconfig` defines C# style; `dotnet format --verify-no-changes` enforces it.
- ESLint flat config (`typescript-eslint`, `eslint-plugin-vue`, `eslint-plugin-tailwindcss` if compatible), Prettier with `prettier-plugin-tailwindcss`.
- `.githooks/pre-commit` runs `make lint` and `make test-unit`; `.githooks/pre-push` runs `make verify`.
- CI (`.github/workflows/ci.yml`) runs `make verify` inside the devcontainer image or an equivalent job, with a SQL Server service container and `CRM_SQL_SERVER` set.

## 7. Secrets and AI keys

- Local: `dotnet user-secrets` for the host; `ANTHROPIC_API_KEY` and `OPENAI_API_KEY` come from the developer's shell through `remoteEnv`. Never committed.
- Azure: Key Vault references and managed identity only.
- AI package set to install (versions from the feed, not from this file): `Microsoft.Extensions.AI`, `Microsoft.Agents.AI`, `Microsoft.Agents.AI.OpenAI`, `Microsoft.Agents.AI.Anthropic`, the official `Anthropic` SDK (it implements `IChatClient`), and the Azure OpenAI client.
