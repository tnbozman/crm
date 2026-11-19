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

echo "frontend-incremental: BuildFrontend is skipped when unchanged and re-runs when an input changes."
