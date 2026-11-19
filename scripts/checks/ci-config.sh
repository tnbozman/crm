#!/usr/bin/env bash
# Checks CI, Dependabot and git hook configuration. Run via: make lint
set -euo pipefail

cd "$(dirname "$0")/../.."

ERRORS=0
fail() { echo "FAIL: $*" >&2; ERRORS=$((ERRORS + 1)); }

CI=.github/workflows/ci.yml
DEPENDABOT=.github/dependabot.yml

# A. CI exists and runs the same gate as developers.
if [[ ! -f "$CI" ]]; then
    fail "$CI is missing"
elif ! grep -qE '^\s+run:\s+make verify\s*$' "$CI"; then
    fail "$CI does not run 'make verify'"
fi

# B. Every action is pinned to a full commit SHA (supply chain: tags can be moved).
shopt -s nullglob
WORKFLOWS=(.github/workflows/*.yml .github/workflows/*.yaml)
for wf in "${WORKFLOWS[@]}"; do
    while IFS= read -r line; do
        ref=$(sed -E 's/^\s*-?\s*uses:\s*([^ #]+).*/\1/' <<<"$line")
        [[ "$ref" == ./* || "$ref" == docker://* ]] && continue
        [[ "$ref" =~ @[0-9a-f]{40}$ ]] || fail "$wf: '$ref' is not pinned to a commit SHA"
    done < <(grep -E '^\s*-?\s*uses:' "$wf" || true)
done

# C. actionlint (installed by scripts/tools/install-actionlint.sh in the devcontainer and CI).
if command -v actionlint >/dev/null; then
    if [[ ${#WORKFLOWS[@]} -gt 0 ]]; then
        actionlint "${WORKFLOWS[@]}" || fail "actionlint reported problems"
    fi
else
    echo "warn: actionlint not installed; run scripts/tools/install-actionlint.sh" >&2
fi

# D. Dependabot covers every ecosystem we ship.
for ecosystem in nuget npm github-actions devcontainers; do
    grep -qE "package-ecosystem:\s*\"?$ecosystem\"?" "$DEPENDABOT" 2>/dev/null \
        || fail "$DEPENDABOT has no update entry for '$ecosystem'"
done

# E. Git hooks: fast checks before commit, full gate before push.
check_hook() {
    local hook=".githooks/$1"; shift
    [[ -x "$hook" ]] || fail "$hook is not executable"
    for target in "$@"; do
        grep -qE "^[^#]*\bmake( -[^ ]+)* $target\b" "$hook" || fail "$hook does not run 'make $target'"
    done
}
check_hook pre-commit lint test-unit
check_hook pre-push verify

echo ""
if [[ $ERRORS -eq 0 ]]; then
    echo "ci-config: all checks passed."
else
    echo "ci-config: $ERRORS check(s) failed." >&2
    exit 1
fi
