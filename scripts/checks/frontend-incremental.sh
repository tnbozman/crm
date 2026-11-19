#!/usr/bin/env bash
# dotnet build must not rebuild the frontend when its inputs are unchanged, and must when one changes.
# Run via: make verify
set -euo pipefail

cd "$(dirname "$0")/../.."

LOG=$(mktemp)
trap 'rm -f "$LOG"' EXIT
build() { dotnet build src/Crm.Host -c Debug -v:n >"$LOG" 2>&1; }

build # bring everything up to date

build
if ! grep -q 'Skipping target "BuildFrontend" because all output files are up-to-date' "$LOG"; then
    echo "FAIL: second dotnet build re-ran BuildFrontend with unchanged inputs" >&2
    exit 1
fi

touch src/web/src/App.vue
build
if grep -q 'Skipping target "BuildFrontend"' "$LOG" || ! grep -qE '^\s+([0-9]+>)?BuildFrontend:' "$LOG"; then
    echo "FAIL: dotnet build did not re-run BuildFrontend after src/web/src/App.vue changed" >&2
    exit 1
fi

# A content change gives the bundle new hashed file names. The build must not fail on the old ones
# that MSBuild saw in wwwroot at evaluation time (static web assets).
VIEW=src/web/src/views/HomeView.vue
BACKUP=$(mktemp)
cp "$VIEW" "$BACKUP"
trap 'cp "$BACKUP" "$VIEW"; rm -f "$LOG" "$BACKUP"' EXIT
sed -i 's|<h1>CRM</h1>|<h1>CRM (incremental check)</h1>|' "$VIEW"
if ! build; then
    echo "FAIL: dotnet build failed after a frontend change that renames the hashed bundle:" >&2
    grep -E "error" "$LOG" | sed 's| \[.*||' | sort -u | head -5 >&2
    exit 1
fi
cp "$BACKUP" "$VIEW"
if ! build; then
    echo "FAIL: dotnet build failed after reverting the frontend change" >&2
    exit 1
fi

echo "frontend-incremental: BuildFrontend is skipped when unchanged, re-runs when an input changes, and survives renamed bundles."
