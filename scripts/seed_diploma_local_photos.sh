#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export SEED_PROFILE=diploma_local
export OUTPUT_FILE="${OUTPUT_FILE:-/home/Admin/Projects/Diploma/tmp/diploma_local_seed_results.jsonl}"
exec python3 "${SCRIPT_DIR}/seed_realty_showcase.py" "$@"
