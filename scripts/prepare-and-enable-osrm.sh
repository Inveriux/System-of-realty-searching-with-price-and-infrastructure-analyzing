#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
CLUSTER_CONTEXT="${CLUSTER_CONTEXT:-kind-local-diploma}"
NAMESPACE="${NAMESPACE:-diploma}"

log() {
  printf '\n[%s] %s\n' "$(date '+%H:%M:%S')" "$1"
}

log "Preparing persistent OSRM Belarus graphs on host storage"
"${SCRIPT_DIR}/prepare-osrm-belarus-data.sh"

log "Scaling OSRM deployments back to 1 replica"
kubectl --context "${CLUSTER_CONTEXT}" -n "${NAMESPACE}" scale deployment/osrm-foot-deployment --replicas=1
kubectl --context "${CLUSTER_CONTEXT}" -n "${NAMESPACE}" scale deployment/osrm-car-deployment --replicas=1

log "Waiting for OSRM deployments to finish rolling out"
kubectl --context "${CLUSTER_CONTEXT}" -n "${NAMESPACE}" rollout status deployment/osrm-foot-deployment --timeout=20m
kubectl --context "${CLUSTER_CONTEXT}" -n "${NAMESPACE}" rollout status deployment/osrm-car-deployment --timeout=20m

log "OSRM services are ready"
kubectl --context "${CLUSTER_CONTEXT}" -n "${NAMESPACE}" get deployments osrm-foot-deployment osrm-car-deployment
