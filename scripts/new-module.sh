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

LOWER=$(tr '[:upper:]' '[:lower:]' <<<"$NAME")
TEMPLATES=scripts/templates/module
MARKER="make module-new adds modules above this line"

echo "module-new: generating $NAME (schema $LOWER)"
while IFS= read -r -d '' template; do
    relative=${template#"$TEMPLATES/"}
    target=${relative//__Module__/$NAME}
    target=${target//__module__/$LOWER}
    mkdir -p "$(dirname "$target")"
    sed -e "s/__Module__/$NAME/g" -e "s/__module__/$LOWER/g" "$template" >"$target"
done < <(find "$TEMPLATES" -type f -print0 | sort -z)

PROJECT="src/Modules/Crm.$NAME/Crm.$NAME.csproj"
TESTS="tests/Modules/$NAME/Crm.$NAME.Tests/Crm.$NAME.Tests.csproj"

echo "module-new: adding the module to Crm.slnx, Crm.Host and Crm.Migrator"
dotnet sln Crm.slnx add --solution-folder src/Modules "$PROJECT" >/dev/null
dotnet sln Crm.slnx add --solution-folder tests/Modules "$TESTS" >/dev/null

# The host composes every module; the Migrator applies every module's migrations (ADR-013).
for list in src/Crm.Host/HostModules.cs src/Crm.Migrator/MigratorModules.cs; do
    grep -q "$MARKER" "$list" || die "$list has lost its '$MARKER' line"
    dotnet reference add --project "$(dirname "$list")/$(basename "$(dirname "$list")").csproj" "$PROJECT" >/dev/null
    sed -i "s|^\(\s*\)// $MARKER|\1new Crm.$NAME.${NAME}Module(),\n&|" "$list"
done

echo "module-new: generating the initial migration"
bash scripts/migration-add.sh "$NAME" Initial

echo "module-new: done. Next: make build-backend && make test-arch, then replace the example Item with the module's own model."
