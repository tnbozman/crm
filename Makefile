.DEFAULT_GOAL := help
SHELL := /bin/bash

# S0-3 adds the full target set; this file grows with each story.
.PHONY: help doctor hooks

help:            ## list targets
	@grep -E '^[a-zA-Z_-]+:.*?## ' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "%-18s %s\n", $$1, $$2}'

doctor:          ## check environment is ready (tools, SQL Server, VS Code config)
	@bash scripts/checks/doctor.sh

hooks:           ## install git hooks
	git config core.hooksPath .githooks
