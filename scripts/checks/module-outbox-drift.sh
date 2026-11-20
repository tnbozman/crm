#!/usr/bin/env bash
# ADR-009: each module owns a copy of the outbox, inbox and relay, generated from the template. This check fails when a
# module's copy differs from the template (after the module name is substituted), so a fix is made in the template and
# applied to every module. Run via: make lint (and CI through make verify).
set -euo pipefail

cd "$(dirname "$0")/../.."

TEMPLATE=scripts/templates/module/src/Modules/Crm.__Module__/Persistence/Messaging

ERRORS=0
fail() { echo "FAIL: $*" >&2; ERRORS=$((ERRORS + 1)); }

shopt -s nullglob
for module_dir in src/Modules/Crm.*/; do
    module=$(basename "$module_dir")
    module=${module#Crm.}
    lower=$(tr '[:upper:]' '[:lower:]' <<<"$module")
    messaging="${module_dir}Persistence/Messaging"

    expected_files=()
    for template in "$TEMPLATE"/*; do
        name=$(basename "$template")
        name=${name//__Module__/$module}
        expected_files+=("$name")
        actual="$messaging/$name"
        if [[ ! -f "$actual" ]]; then
            fail "$actual is missing (template: $template)"
        elif ! diff -u <(sed -e "s/__Module__/$module/g" -e "s/__module__/$lower/g" "$template") "$actual" >&2; then
            fail "$actual differs from the template $template; change the template and apply it to every module"
        fi
    done

    for actual in "$messaging"/*; do
        name=$(basename "$actual")
        [[ " ${expected_files[*]} " == *" $name "* ]] || fail "$actual is not in the template; outbox, inbox and relay code lives in $TEMPLATE"
    done
done

if [[ $ERRORS -eq 0 ]]; then
    echo "module-outbox-drift: every module's outbox, inbox and relay match the template."
else
    echo "module-outbox-drift: $ERRORS check(s) failed." >&2
    exit 1
fi
