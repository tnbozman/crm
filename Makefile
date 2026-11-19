.DEFAULT_GOAL := help
SHELL := /bin/bash
SLN := Crm.slnx
WEB := src/web
HOST := src/Crm.Host

# dotnet test runs on Microsoft.Testing.Platform (ADR-003). Exit code 8 means "zero tests ran"
# (a filter matched nothing in a project); real failures still exit non-zero.
TEST := dotnet test --solution $(SLN) -p:SkipFrontend=true --ignore-exit-code 8

# Stub for work owned by a later story: $(call todo,<story>,<message>). No commas in the message.
# Prints to stderr and exits 0 so verify and post-create keep working until the story lands.
todo = @echo "TODO($(1)): $(2)" >&2

.PHONY: help doctor hooks build dev \
        test test-unit test-int test-arch test-web \
        lint format format-check api-client api-client-check \
        db-reset migration-add module-new verify

help:            ## list targets
	@grep -E '^[a-zA-Z_-]+:.*?## ' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "%-18s %s\n", $$1, $$2}'

doctor:          ## check environment is ready (tools, SQL Server, VS Code config)
	@bash scripts/checks/doctor.sh

hooks:           ## install git hooks
	git config core.hooksPath .githooks

build:           ## backend build; also builds the frontend into Crm.Host/wwwroot
	dotnet build $(SLN) -c Debug

dev:             ## backend watch + Vite dev server with proxy
	$(call todo,S0-8,make dev runs dotnet watch and the Vite dev server)

test-unit:       ## tests that need no database
	$(TEST) --filter-not-trait "Category=Integration" --filter-not-trait "Category=Architecture"

test-int:        ## tests needing SQL Server
	$(TEST) --filter-trait "Category=Integration"

test-arch:       ## architecture tests
	$(TEST) --filter-trait "Category=Architecture"

test-web:        ## Vitest
	$(call todo,S0-5,make test-web runs Vitest once the Vue app exists)

test: test-unit test-int test-arch test-web   ## everything except e2e

lint: format-check   ## C# format check, ESLint, Prettier check, vue-tsc
	$(call todo,S0-5,ESLint and vue-tsc join make lint with the Vue app)

format:          ## apply formatting
	dotnet format $(SLN)
	$(call todo,S0-5,Prettier joins make format with the Vue app)

format-check:    ## fail if formatting would change files
	dotnet format $(SLN) --verify-no-changes
	$(call todo,S0-5,Prettier check joins make format-check with the Vue app)

api-client:      ## regenerate TypeScript API types from OpenAPI
	$(call todo,S0-4,make api-client emits the OpenAPI document; S0-5 generates the TypeScript client)

api-client-check: api-client   ## fail if generated client differs from committed
	$(call todo,S0-5,make api-client-check diffs the generated client in $(WEB)/src/api)

db-reset:        ## drop, recreate and migrate the dev database
	$(call todo,S1-2,make db-reset needs Crm.Migrator to know the module list)

migration-add:   ## make migration-add module=Directory name=AddOrganisation
	$(call todo,S1-2,make migration-add needs the first module DbContext)

module-new:      ## scaffold a module: make module-new name=Directory
	$(call todo,S1-2,make module-new runs scripts/new-module.sh)

verify: lint build test api-client-check   ## what CI runs
