#!/usr/bin/env bash
# Scaffolds a module (ADR-013): make module-new name=<Module>
# Generates from scripts/templates/module/, replacing __Module__ (PascalCase) and __module__ (schema, lower case)
# in paths and contents, then wires the module into the solution, the host and the Migrator.
set -euo pipefail

cd "$(dirname "$0")/.."

die() { echo "module-new: $*" >&2; exit 1; }

NAME=${1:-}
[[ -n "$NAME" ]] || die "usage: make module-new name=<Module> (PascalCase, for example Directory)"
[[ "$NAME" =~ ^[A-Z][a-z0-9]+([A-Z][a-z0-9]+)*$ ]] || die "'$NAME' is not a PascalCase name (letters and digits, starting with a capital)"
case "$NAME" in
    Host | Mediator | SharedKernel | Testing | Migrator | Modules | Architecture | Fixture)
        die "'$NAME' is a platform project name, not a module" ;;
esac
[[ ! -e "src/Modules/Crm.$NAME" ]] || die "src/Modules/Crm.$NAME already exists"
[[ ! -e "src/Crm.Mediator/Contracts/$NAME" ]] || die "src/Crm.Mediator/Contracts/$NAME already exists"
