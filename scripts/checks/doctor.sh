#!/usr/bin/env bash
# Checks the devcontainer environment. Exit non-zero if any check fails.
# Run via: make doctor
set -euo pipefail

ERRORS=0
fail() { echo "FAIL: $*" >&2; ERRORS=$((ERRORS + 1)); }
pass() { echo "ok:   $*"; }

# ---------------------------------------------------------------------------
# Tool checks
# ---------------------------------------------------------------------------

# dotnet must be 10.x
if ! command -v dotnet &>/dev/null; then
    fail "dotnet not found"
else
    DOTNET_VER=$(dotnet --version 2>/dev/null)
    if [[ "$DOTNET_VER" != 10.* ]]; then
        fail "dotnet version is $DOTNET_VER, need 10.x"
    else
        pass "dotnet $DOTNET_VER"
    fi
fi

for tool in node pnpm make sqlcmd git; do
    if ! command -v "$tool" &>/dev/null; then
        fail "$tool not found"
    else
        pass "$tool present"
    fi
done

# ---------------------------------------------------------------------------
# SQL Server connectivity
# ---------------------------------------------------------------------------

SQL_SERVER="${CRM_SQL_SERVER:-}"
if [[ -z "$SQL_SERVER" ]]; then
    fail "CRM_SQL_SERVER not set"
else
    # SA password is the devcontainer default; override with MSSQL_SA_PASSWORD if set
    SQL_PASS="${MSSQL_SA_PASSWORD:-Dev_Only_Passw0rd!}"
    if sqlcmd -C -S "$SQL_SERVER" -U sa -P "$SQL_PASS" -Q 'SELECT 1' -b &>/dev/null; then
        pass "SQL Server at $SQL_SERVER accepts login"
    else
        fail "SQL Server at $SQL_SERVER did not accept login (sa / \$MSSQL_SA_PASSWORD)"
    fi
fi

# ---------------------------------------------------------------------------
# VS Code extensions parity
# ---------------------------------------------------------------------------

DC_JSON=".devcontainer/devcontainer.json"
VS_JSON=".vscode/extensions.json"

if [[ ! -f "$DC_JSON" ]]; then
    fail "$DC_JSON not found"
elif [[ ! -f "$VS_JSON" ]]; then
    fail "$VS_JSON not found"
else
    DC_EXTS=$(jq -r '.customizations.vscode.extensions[] | ascii_downcase' "$DC_JSON" | sort)
    VS_EXTS=$(jq -r '.recommendations[] | ascii_downcase' "$VS_JSON" | sort)
    if [[ "$DC_EXTS" == "$VS_EXTS" ]]; then
        pass "VS Code extensions match devcontainer"
    else
        fail "VS Code extensions mismatch between $DC_JSON and $VS_JSON"
        diff <(echo "$DC_EXTS") <(echo "$VS_EXTS") >&2 || true
    fi
fi

# ---------------------------------------------------------------------------
# Summary
# ---------------------------------------------------------------------------

echo ""
if [[ $ERRORS -eq 0 ]]; then
    echo "doctor: all checks passed."
else
    echo "doctor: $ERRORS check(s) failed." >&2
    exit 1
fi
