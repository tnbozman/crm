#!/usr/bin/env bash
# make sbom must produce CycloneDX SBOMs for what ships: the backend (Crm.Host and its references) and the
# frontend's production dependencies. Run via: make check-sbom (CI uploads the files as an artifact).
set -euo pipefail

cd "$(dirname "$0")/../.."

rm -rf artifacts/sbom
make --no-print-directory sbom >/dev/null

ERRORS=0
fail() { echo "FAIL: $*" >&2; ERRORS=$((ERRORS + 1)); }

check() {
    local file="artifacts/sbom/$1"; shift
    if [[ ! -s "$file" ]]; then
        fail "$file missing or empty"
        return
    fi
    [[ "$(jq -r '.bomFormat // empty' "$file")" == "CycloneDX" ]] || fail "$file is not a CycloneDX document"
    for component in "$@"; do
        jq -e --arg n "$component" '[.components[]? | .name] | index($n)' "$file" >/dev/null \
            || fail "$file does not list component '$component'"
    done
    echo "$file: $(jq '.components | length' "$file") components, spec $(jq -r '.specVersion' "$file")"
}

check backend.cdx.json Microsoft.AspNetCore.OpenApi
check frontend.cdx.json vue openvue

if [[ $ERRORS -eq 0 ]]; then
    echo "sbom: CycloneDX SBOMs produced."
else
    exit 1
fi
