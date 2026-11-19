.DEFAULT_GOAL := help
SHELL := /bin/bash
SLN := Crm.slnx
WEB := src/web
HOST := src/Crm.Host
OPENAPI := openapi/Crm.Host.json

# dotnet test runs on Microsoft.Testing.Platform (ADR-003). Exit code 8 means "zero tests ran"
# (a filter matched nothing in a project); real failures still exit non-zero.
TEST := dotnet test --solution $(SLN) -p:SkipFrontend=true --ignore-exit-code 8

# Stub for work owned by a later story: $(call todo,<story>,<message>). No commas in the message.
# Prints to stderr and exits 0 so verify and post-create keep working until the story lands.
todo = @echo "TODO($(1)): $(2)" >&2

.PHONY: help doctor setup hooks build build-backend build-web run dev dev-web \
        test test-unit test-int test-arch test-web test-e2e \
        lint format format-check api-client api-client-check \
        db-up db-reset migrate migration-add module-new \
        publish check-dev check-spa audit sbom check-sbom verify ai-smoke coverage clean

help:            ## list targets
	@grep -E '^[a-zA-Z0-9_-]+:.*?## ' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "%-18s %s\n", $$1, $$2}'

doctor:          ## check environment is ready (tools, SQL Server, VS Code config)
	@bash scripts/checks/doctor.sh

setup:           ## restore backend and frontend dependencies
	dotnet tool restore && dotnet restore $(SLN)
	pnpm --dir $(WEB) install --frozen-lockfile

hooks:           ## install git hooks
	git config core.hooksPath .githooks

build:           ## backend build; also builds the frontend into Crm.Host/wwwroot
	dotnet build $(SLN) -c Debug

build-backend:   ## backend only (skip frontend build)
	dotnet build $(SLN) -c Debug -p:SkipFrontend=true

build-web:       ## frontend only
	pnpm --dir $(WEB) build

run:             ## run the host serving the built frontend (https://localhost:5001)
	dotnet run --project $(HOST) --launch-profile https

dev:             ## backend watch (https://localhost:5001) + Vite dev server with /api proxy (http://localhost:5173)
	@# dotnet watch reads -p as --project, so SkipFrontend goes in as an environment variable (an MSBuild property).
	@# If either process exits, wait -n returns and the EXIT trap stops the other.
	@trap 'kill 0' INT TERM EXIT; \
	SkipFrontend=true dotnet watch --project $(HOST) --launch-profile https & \
	pnpm --dir $(WEB) dev & \
	wait -n

dev-web:         ## Vite dev server only (http://localhost:5173; the debugger runs the host)
	pnpm --dir $(WEB) dev

test-unit:       ## tests that need no database
	$(TEST) --filter-not-trait "Category=Integration" --filter-not-trait "Category=Architecture"

test-int:        ## tests needing SQL Server
	$(TEST) --filter-trait "Category=Integration"

test-arch:       ## architecture tests
	$(TEST) --filter-trait "Category=Architecture"

test-web:        ## Vitest
	pnpm --dir $(WEB) test --run

test-e2e:        ## Playwright journeys against the built host
	$(call todo,S1-7,make test-e2e runs the first Playwright journeys)

test: test-unit test-int test-arch test-web   ## everything except e2e

lint: format-check   ## C# format check, ESLint, Prettier check, vue-tsc
	@bash scripts/checks/make-targets.sh
	@bash scripts/checks/vscode-config.sh
	@bash scripts/checks/ci-config.sh
	@bash scripts/checks/devcontainer-config.sh
	pnpm --dir $(WEB) lint
	pnpm --dir $(WEB) typecheck

format:          ## apply formatting
	dotnet format $(SLN)
	pnpm --dir $(WEB) format

format-check:    ## fail if formatting would change files
	dotnet format $(SLN) --verify-no-changes
	pnpm --dir $(WEB) format:check

api-client:      ## regenerate the OpenAPI document and TypeScript API types
	dotnet build $(HOST) -p:SkipFrontend=true -p:GenerateOpenApi=true
	@jq -e '.paths["/api/v1/health"]' $(OPENAPI) >/dev/null || { echo "api-client: $(OPENAPI) missing or does not describe /api/v1/health" >&2; exit 1; }
	pnpm --dir $(WEB) api:generate

api-client-check: api-client   ## fail if generated client differs from committed
	@git diff --exit-code -- $(OPENAPI) || { echo "api-client-check: $(OPENAPI) is stale; run make api-client and commit it" >&2; exit 1; }
	@git diff --exit-code -- $(WEB)/src/api || { echo "api-client-check: $(WEB)/src/api is stale; run make api-client and commit it" >&2; exit 1; }

db-up:           ## start the dev database service (run on the Docker host)
	@command -v docker >/dev/null || { echo "db-up: docker is not available here. In the devcontainer the db service is started by docker-compose; check it with make doctor." >&2; exit 1; }
	docker compose -f .devcontainer/docker-compose.yml up -d db

db-reset:        ## drop, recreate and migrate the dev database
	$(call todo,S1-2,make db-reset needs Crm.Migrator to know the module list)

migrate:         ## apply pending migrations
	$(call todo,S1-2,make migrate needs Crm.Migrator to know the module list)

migration-add:   ## make migration-add module=Directory name=AddOrganisation
	$(call todo,S1-2,make migration-add needs the first module DbContext)

module-new:      ## scaffold a module: make module-new name=Directory
	$(call todo,S1-2,make module-new runs scripts/new-module.sh)

publish:         ## publish the host (includes wwwroot)
	dotnet publish $(HOST) -c Release -o artifacts/publish

check-dev:       ## smoke test make dev (needs ports 5001 and 5173 free; not part of verify)
	@bash scripts/checks/dev-smoke.sh

check-spa:       ## frontend build checks: incremental build, clean publish contains the SPA
	@bash scripts/checks/frontend-incremental.sh
	@bash scripts/checks/publish-contains-spa.sh

audit:           ## fail on High/Critical vulnerabilities (NuGet incl. transitive, npm)
	@bash scripts/checks/dependency-audit.sh

sbom:            ## CycloneDX SBOMs of what ships: artifacts/sbom/{backend,frontend}.cdx.json
	mkdir -p artifacts/sbom
	dotnet tool run dotnet-CycloneDX $(HOST)/Crm.Host.csproj -o artifacts/sbom -fn backend.cdx.json -F Json
	pnpm --dir $(WEB) sbom --sbom-format cyclonedx --prod > artifacts/sbom/frontend.cdx.json

check-sbom:      ## produce the SBOMs and check they are valid CycloneDX listing known components
	@bash scripts/checks/sbom.sh

verify: lint build test api-client-check audit check-spa   ## what CI runs

ai-smoke:        ## calls each configured AI provider once (skipped without keys)
	$(call todo,S2-7,make ai-smoke calls each configured AI provider)

coverage:        ## coverage report (artifacts/coverage/report/index.html)
	rm -rf artifacts/coverage
	$(TEST) --coverage --coverage-output-format cobertura --results-directory $(CURDIR)/artifacts/coverage
	dotnet reportgenerator -reports:"artifacts/coverage/**/*.cobertura.xml" -targetdir:artifacts/coverage/report -reporttypes:"Html;TextSummary"
	@cat artifacts/coverage/report/Summary.txt

clean:           ## remove build output
	dotnet clean $(SLN) && rm -rf artifacts $(HOST)/wwwroot $(WEB)/dist
