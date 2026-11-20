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
trap 'rm -rf "$WORK"' EXIT

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

echo ""
if [[ $ERRORS -eq 0 ]]; then
    echo "new-module: all checks passed."
else
    echo "new-module: $ERRORS check(s) failed." >&2
    exit 1
fi
