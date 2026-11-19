#!/usr/bin/env bash
# Checks the VS Code workspace configuration. Run via: make lint
set -euo pipefail

cd "$(dirname "$0")/../.."

ERRORS=0
fail() { echo "FAIL: $*" >&2; ERRORS=$((ERRORS + 1)); }

LAUNCH=.vscode/launch.json
TASKS=.vscode/tasks.json
SETTINGS=.vscode/settings.json

# A. Every .vscode/*.json is valid JSON (no comments, so jq and CI tools can read it).
for f in .vscode/*.json; do
    jq empty "$f" 2>/dev/null || fail "$f is not valid JSON"
done
[[ $ERRORS -eq 0 ]] || { echo "vscode-config: fix JSON first." >&2; exit 1; }

LABELS=$(jq -r '.tasks[].label' "$TASKS")
has_label() { grep -qxF "$1" <<<"$LABELS"; }

# B. Every preLaunchTask (configurations and compounds) names a task.
while IFS= read -r task; do
    [[ -z "$task" ]] || has_label "$task" || fail "$LAUNCH preLaunchTask '$task' is not a task label in $TASKS"
done < <(jq -r '(.configurations[]?, .compounds[]?) | .preLaunchTask // empty' "$LAUNCH")

# C. Every task runs exactly one make target, and that target is in make help.
HELP_TARGETS=$(make --no-print-directory -s help | awk '{print $1}')
while IFS=$'\t' read -r label command; do
    if [[ ! "$command" =~ ^make\ ([a-zA-Z0-9_-]+)$ ]]; then
        fail "task '$label' runs '$command'; tasks must run 'make <target>'"
    elif ! grep -qx "${BASH_REMATCH[1]}" <<<"$HELP_TARGETS"; then
        fail "task '$label' runs 'make ${BASH_REMATCH[1]}', which make help does not list"
    fi
done < <(jq -r '.tasks[] | [.label, .command] | @tsv' "$TASKS")

# D. Every configuration a compound names exists.
NAMES=$(jq -r '.configurations[].name' "$LAUNCH")
while IFS= read -r name; do
    [[ -z "$name" ]] || grep -qxF "$name" <<<"$NAMES" || fail "compound refers to missing configuration '$name'"
done < <(jq -r '.compounds[]?.configurations[]' "$LAUNCH")

# E. Vitest Explorer points at the real Vitest config.
ROOT_CONFIG=$(jq -r '."vitest.rootConfig" // empty' "$SETTINGS")
if [[ -z "$ROOT_CONFIG" ]]; then
    fail "$SETTINGS has no vitest.rootConfig (Vitest Explorer would not find src/web)"
elif [[ ! -f "$ROOT_CONFIG" ]]; then
    fail "$SETTINGS vitest.rootConfig '$ROOT_CONFIG' does not exist"
fi

# F. Every launchSettingsProfile exists in the host's launchSettings.json.
while IFS=$'\t' read -r name profile file; do
    file=${file//\$\{workspaceFolder\}/.}
    jq -e --arg p "$profile" '.profiles[$p]' "$file" >/dev/null 2>&1 \
        || fail "configuration '$name' uses launch profile '$profile', which $file does not define"
done < <(jq -r '.configurations[] | select(.launchSettingsProfile) | [.name, .launchSettingsProfile, (.launchSettingsFilePath // "src/Crm.Host/Properties/launchSettings.json")] | @tsv' "$LAUNCH")

# G. The Vite /api proxy targets a URL the debugged host listens on (Host (.NET) launch profile).
HOST_PROFILE=$(jq -r '.configurations[] | select(.name == "Host (.NET)") | .launchSettingsProfile // empty' "$LAUNCH")
PROXY_TARGET=$(grep -A3 "'/api'" src/web/vite.config.ts | grep -oE "target: '[^']+'" | sed -E "s/target: '([^']+)'/\1/" || true)
if [[ -z "$HOST_PROFILE" ]]; then
    fail "configuration 'Host (.NET)' has no launchSettingsProfile, so its URLs are not the ones make run and the Vite proxy use"
elif [[ -z "$PROXY_TARGET" ]]; then
    fail "src/web/vite.config.ts has no '/api' proxy target"
else
    URLS=$(jq -r --arg p "$HOST_PROFILE" '.profiles[$p].applicationUrl // ""' src/Crm.Host/Properties/launchSettings.json | tr ';' '\n')
    grep -qxF "$PROXY_TARGET" <<<"$URLS" \
        || fail "Vite proxies /api to $PROXY_TARGET, but launch profile '$HOST_PROFILE' listens on: $(tr '\n' ' ' <<<"$URLS")"
fi

echo ""
if [[ $ERRORS -eq 0 ]]; then
    echo "vscode-config: all checks passed."
else
    echo "vscode-config: $ERRORS check(s) failed." >&2
    exit 1
fi
