#!/usr/bin/env bash
# Adds an EF Core migration to a module: make migration-add module=<Module> name=<Name>
# Migrations go in Persistence/Migrations and are made internal, like every module type (ADR-012).
set -euo pipefail

cd "$(dirname "$0")/.."

die() { echo "migration-add: $*" >&2; exit 1; }

MODULE=${1:-}
NAME=${2:-}
[[ -n "$MODULE" && -n "$NAME" ]] || die "usage: make migration-add module=<Module> name=<Name>"
[[ "$NAME" =~ ^[A-Z][A-Za-z0-9]*$ ]] || die "'$NAME' is not a PascalCase migration name"
PROJECT="src/Modules/Crm.$MODULE"
[[ -f "$PROJECT/Crm.$MODULE.csproj" ]] || die "no module project $PROJECT/Crm.$MODULE.csproj"

MIGRATIONS="$PROJECT/Persistence/Migrations"
dotnet restore "$PROJECT" -v quiet >/dev/null
dotnet tool run dotnet-ef migrations add "$NAME" \
    --project "$PROJECT" \
    --output-dir Persistence/Migrations

# dotnet-ef generates public migration classes; module types are internal (Module_Types_AreInternal_ExceptTheModuleClass).
# dotnet-ef also writes a byte order mark, which .editorconfig (charset = utf-8) and make lint reject.
sed -i -E -e '1s/^\xEF\xBB\xBF//' -e 's/^(\s*)public (partial class)/\1internal \2/' "$MIGRATIONS"/*.cs
