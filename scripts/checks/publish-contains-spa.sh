#!/usr/bin/env bash
# A publish from a clean state (no wwwroot yet, as in CI or a fresh clone) must contain the SPA.
# Run via: make verify
set -euo pipefail

cd "$(dirname "$0")/../.."

PUBLISH=artifacts/publish
rm -rf src/Crm.Host/wwwroot "$PUBLISH"

make --no-print-directory publish >/dev/null

INDEX="$PUBLISH/wwwroot/index.html"
if [[ ! -f "$INDEX" ]]; then
    echo "FAIL: $INDEX missing after make publish from a clean state" >&2
    exit 1
fi

# Every asset index.html references must be published too.
MISSING=0
for asset in $(grep -oE '(src|href)="/assets/[^"]+"' "$INDEX" | sed -E 's/^(src|href)="\/(.*)"$/\2/'); do
    if [[ ! -f "$PUBLISH/wwwroot/$asset" ]]; then
        echo "FAIL: $INDEX references $asset, which is not published" >&2
        MISSING=$((MISSING + 1))
    fi
done
[[ $MISSING -eq 0 ]] || exit 1

echo "publish-contains-spa: $INDEX and its assets are published."
