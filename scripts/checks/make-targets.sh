#!/usr/bin/env bash
# Asserts the Makefile exposes the targets promised in CLAUDE.md.
# Run via: make lint (and CI through make verify).
set -euo pipefail

cd "$(dirname "$0")/../.."

ERRORS=0
fail() { echo "FAIL: $*" >&2; ERRORS=$((ERRORS + 1)); }

HELP_TARGETS=$(make --no-print-directory -s help | awk '{print $1}' | sort -u)

has_target() { grep -qx "$1" <<<"$HELP_TARGETS"; }

# A. Every target in the CLAUDE.md "Commands" block appears in make help.
CLAUDE_TARGETS=$(awk '/^## Commands/,/^## Test-driven/' CLAUDE.md | grep -oE '^make [a-z-]+' | awk '{print $2}' | sort -u)
for t in $CLAUDE_TARGETS; do
    has_target "$t" || fail "CLAUDE.md lists 'make $t' but make help does not"
done

# B. Every target in the Makefile spec (docs/environment.md §3, the .PHONY list) appears in make help.
SPEC_TARGETS=$(awk '/^## 3\. Makefile/,/^## 4\./' docs/environment.md \
    | awk '/^\.PHONY:/{on=1} on{print} on && !/\\$/{exit}' \
    | sed -e 's/^\.PHONY://' -e 's/\\//g' | tr -s ' \t' '\n' | grep -v '^$' | sort -u)
[[ -n "$SPEC_TARGETS" ]] || fail "could not read the .PHONY list from docs/environment.md §3"
for t in $SPEC_TARGETS; do
    has_target "$t" || fail "docs/environment.md §3 specifies '$t' but make help does not list it"
done

echo ""
if [[ $ERRORS -eq 0 ]]; then
    echo "make-targets: all checks passed."
else
    echo "make-targets: $ERRORS check(s) failed." >&2
    exit 1
fi
