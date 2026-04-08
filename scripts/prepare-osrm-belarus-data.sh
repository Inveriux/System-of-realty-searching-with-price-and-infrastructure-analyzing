#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

KIND_HOST_DATA_ROOT="${KIND_HOST_DATA_ROOT:-${REPO_ROOT}/.local/kind-data}"
OSRM_PBF_URL="${OSRM_PBF_URL:-https://download.geofabrik.de/europe/belarus-latest.osm.pbf}"
OSRM_PBF_NAME="${OSRM_PBF_NAME:-belarus-latest.osm.pbf}"
OSRM_DATASET_NAME="${OSRM_DATASET_NAME:-belarus-latest.osrm}"
OSRM_IMAGE="${OSRM_IMAGE:-osrm/osrm-backend:latest}"
CURL_IMAGE="${CURL_IMAGE:-curlimages/curl:8.12.1}"

log() {
  printf '\n[%s] %s\n' "$(date '+%H:%M:%S')" "$1"
}

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Missing required command: $1" >&2
    exit 1
  fi
}

prepare_profile() {
  local profile_name="$1"
  local lua_profile="$2"
  local target_dir="${KIND_HOST_DATA_ROOT}/osrm-${profile_name}-data"
  local pbf_path="${target_dir}/${OSRM_PBF_NAME}"
  local part_path="${target_dir}/${OSRM_PBF_NAME}.part"
  local dataset_path="${target_dir}/${OSRM_DATASET_NAME}"

  mkdir -p "${target_dir}"

  if [[ -f "${dataset_path}" ]]; then
    log "OSRM ${profile_name} graph already exists in ${target_dir}, skipping rebuild"
    return
  fi

  if [[ -f "${pbf_path}" ]]; then
    log "Reusing existing Belarus PBF for OSRM ${profile_name}"
  else
    log "Downloading Belarus PBF for OSRM ${profile_name}"
    rm -f "${part_path}"
    docker run --rm \
      --user 0:0 \
      -v "${target_dir}:/data" \
      "${CURL_IMAGE}" \
      /bin/sh -c "
        set -eu
        rm -f /data/${OSRM_PBF_NAME}.part
        curl --fail --location --retry 5 --retry-all-errors --output /data/${OSRM_PBF_NAME}.part '${OSRM_PBF_URL}'
        mv /data/${OSRM_PBF_NAME}.part /data/${OSRM_PBF_NAME}
      "
  fi

  log "Building OSRM ${profile_name} graph"
  docker run --rm \
    --user 0:0 \
    -v "${target_dir}:/data" \
    "${OSRM_IMAGE}" \
    /bin/sh -c "
      set -eu
      osrm-extract -p ${lua_profile} /data/${OSRM_PBF_NAME}
      osrm-partition /data/${OSRM_DATASET_NAME}
      osrm-customize /data/${OSRM_DATASET_NAME}
    "
}

main() {
  require_command docker

  prepare_profile "foot" "/opt/foot.lua"
  prepare_profile "car" "/opt/car.lua"

  log "OSRM Belarus graphs are ready under ${KIND_HOST_DATA_ROOT}"
}

main "$@"
