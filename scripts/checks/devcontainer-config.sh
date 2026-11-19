#!/usr/bin/env bash
# Checks the devcontainer's network exposure. Run via: make lint
# The development SQL password is public (docker-compose.yml in a public repo), so SQL Server must not be
# published on the host's network interfaces. Reach it as `db` inside the compose network, or through VS Code
# port forwarding ("db:1433"), which binds to localhost on the developer's machine.
set -euo pipefail

cd "$(dirname "$0")/../.."

ERRORS=0
fail() { echo "FAIL: $*" >&2; ERRORS=$((ERRORS + 1)); }

COMPOSE=.devcontainer/docker-compose.yml
DEVCONTAINER=.devcontainer/devcontainer.json

# A. The db service publishes no ports (a `ports:` key between `db:` and the next top-level-indented key).
if awk '/^  db:/{in_db=1; next} /^  [a-z]/{in_db=0} in_db && /^    ports:/{found=1} END{exit !found}' "$COMPOSE"; then
    fail "$COMPOSE publishes db ports on the host; remove 'ports:' and use VS Code forwarding (\"db:1433\")"
fi

# B. If SQL Server is forwarded, it is forwarded from the db service (localhost-only on the developer's machine).
if jq -e '.forwardPorts // [] | map(tostring) | index("1433")' "$DEVCONTAINER" >/dev/null; then
    fail "$DEVCONTAINER forwards 1433 from the app container, where nothing listens; use \"db:1433\""
fi

echo ""
if [[ $ERRORS -eq 0 ]]; then
    echo "devcontainer-config: all checks passed."
else
    echo "devcontainer-config: $ERRORS check(s) failed." >&2
    exit 1
fi
