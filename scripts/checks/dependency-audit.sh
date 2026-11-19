#!/usr/bin/env bash
# Fails on High or Critical known vulnerabilities in NuGet (including transitive) or npm dependencies.
# Run via: make audit (part of make verify). Policy: CLAUDE.md, critical vulnerabilities fixed within 48 hours.
set -euo pipefail

cd "$(dirname "$0")/../.."

ERRORS=0

echo "== NuGet (dotnet list package --vulnerable --include-transitive)"
# NuGet Audit already turns advisories into restore errors (NU1901-NU1904, warnings as errors), so a vulnerable
# package fails restore before dotnet list can run. Report those errors and treat them as findings.
if ! RESTORE=$(dotnet restore Crm.slnx -p:SkipFrontend=true 2>&1); then
    if grep -qE 'NU190[1-4]' <<<"$RESTORE"; then
        grep -oE "Package '[^']+' [^ ]+ has a known [a-z]+ severity vulnerability, [^ ]+" <<<"$RESTORE" | sort -u
        echo "FAIL: NuGet Audit blocked restore" >&2
        ERRORS=$((ERRORS + 1))
    else
        echo "$RESTORE" >&2
        exit 1
    fi
fi
JSON=$([[ $ERRORS -eq 0 ]] && dotnet list Crm.slnx package --vulnerable --include-transitive --format json || echo '{}')
FINDINGS=$(jq -r '
  [.projects[]? | .path as $p | .frameworks[]? | (.topLevelPackages // []) + (.transitivePackages // [])
   | .[] | . as $pkg | .vulnerabilities[]?
   | "\($pkg.id) \($pkg.resolvedVersion) \(.severity) \(.advisoryurl)"] | unique | .[]' <<<"$JSON")
if [[ -n "$FINDINGS" ]]; then
    echo "$FINDINGS"
    if grep -qE ' (High|Critical) ' <<<"$FINDINGS"; then
        echo "FAIL: High or Critical NuGet vulnerabilities" >&2
        ERRORS=$((ERRORS + 1))
    fi
else
    echo "no known vulnerabilities"
fi

echo "== npm (pnpm audit, src/web)"
if ! pnpm --dir src/web audit --audit-level high; then
    echo "FAIL: High or Critical npm vulnerabilities" >&2
    ERRORS=$((ERRORS + 1))
fi

echo ""
if [[ $ERRORS -eq 0 ]]; then
    echo "dependency-audit: no High or Critical vulnerabilities."
else
    echo "dependency-audit: $ERRORS ecosystem(s) failed." >&2
    exit 1
fi
