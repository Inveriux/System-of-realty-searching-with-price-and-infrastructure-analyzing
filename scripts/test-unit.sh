#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/test-common.sh"

run_dotnet_tests \
  "$REPO_ROOT/src/Backend/tests/Backend.UnitTests/Backend.UnitTests.csproj" \
  "unit" \
  "Category=Unit" \
  "backend-unit-tests"
