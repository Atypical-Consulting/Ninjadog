#!/usr/bin/env bash
set -euo pipefail

# Install the Ninjadog CLI tool from local source.
# Usage: ./scripts/install.sh

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Determine version: use latest git tag + "-local" suffix, skip MinVer
LATEST_TAG=$(git -C "$REPO_ROOT" tag --sort=-v:refname | head -1 2>/dev/null || echo "v0.0.0")
LOCAL_VERSION="${LATEST_TAG#v}-local"
echo "==> Version: $LOCAL_VERSION"

echo ""
echo "==> Packing Ninjadog CLI tool..."
dotnet pack "$REPO_ROOT/src/tools/Ninjadog.CLI/Ninjadog.CLI.csproj" \
  --configuration Release \
  --output "$REPO_ROOT/artifacts" \
  /p:MinVerSkip=true /p:Version="$LOCAL_VERSION"

# Find the generated nupkg
NUPKG=$(find "$REPO_ROOT/artifacts" -name 'Ninjadog.*.nupkg' -not -name '*.symbols.*' | sort -V | tail -1)

if [ -z "$NUPKG" ]; then
  echo "ERROR: No .nupkg found in artifacts/" >&2
  exit 1
fi

echo ""
echo "==> Installing ninjadog tool globally from $NUPKG..."
dotnet tool uninstall -g Ninjadog 2>/dev/null || true
dotnet tool install -g Ninjadog --add-source "$REPO_ROOT/artifacts" --version "$LOCAL_VERSION"

echo ""
echo "==> Verifying installation..."
ninjadog --help > /dev/null 2>&1 && echo "ninjadog is available on PATH"

echo ""
echo "Done! Run 'ninjadog --help' to get started."
