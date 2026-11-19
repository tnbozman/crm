#!/usr/bin/env bash
# Post-create script for the devcontainer. Must be idempotent.
set -euo pipefail

cd /workspaces/crm

echo "==> Restoring dotnet local tools..."
dotnet tool restore

echo "==> Creating the ASP.NET Core HTTPS development certificate (no-op if present)..."
dotnet dev-certs https

echo "==> Enabling corepack (pnpm)..."
corepack enable

echo "==> Installing frontend dependencies..."
if [[ -f "src/web/package.json" ]]; then
    pnpm --dir src/web install --frozen-lockfile
else
    echo "    src/web not yet present — skipping (will run after S0-5)"
fi

echo "==> Installing git hooks..."
make hooks

echo "==> Waiting for SQL Server at ${CRM_SQL_SERVER:-db}..."
if [[ -z "${CRM_SQL_PASSWORD:-}" ]]; then
    echo "    CRM_SQL_PASSWORD not set; skipping (make doctor will report it)."
else
    for i in $(seq 1 30); do
        if sqlcmd -C -S "${CRM_SQL_SERVER:-db}" -U "${CRM_SQL_USER:-sa}" -P "$CRM_SQL_PASSWORD" -Q 'SELECT 1' -b &>/dev/null; then
            echo "    SQL Server ready."
            break
        fi
        echo "    Attempt $i/30 — sleeping 3s..."
        sleep 3
    done
fi

echo "==> Running migrations..."
if [[ -f "src/Crm.Migrator/Crm.Migrator.csproj" ]]; then
    make db-reset
else
    echo "    Crm.Migrator not yet present — skipping (will run after S0-4)"
fi

echo ""
echo "==> Available targets:"
make help
