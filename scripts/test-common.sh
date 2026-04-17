#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
ARTIFACTS_ROOT="$REPO_ROOT/Tests"

run_dotnet_tests() {
  local project_path="$1"
  local kind="$2"
  local filter="$3"
  local name="$4"

  local results_dir="$ARTIFACTS_ROOT/test-results/$kind"
  local coverage_dir="$ARTIFACTS_ROOT/coverage/$kind"
  local logs_dir="$ARTIFACTS_ROOT/logs"
  local log_file="$logs_dir/${name}.log"

  mkdir -p "$results_dir" "$coverage_dir" "$logs_dir" "$ARTIFACTS_ROOT/coverage/combined"

  local command
  printf -v command 'cd %q && dotnet test %q --configuration Debug --filter %q --results-directory %q --logger %q --collect:%q -m:1 -nodeReuse:false -p:UseSharedCompilation=false' \
    "$REPO_ROOT" \
    "$project_path" \
    "$filter" \
    "$results_dir" \
    "trx;LogFileName=${name}.trx" \
    "XPlat Code Coverage"

  /usr/bin/bash -lc "$command"

  {
    echo "name=$name"
    echo "kind=$kind"
    echo "project=$project_path"
    echo "filter=$filter"
    echo "results_dir=$results_dir"
    echo "coverage_dir=$coverage_dir"
  } >"$log_file"

  local coverage_file
  coverage_file="$(find "$results_dir" -type f -name 'coverage.cobertura.xml' | sort | tail -n 1 || true)"
  if [[ -n "${coverage_file:-}" ]]; then
    cp "$coverage_file" "$coverage_dir/${name}.cobertura.xml"
  fi
}
