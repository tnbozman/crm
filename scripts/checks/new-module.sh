#!/usr/bin/env bash
# S1-4: make module-new scaffolds a compliant module (ADR-013). Runs in a temporary copy of the working tree,
# so the repository is never changed. Run via: make check-module-new (part of make verify).
set -euo pipefail

cd "$(dirname "$0")/../.."
ROOT=$PWD

ERRORS=0
fail() { echo "FAIL: $*" >&2; ERRORS=$((ERRORS + 1)); }
step() { echo "" && echo "== new-module: $*"; }

WORK=$(mktemp -d)
CHECK_DB="crm_module_check_$$"

# The dev SQL Server from the devcontainer and CI (CRM_SQL_*); the password goes through the environment, not argv.
sql() {
    SQLCMDPASSWORD="${CRM_SQL_PASSWORD:-}" sqlcmd -C -S "${CRM_SQL_SERVER:-}" -U "${CRM_SQL_USER:-sa}" -b -h -1 -W -Q "SET NOCOUNT ON; $1"
}
cleanup() {
    sql "IF DB_ID(N'$CHECK_DB') IS NOT NULL BEGIN ALTER DATABASE [$CHECK_DB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$CHECK_DB]; END" \
        >/dev/null 2>&1 || true
    rm -rf "$WORK"
}
trap cleanup EXIT

# Tracked and untracked (not ignored) files, so uncommitted template changes are tested too.
git ls-files -co --exclude-standard \
    | while IFS= read -r f; do [[ -e "$f" ]] && printf '%s\0' "$f"; done \
    | tar --null -T - -cf - | tar -xf - -C "$WORK"
cd "$WORK"

MAKE=(make --no-print-directory)

tree_digest() { find . -path ./node_modules -prune -o -type f -print0 | sort -z | xargs -0 sha1sum | sha1sum; }

step "rejects names that are not PascalCase or already taken"
before=$(tree_digest)
for bad in "" zzz "Zz-z" "1Zzz" "Host" "Mediator" "SharedKernel"; do
    if "${MAKE[@]}" module-new name="$bad" >/dev/null 2>&1; then
        fail "make module-new name='$bad' succeeded; it must be refused"
    fi
done
[[ "$(tree_digest)" == "$before" ]] || fail "a refused name changed files"

step "scaffolds Zzz"
"${MAKE[@]}" module-new name=Zzz || { echo "FAIL: make module-new name=Zzz failed" >&2; exit 1; }

expect_file() { [[ -f "$1" ]] || fail "missing $1"; }
expect_dir() { [[ -d "$1" ]] || fail "missing folder $1"; }

M=src/Modules/Crm.Zzz
expect_file "$M/Crm.Zzz.csproj"
expect_file "$M/ZzzModule.cs"
for folder in Controllers Services Dtos Models Persistence Mapping Validation Handlers Skills; do
    expect_dir "$M/$folder"
done
expect_file "$M/Persistence/ZzzDbContext.cs"
compgen -G "$M/Persistence/Migrations/*_Initial.cs" >/dev/null || fail "missing the initial migration in $M/Persistence/Migrations"
expect_dir src/Crm.Mediator/Contracts/Zzz
expect_file tests/Modules/Zzz/Crm.Zzz.Tests/Crm.Zzz.Tests.csproj
expect_dir src/web/src/features/zzz

grep -q 'src/Modules/Crm.Zzz/Crm.Zzz.csproj' Crm.slnx || fail "Crm.slnx does not include the module"
grep -q 'tests/Modules/Zzz/Crm.Zzz.Tests/Crm.Zzz.Tests.csproj' Crm.slnx || fail "Crm.slnx does not include the module's tests"

step "a second scaffold of the same name is refused"
if "${MAKE[@]}" module-new name=Zzz >/dev/null 2>&1; then
    fail "make module-new name=Zzz succeeded twice"
fi

step "the outbox drift check passes on the generated module and fails on a planted change"
bash scripts/checks/module-outbox-drift.sh || fail "module-outbox-drift.sh fails on a freshly generated module"
RELAY=src/Modules/Crm.Zzz/Persistence/Messaging/ZzzOutboxRelay.cs
cp "$RELAY" "$WORK/relay.orig"
sed -i 's/public const int MaxAttempts = 10;/public const int MaxAttempts = 11;/' "$RELAY"
if bash scripts/checks/module-outbox-drift.sh >/dev/null 2>&1; then
    fail "module-outbox-drift.sh passed with a planted change to $RELAY"
fi
cp "$WORK/relay.orig" "$RELAY"
echo "// extra" >src/Modules/Crm.Zzz/Persistence/Messaging/Extra.cs
if bash scripts/checks/module-outbox-drift.sh >/dev/null 2>&1; then
    fail "module-outbox-drift.sh passed with a file added to the module's Messaging folder"
fi
rm src/Modules/Crm.Zzz/Persistence/Messaging/Extra.cs
rm "$WORK/relay.orig"

step "the solution builds and the architecture tests pass"
dotnet build Crm.slnx -c Debug -p:SkipFrontend=true -nologo -v quiet || fail "dotnet build failed"
"${MAKE[@]}" test-arch || fail "make test-arch failed"

step "the generated code is formatted (make lint)"
dotnet format Crm.slnx --verify-no-changes --include src/Modules/Crm.Zzz/ tests/Modules/Zzz/ src/Crm.Mediator/Contracts/Zzz/ \
    || fail "generated C# is not formatted; fix the templates in scripts/templates/module"
(cd "$ROOT/src/web" && node_modules/.bin/prettier --check "$WORK/src/web/src/features/zzz") \
    || fail "the generated web feature folder is not formatted"

step "the generated module's own tests pass (unit, and integration against SQL Server)"
dotnet test --project tests/Modules/Zzz/Crm.Zzz.Tests -p:SkipFrontend=true || fail "the generated Crm.Zzz.Tests failed"

step "make db-reset and make migrate apply the module's migrations through the Migrator"
[[ -n "${CRM_SQL_SERVER:-}" && -n "${CRM_SQL_PASSWORD:-}" ]] || fail "CRM_SQL_SERVER and CRM_SQL_PASSWORD are not set (the devcontainer and CI set them)"
reset_output=$("${MAKE[@]}" db-reset DEV_DB="$CHECK_DB" 2>&1) || fail "make db-reset failed"
echo "$reset_output"
grep -q "migrated Zzz" <<<"$reset_output" || fail "make db-reset did not migrate Zzz"
[[ "$(sql "SELECT COUNT(*) FROM [$CHECK_DB].sys.tables t JOIN [$CHECK_DB].sys.schemas s ON s.schema_id = t.schema_id WHERE s.name = N'zzz' AND t.name = N'Items'")" == 1 ]] \
    || fail "zzz.Items does not exist after make db-reset"
"${MAKE[@]}" migrate DEV_DB="$CHECK_DB" || fail "make migrate on an up-to-date database failed"

echo ""
if [[ $ERRORS -eq 0 ]]; then
    echo "new-module: all checks passed."
else
    echo "new-module: $ERRORS check(s) failed." >&2
    exit 1
fi
