#!/usr/bin/env bash
# Smoke test for make dev: the host (dotnet watch) and Vite start, and /api reaches the host through the
# Vite proxy. Needs ports 5001 and 5173 free, so it is not part of make verify. Run via: make check-dev
set -euo pipefail

cd "$(dirname "$0")/../.."

for url in https://localhost:5001/ http://localhost:5173/; do
    if curl -sk -m 2 -o /dev/null "$url"; then
        echo "FAIL: $url is already in use; stop make dev, make run or the debugger first." >&2
        exit 1
    fi
done

LOG=$(mktemp)
# setsid gives make dev its own process group so cleanup stops dotnet watch and Vite too.
setsid make --no-print-directory dev >"$LOG" 2>&1 </dev/null &
DEV=$!
# dotnet watch ignores TERM and INT without a terminal, so escalate to KILL for the whole group.
cleanup() {
    kill -TERM -- "-$DEV" 2>/dev/null || true
    for _ in $(seq 1 10); do pgrep -g "$DEV" >/dev/null || break; sleep 1; done
    kill -KILL -- "-$DEV" 2>/dev/null || true
    wait "$DEV" 2>/dev/null || true
    rm -f "$LOG"
}
trap cleanup EXIT

for _ in $(seq 1 120); do
    if [[ "$(curl -s -m 2 http://localhost:5173/api/v1/health || true)" == '{"status":"ok"}' ]]; then
        echo "dev-smoke: make dev serves /api/v1/health through the Vite proxy."
        exit 0
    fi
    if ! kill -0 "$DEV" 2>/dev/null; then
        echo "FAIL: make dev exited before the proxy answered. Log:" >&2
        sed 's/\x1b\[[0-9;]*m//g' "$LOG" | tail -20 >&2
        exit 1
    fi
    sleep 1
done

echo "FAIL: /api/v1/health did not answer through the Vite proxy within 120 s. Log:" >&2
sed 's/\x1b\[[0-9;]*m//g' "$LOG" | grep -v "proxy error\|ECONNREFUSED\|afterConnect" | tail -20 >&2
exit 1
