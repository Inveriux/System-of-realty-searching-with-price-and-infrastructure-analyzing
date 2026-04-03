#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export SEED_PROFILE=multicity
exec python3 "${SCRIPT_DIR}/seed_realty_showcase.py" "$@"
