#!/usr/bin/env bash
# Installs a pinned, checksum-verified actionlint (GitHub Actions workflow linter).
# Used by the devcontainer Dockerfile and CI. Usage: install-actionlint.sh [install-dir]
# To bump: change VERSION and both checksums from the release's actionlint_<version>_checksums.txt.
set -euo pipefail

VERSION=1.7.12
DIR="${1:-$HOME/.local/bin}"

case "$(uname -m)" in
    x86_64) ARCH=amd64; SHA256=8aca8db96f1b94770f1b0d72b6dddcb1ebb8123cb3712530b08cc387b349a3d8 ;;
    aarch64 | arm64) ARCH=arm64; SHA256=325e971b6ba9bfa504672e29be93c24981eeb1c07576d730e9f7c8805afff0c6 ;;
    *) echo "install-actionlint: unsupported architecture $(uname -m)" >&2; exit 1 ;;
esac

if command -v actionlint >/dev/null && [[ "$(actionlint -version | head -1)" == "$VERSION" ]]; then
    echo "actionlint $VERSION already installed"
    exit 0
fi

TMP=$(mktemp -d)
trap 'rm -rf "$TMP"' EXIT
ARCHIVE="actionlint_${VERSION}_linux_${ARCH}.tar.gz"
curl -fsSL -o "$TMP/$ARCHIVE" "https://github.com/rhysd/actionlint/releases/download/v${VERSION}/${ARCHIVE}"
echo "${SHA256}  $TMP/$ARCHIVE" | sha256sum --check --quiet
tar -xzf "$TMP/$ARCHIVE" -C "$TMP" actionlint
mkdir -p "$DIR"
install -m 0755 "$TMP/actionlint" "$DIR/actionlint"
echo "actionlint $VERSION installed to $DIR"
