#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

CLUSTER_NAME="${CLUSTER_NAME:-local-diploma}"
CLUSTER_CONTEXT="kind-${CLUSTER_NAME}"
NAMESPACE="${NAMESPACE:-diploma}"
ENVOY_RELEASE_NAME="${ENVOY_RELEASE_NAME:-eg}"
ENVOY_NAMESPACE="${ENVOY_NAMESPACE:-envoy-gateway-system}"
ENVOY_GATEWAY_VERSION="${ENVOY_GATEWAY_VERSION:-v1.7.0}"

KIND_CONFIG_PATH="${REPO_ROOT}/src/K8S/kind-cluster.yaml"
K8S_ROOT="${REPO_ROOT}/src/K8S"
BACKEND_K8S_ROOT="${K8S_ROOT}/backend"
CERT_DIR="${K8S_ROOT}/cert"
BACKEND_ROOT="${REPO_ROOT}/src/Backend"
KIND_HOST_DATA_ROOT="${KIND_HOST_DATA_ROOT:-${REPO_ROOT}/.local/kind-data}"
KIND_GENERATED_CONFIG_PATH=""

readonly APP_IMAGES=(
  "diploma.listingservice:1.0|${BACKEND_ROOT}/Services/ListingService/Dockerfile"
  "diploma.userservice:0.7|${BACKEND_ROOT}/Services/UserService/Dockerfile"
  "diploma.searchservice:1.0|${BACKEND_ROOT}/Services/SearchService/Dockerfile"
  "diploma.infrastructureservice:1.0|${BACKEND_ROOT}/Services/InfrastructureService/Dockerfile"
  "diploma.imageservice:latest|${BACKEND_ROOT}/Services/ImageService/Dockerfile"
  "diploma.listingpagebff:1.0|${BACKEND_ROOT}/Services/ListingPageBff/Dockerfile"
  "diploma.pricinganalyticsservice:1.0|${BACKEND_ROOT}/Services/PricingAnalyticsService/Dockerfile"
)

log() {
  printf '\n[%s] %s\n' "$(date '+%H:%M:%S')" "$1"
}

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Missing required command: $1" >&2
    exit 1
  fi
}

cleanup() {
  if [[ -n "${KIND_GENERATED_CONFIG_PATH}" && -f "${KIND_GENERATED_CONFIG_PATH}" ]]; then
    rm -f "${KIND_GENERATED_CONFIG_PATH}"
  fi
}

cluster_exists() {
  kind get clusters 2>/dev/null | grep -Fxq "${CLUSTER_NAME}"
}

delete_previous_namespace() {
  if ! cluster_exists; then
    return
  fi

  if kubectl --context "${CLUSTER_CONTEXT}" get namespace "${NAMESPACE}" >/dev/null 2>&1; then
    log "Deleting namespace ${NAMESPACE} from ${CLUSTER_CONTEXT}"
    kubectl --context "${CLUSTER_CONTEXT}" delete namespace "${NAMESPACE}" --timeout=120s || true
  fi
}

delete_previous_cluster() {
  if cluster_exists; then
    log "Deleting existing kind cluster ${CLUSTER_NAME}"
    kind delete cluster --name "${CLUSTER_NAME}"
  fi
}

remove_previous_builds() {
  log "Removing previously built application images"

  local entry image
  for entry in "${APP_IMAGES[@]}"; do
    image="${entry%%|*}"
    docker image rm -f "${image}" >/dev/null 2>&1 || true
  done
}

prepare_host_directories() {
  log "Preparing persistent host directories under ${KIND_HOST_DATA_ROOT}"

  mkdir -p \
    "${KIND_HOST_DATA_ROOT}/listing-service-data" \
    "${KIND_HOST_DATA_ROOT}/user-service-data" \
    "${KIND_HOST_DATA_ROOT}/search-service-data" \
    "${KIND_HOST_DATA_ROOT}/infrastructure-service-data" \
    "${KIND_HOST_DATA_ROOT}/overpass-belarus-db"
}

generate_kind_config() {
  log "Generating kind config with host mounts from ${KIND_HOST_DATA_ROOT}"

  KIND_GENERATED_CONFIG_PATH="$(mktemp "${TMPDIR:-/tmp}/kind-${CLUSTER_NAME}-XXXX.yaml")"

  cat >"${KIND_GENERATED_CONFIG_PATH}" <<EOF
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
  - role: control-plane
    extraMounts:
      - hostPath: ${KIND_HOST_DATA_ROOT}
        containerPath: /mnt/diploma
  - role: worker
    extraMounts:
      - hostPath: ${KIND_HOST_DATA_ROOT}
        containerPath: /mnt/diploma
  - role: worker
    extraMounts:
      - hostPath: ${KIND_HOST_DATA_ROOT}
        containerPath: /mnt/diploma
EOF
}

create_cluster() {
  log "Creating kind cluster ${CLUSTER_NAME}"
  kind create cluster --name "${CLUSTER_NAME}" --config "${KIND_GENERATED_CONFIG_PATH:-${KIND_CONFIG_PATH}}"
}

build_images() {
  log "Building backend images"

  local entry image dockerfile
  for entry in "${APP_IMAGES[@]}"; do
    image="${entry%%|*}"
    dockerfile="${entry##*|}"

    log "Building ${image}"
    docker build \
      --tag "${image}" \
      --file "${dockerfile}" \
      "${BACKEND_ROOT}"
  done
}

load_images_into_kind() {
  log "Loading images into kind cluster ${CLUSTER_NAME}"

  local entry image
  for entry in "${APP_IMAGES[@]}"; do
    image="${entry%%|*}"
    kind load docker-image --name "${CLUSTER_NAME}" "${image}"
  done
}

install_envoy_gateway() {
  log "Installing Envoy Gateway ${ENVOY_GATEWAY_VERSION} via Helm"

  helm upgrade --install "${ENVOY_RELEASE_NAME}" \
    "oci://docker.io/envoyproxy/gateway-helm" \
    --version "${ENVOY_GATEWAY_VERSION}" \
    --kube-context "${CLUSTER_CONTEXT}" \
    --namespace "${ENVOY_NAMESPACE}" \
    --create-namespace

  kubectl --context "${CLUSTER_CONTEXT}" wait \
    --namespace "${ENVOY_NAMESPACE}" \
    --for=condition=Available \
    --timeout=5m \
    deployment/envoy-gateway
}

apply_manifests() {
  log "Applying Kubernetes manifests to namespace ${NAMESPACE}"

  kubectl --context "${CLUSTER_CONTEXT}" apply -f "${K8S_ROOT}/diploma-namespace.yaml"

  kubectl --context "${CLUSTER_CONTEXT}" create secret tls diploma-tls \
    --namespace "${NAMESPACE}" \
    --cert "${CERT_DIR}/diploma.crt" \
    --key "${CERT_DIR}/diploma.key" \
    --dry-run=client \
    -o yaml | kubectl --context "${CLUSTER_CONTEXT}" apply -f -

  kubectl --context "${CLUSTER_CONTEXT}" apply -f "${BACKEND_K8S_ROOT}/envoy-gateway-class.yaml"
  kubectl --context "${CLUSTER_CONTEXT}" apply -f "${BACKEND_K8S_ROOT}/gateway.yaml"

  local manifest_dir
  for manifest_dir in "${BACKEND_K8S_ROOT}"/*; do
    if [[ ! -d "${manifest_dir}" ]]; then
      continue
    fi

    kubectl --context "${CLUSTER_CONTEXT}" apply -R -f "${manifest_dir}"
  done
}

wait_for_rollouts() {
  log "Waiting for deployments to finish rolling out"

  kubectl --context "${CLUSTER_CONTEXT}" rollout status deployment/listing-service-deployment -n "${NAMESPACE}" --timeout=5m
  kubectl --context "${CLUSTER_CONTEXT}" rollout status deployment/user-service-deployment -n "${NAMESPACE}" --timeout=5m
  kubectl --context "${CLUSTER_CONTEXT}" rollout status deployment/search-service -n "${NAMESPACE}" --timeout=5m
  kubectl --context "${CLUSTER_CONTEXT}" rollout status deployment/infrastructure-service-deployment -n "${NAMESPACE}" --timeout=5m
  kubectl --context "${CLUSTER_CONTEXT}" rollout status deployment/image-service-deployment -n "${NAMESPACE}" --timeout=5m
  kubectl --context "${CLUSTER_CONTEXT}" rollout status deployment/listing-page-bff -n "${NAMESPACE}" --timeout=5m
  kubectl --context "${CLUSTER_CONTEXT}" rollout status deployment/pricing-analytics-service -n "${NAMESPACE}" --timeout=5m
  kubectl --context "${CLUSTER_CONTEXT}" rollout status deployment/overpass-belarus-deployment -n "${NAMESPACE}" --timeout=15m
}

print_summary() {
  log "Cluster bootstrap finished"
  kubectl --context "${CLUSTER_CONTEXT}" get pods -A
}

main() {
  trap cleanup EXIT

  require_command docker
  require_command kind
  require_command kubectl
  require_command helm

  delete_previous_namespace
  delete_previous_cluster
  remove_previous_builds
  prepare_host_directories
  generate_kind_config
  create_cluster
  build_images
  load_images_into_kind
  install_envoy_gateway
  apply_manifests
  wait_for_rollouts
  print_summary
}

main "$@"
