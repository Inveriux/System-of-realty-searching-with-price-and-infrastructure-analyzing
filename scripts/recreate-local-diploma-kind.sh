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
GATEWAY_GENERATED_MANIFEST_PATH=""

INCLUDE_STATEFUL_SERVICES="${INCLUDE_STATEFUL_SERVICES:-true}"
INCLUDE_GEO_SERVICES="${INCLUDE_GEO_SERVICES:-true}"
PREPARE_OSRM_DATA="${PREPARE_OSRM_DATA:-false}"
BUILD_IMAGES="${BUILD_IMAGES:-false}"

readonly IMAGE_SERVICE="ImageService|diploma.imageservice:latest|${BACKEND_ROOT}/Services/ImageService/Dockerfile|image-service-deployment"
readonly LISTING_PAGE_BFF_SERVICE="ListingPageBff|diploma.listingpagebff:1.0|${BACKEND_ROOT}/Services/ListingPageBff/Dockerfile|listing-page-bff"
readonly LISTING_SERVICE="ListingService|diploma.listingservice:1.0|${BACKEND_ROOT}/Services/ListingService/Dockerfile|listing-service-deployment"
readonly USER_SERVICE="UserService|diploma.userservice:0.7|${BACKEND_ROOT}/Services/UserService/Dockerfile|user-service-deployment"
readonly SEARCH_SERVICE="SearchService|diploma.searchservice:1.0|${BACKEND_ROOT}/Services/SearchService/Dockerfile|search-service"
readonly INFRA_SERVICE="InfrastructureService|diploma.infrastructureservice:1.0|${BACKEND_ROOT}/Services/InfrastructureService/Dockerfile|infrastructure-service-deployment"
readonly PRICING_SERVICE="PricingAnalyticsService|diploma.pricinganalyticsservice:1.0|${BACKEND_ROOT}/Services/PricingAnalyticsService/Dockerfile|pricing-analytics-service"
readonly CHAT_SERVICE="ChatService|diploma.chatservice:1.0|${BACKEND_ROOT}/Services/ChatService/Dockerfile|chat-service-deployment"

APP_SERVICES=(
  "${IMAGE_SERVICE}"
  "${LISTING_PAGE_BFF_SERVICE}"
)

ROLLOUT_DEPLOYMENTS=(
  "image-service-deployment"
  "listing-page-bff"
)

if [[ "${INCLUDE_STATEFUL_SERVICES}" == "true" ]]; then
  APP_SERVICES+=(
    "${LISTING_SERVICE}"
    "${USER_SERVICE}"
    "${SEARCH_SERVICE}"
    "${INFRA_SERVICE}"
    "${PRICING_SERVICE}"
    "${CHAT_SERVICE}"
  )

  ROLLOUT_DEPLOYMENTS+=(
    "listing-service-deployment"
    "user-service-deployment"
    "search-service"
    "infrastructure-service-deployment"
    "pricing-analytics-service"
    "chat-service-deployment"
  )
fi

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

  if [[ -n "${GATEWAY_GENERATED_MANIFEST_PATH}" && -f "${GATEWAY_GENERATED_MANIFEST_PATH}" ]]; then
    rm -f "${GATEWAY_GENERATED_MANIFEST_PATH}"
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

prepare_host_directories() {
  log "Preparing persistent host directories under ${KIND_HOST_DATA_ROOT}"

  mkdir -p "${KIND_HOST_DATA_ROOT}"

  if [[ "${INCLUDE_STATEFUL_SERVICES}" == "true" ]]; then
    mkdir -p \
      "${KIND_HOST_DATA_ROOT}/listing-service-data" \
      "${KIND_HOST_DATA_ROOT}/user-service-data" \
      "${KIND_HOST_DATA_ROOT}/search-service-data" \
      "${KIND_HOST_DATA_ROOT}/infrastructure-service-data" \
      "${KIND_HOST_DATA_ROOT}/chat-service-data"
  fi

  if [[ "${INCLUDE_GEO_SERVICES}" == "true" ]]; then
    mkdir -p \
      "${KIND_HOST_DATA_ROOT}/overpass-belarus-db" \
      "${KIND_HOST_DATA_ROOT}/osrm-foot-data" \
      "${KIND_HOST_DATA_ROOT}/osrm-car-data"
  fi
}

prepare_osrm_data() {
  if [[ "${INCLUDE_GEO_SERVICES}" != "true" ]]; then
    log "Skipping OSRM data preparation because geo services are disabled"
    return
  fi

  if [[ "${PREPARE_OSRM_DATA}" != "true" ]]; then
    log "Skipping OSRM data preparation because PREPARE_OSRM_DATA=${PREPARE_OSRM_DATA}"
    return
  fi

  log "Preparing OSRM Belarus graphs on persistent host storage"
  "${REPO_ROOT}/scripts/prepare-osrm-belarus-data.sh"
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

service_name() {
  local entry="$1"
  echo "${entry%%|*}"
}

service_image() {
  local entry="$1"
  local rest="${entry#*|}"
  echo "${rest%%|*}"
}

service_dockerfile() {
  local entry="$1"
  local rest="${entry#*|}"
  rest="${rest#*|}"
  echo "${rest%%|*}"
}

build_images() {
  if [[ "${BUILD_IMAGES}" != "true" ]]; then
    log "Skipping backend image builds because BUILD_IMAGES=${BUILD_IMAGES}"
    return
  fi

  log "Building backend images"

  local entry image dockerfile name
  for entry in "${APP_SERVICES[@]}"; do
    name="$(service_name "${entry}")"
    image="$(service_image "${entry}")"
    dockerfile="$(service_dockerfile "${entry}")"

    log "Building ${name} as ${image}"
    docker build \
      --tag "${image}" \
      --file "${dockerfile}" \
      "${BACKEND_ROOT}"
  done
}

ensure_images_exist() {
  log "Checking that required Docker images already exist locally"

  local entry image missing=0
  for entry in "${APP_SERVICES[@]}"; do
    image="$(service_image "${entry}")"
    if ! docker image inspect "${image}" >/dev/null 2>&1; then
      echo "Missing local Docker image: ${image}" >&2
      missing=1
    fi
  done

  if [[ "${missing}" -ne 0 ]]; then
    echo "One or more required images are missing locally. Build them first or rerun with BUILD_IMAGES=true." >&2
    exit 1
  fi
}

load_images_into_kind() {
  log "Loading images into kind cluster ${CLUSTER_NAME}"

  local entry image
  for entry in "${APP_SERVICES[@]}"; do
    image="$(service_image "${entry}")"
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

apply_base_manifests() {
  log "Applying base Kubernetes manifests to namespace ${NAMESPACE}"

  kubectl --context "${CLUSTER_CONTEXT}" apply -f "${K8S_ROOT}/diploma-namespace.yaml"

  kubectl --context "${CLUSTER_CONTEXT}" create secret tls diploma-tls \
    --namespace "${NAMESPACE}" \
    --cert "${CERT_DIR}/diploma.crt" \
    --key "${CERT_DIR}/diploma.key" \
    --dry-run=client \
    -o yaml | kubectl --context "${CLUSTER_CONTEXT}" apply -f -

  kubectl --context "${CLUSTER_CONTEXT}" apply -f "${BACKEND_K8S_ROOT}/envoy-gateway-class.yaml"
}

generate_gateway_manifest() {
  GATEWAY_GENERATED_MANIFEST_PATH="$(mktemp "${TMPDIR:-/tmp}/diploma-gateway-${CLUSTER_NAME}-XXXX.yaml")"

  cat >"${GATEWAY_GENERATED_MANIFEST_PATH}" <<'EOF'
apiVersion: gateway.networking.k8s.io/v1
kind: Gateway
metadata:
  name: diploma-gateway
  namespace: diploma
spec:
  gatewayClassName: eg
  listeners:
  - name: http
    protocol: HTTP
    port: 8080
    allowedRoutes:
      namespaces:
        from: Same
  - name: https
    protocol: HTTPS
    port: 8443
    allowedRoutes:
      namespaces:
        from: Same
    tls:
      mode: Terminate
      certificateRefs:
        - kind: Secret
          name: diploma-tls
---
apiVersion: gateway.networking.k8s.io/v1
kind: HTTPRoute
metadata:
  name: diploma-route
  namespace: diploma
spec:
  parentRefs:
    - name: diploma-gateway
  hostnames:
    - listing-service.com
    - diploma.com
  rules:
    - matches:
      - path:
          type: PathPrefix
          value: /api/images
      backendRefs:
        - name: image-service-svc
          port: 8080
    - matches:
      - path:
          type: PathPrefix
          value: /api/listing-pages
      backendRefs:
        - name: listing-page-bff-svc
          port: 8080
EOF

  if [[ "${INCLUDE_STATEFUL_SERVICES}" == "true" ]]; then
    cat >>"${GATEWAY_GENERATED_MANIFEST_PATH}" <<'EOF'
    - matches:
      - path:
          type: PathPrefix
          value: /api/listings
      backendRefs:
        - name: listing-service-svc
          port: 8080
    - matches:
      - path:
          type: PathPrefix
          value: /api/search
      backendRefs:
        - name: search-service-svc
          port: 8080
    - matches:
      - path:
          type: PathPrefix
          value: /api/pricing-analytics
      backendRefs:
        - name: pricing-analytics-service-svc
          port: 8080
    - matches:
      - path:
          type: PathPrefix
          value: /api/locations
      backendRefs:
        - name: infrastructure-service-svc
          port: 8080
    - matches:
      - path:
          type: PathPrefix
          value: /api/listing_types
      - path:
          type: PathPrefix
          value: /api/building_types
      - path:
          type: PathPrefix
          value: /api/bathroom_types
      - path:
          type: PathPrefix
          value: /api/renovation_types
      - path:
          type: PathPrefix
          value: /api/building_materials
      - path:
          type: PathPrefix
          value: /api/class_types
      - path:
          type: PathPrefix
          value: /api/gas_types
      - path:
          type: PathPrefix
          value: /api/heating_types
      - path:
          type: PathPrefix
          value: /api/land_categories
      - path:
          type: PathPrefix
          value: /api/layout_types
      - path:
          type: PathPrefix
          value: /api/office_classes
      - path:
          type: PathPrefix
          value: /api/road_access_types
      - path:
          type: PathPrefix
          value: /api/sewer_types
      - path:
          type: PathPrefix
          value: /api/wall_materials
      - path:
          type: PathPrefix
          value: /api/water_supply_types
      backendRefs:
        - name: listing-service-svc
          port: 8080
    - matches:
      - path:
          type: PathPrefix
          value: /api/users
      backendRefs:
        - name: user-service-svc
          port: 8080
    - matches:
      - path:
          type: PathPrefix
          value: /api/chat
      - path:
          type: PathPrefix
          value: /hubs/chat
      backendRefs:
        - name: chat-service-svc
          port: 8080
EOF
  fi

  cat >>"${GATEWAY_GENERATED_MANIFEST_PATH}" <<'EOF'
---
apiVersion: gateway.networking.k8s.io/v1
kind: HTTPRoute
metadata:
  name: diploma-mobile-route
  namespace: diploma
spec:
  parentRefs:
    - name: diploma-gateway
  rules:
    - matches:
      - path:
          type: PathPrefix
          value: /api/images
      backendRefs:
        - name: image-service-svc
          port: 8080
    - matches:
      - path:
          type: PathPrefix
          value: /api/listing-pages
      backendRefs:
        - name: listing-page-bff-svc
          port: 8080
EOF

  if [[ "${INCLUDE_STATEFUL_SERVICES}" == "true" ]]; then
    cat >>"${GATEWAY_GENERATED_MANIFEST_PATH}" <<'EOF'
    - matches:
      - path:
          type: PathPrefix
          value: /api/listings
      backendRefs:
        - name: listing-service-svc
          port: 8080
    - matches:
      - path:
          type: PathPrefix
          value: /api/search
      backendRefs:
        - name: search-service-svc
          port: 8080
    - matches:
      - path:
          type: PathPrefix
          value: /api/pricing-analytics
      backendRefs:
        - name: pricing-analytics-service-svc
          port: 8080
    - matches:
      - path:
          type: PathPrefix
          value: /api/locations
      backendRefs:
        - name: infrastructure-service-svc
          port: 8080
    - matches:
      - path:
          type: PathPrefix
          value: /api/listing_types
      - path:
          type: PathPrefix
          value: /api/building_types
      - path:
          type: PathPrefix
          value: /api/bathroom_types
      - path:
          type: PathPrefix
          value: /api/renovation_types
      - path:
          type: PathPrefix
          value: /api/building_materials
      - path:
          type: PathPrefix
          value: /api/class_types
      - path:
          type: PathPrefix
          value: /api/gas_types
      - path:
          type: PathPrefix
          value: /api/heating_types
      - path:
          type: PathPrefix
          value: /api/land_categories
      - path:
          type: PathPrefix
          value: /api/layout_types
      - path:
          type: PathPrefix
          value: /api/office_classes
      - path:
          type: PathPrefix
          value: /api/road_access_types
      - path:
          type: PathPrefix
          value: /api/sewer_types
      - path:
          type: PathPrefix
          value: /api/wall_materials
      - path:
          type: PathPrefix
          value: /api/water_supply_types
      backendRefs:
        - name: listing-service-svc
          port: 8080
    - matches:
      - path:
          type: PathPrefix
          value: /api/users
      backendRefs:
        - name: user-service-svc
          port: 8080
    - matches:
      - path:
          type: PathPrefix
          value: /api/chat
      - path:
          type: PathPrefix
          value: /hubs/chat
      backendRefs:
        - name: chat-service-svc
          port: 8080
EOF
  fi
}

apply_gateway_manifest() {
  log "Applying Gateway API manifests for the enabled service set"
  generate_gateway_manifest
  kubectl --context "${CLUSTER_CONTEXT}" apply -f "${GATEWAY_GENERATED_MANIFEST_PATH}"
}

apply_directory_files() {
  local manifest_dir="$1"
  shift

  local file path
  for file in "$@"; do
    path="${manifest_dir}/${file}"
    if [[ -f "${path}" ]]; then
      kubectl --context "${CLUSTER_CONTEXT}" apply -f "${path}"
    fi
  done
}

apply_stateless_manifests() {
  log "Applying stateless service manifests"

  apply_directory_files "${BACKEND_K8S_ROOT}/ImageService" \
    "configmap.yaml" \
    "deployment.yaml"

  apply_directory_files "${BACKEND_K8S_ROOT}/ListingPageBff" \
    "configmap.yaml" \
    "deployment.yaml"
}

apply_stateful_manifests() {
  log "Applying manifests for DB-backed services"

  apply_directory_files "${BACKEND_K8S_ROOT}/ListingService" \
    "pv.yaml" \
    "pvc.yaml" \
    "db-secret.yaml" \
    "database-stateful.yaml" \
    "configmap.yaml" \
    "dep-secret.yaml" \
    "deployment.yaml"

  apply_directory_files "${BACKEND_K8S_ROOT}/UserService" \
    "pv.yaml" \
    "pvc.yaml" \
    "db-secret.yaml" \
    "database-stateful.yaml" \
    "configmap.yaml" \
    "dep-secret.yaml" \
    "deployment.yaml"

  apply_directory_files "${BACKEND_K8S_ROOT}/SearchService" \
    "pv.yaml" \
    "pvc.yaml" \
    "db-secret.yaml" \
    "database-stateful.yaml" \
    "configmap.yaml" \
    "dep-secret.yaml" \
    "deployment.yaml"

  apply_directory_files "${BACKEND_K8S_ROOT}/InfrastructureService" \
    "pv.yaml" \
    "pvc.yaml" \
    "db-secret.yaml" \
    "database-stateful.yaml" \
    "configmap.yaml" \
    "dep-secret.yaml" \
    "deployment.yaml"

  apply_directory_files "${BACKEND_K8S_ROOT}/PricingAnalyticsService" \
    "configmap.yaml" \
    "dep-secret.yaml" \
    "deployment.yaml"

  apply_directory_files "${BACKEND_K8S_ROOT}/ChatService" \
    "pv.yaml" \
    "pvc.yaml" \
    "db-secret.yaml" \
    "database-stateful.yaml" \
    "configmap.yaml" \
    "dep-secret.yaml" \
    "deployment.yaml"
}

apply_geo_manifests() {
  log "Applying geo-service manifests"

  apply_directory_files "${BACKEND_K8S_ROOT}/Overpass" \
    "pv.yaml" \
    "deployment.yaml"

  apply_directory_files "${BACKEND_K8S_ROOT}/Osrm" \
    "pv.yaml" \
    "deployment.yaml"
}

apply_manifests() {
  apply_base_manifests
  apply_stateless_manifests

  if [[ "${INCLUDE_STATEFUL_SERVICES}" == "true" ]]; then
    apply_stateful_manifests
  else
    log "Skipping DB-backed manifests because INCLUDE_STATEFUL_SERVICES=${INCLUDE_STATEFUL_SERVICES}"
  fi

  if [[ "${INCLUDE_GEO_SERVICES}" == "true" ]]; then
    apply_geo_manifests
  else
    log "Skipping Overpass/OSRM manifests because INCLUDE_GEO_SERVICES=${INCLUDE_GEO_SERVICES}"
  fi

  apply_gateway_manifest
}

wait_for_rollouts() {
  log "Waiting for deployments to finish rolling out"

  local deployment
  for deployment in "${ROLLOUT_DEPLOYMENTS[@]}"; do
    kubectl --context "${CLUSTER_CONTEXT}" rollout status "deployment/${deployment}" -n "${NAMESPACE}" --timeout=5m
  done

  if [[ "${INCLUDE_GEO_SERVICES}" == "true" ]]; then
    kubectl --context "${CLUSTER_CONTEXT}" rollout status deployment/overpass-belarus-deployment -n "${NAMESPACE}" --timeout=15m
    kubectl --context "${CLUSTER_CONTEXT}" rollout status deployment/osrm-foot-deployment -n "${NAMESPACE}" --timeout=15m
    kubectl --context "${CLUSTER_CONTEXT}" rollout status deployment/osrm-car-deployment -n "${NAMESPACE}" --timeout=15m
  fi
}

print_summary() {
  log "Cluster bootstrap finished"
  kubectl --context "${CLUSTER_CONTEXT}" get pods -A
}

print_mode_summary() {
  log "Bootstrap mode summary"
  printf '  INCLUDE_STATEFUL_SERVICES=%s\n' "${INCLUDE_STATEFUL_SERVICES}"
  printf '  INCLUDE_GEO_SERVICES=%s\n' "${INCLUDE_GEO_SERVICES}"
  printf '  PREPARE_OSRM_DATA=%s\n' "${PREPARE_OSRM_DATA}"
  printf '  BUILD_IMAGES=%s\n' "${BUILD_IMAGES}"
}

main() {
  trap cleanup EXIT

  require_command docker
  require_command kind
  require_command kubectl
  require_command helm

  print_mode_summary
  delete_previous_namespace
  delete_previous_cluster
  prepare_host_directories
  prepare_osrm_data
  generate_kind_config
  create_cluster
  build_images
  ensure_images_exist
  load_images_into_kind
  install_envoy_gateway
  apply_manifests
  wait_for_rollouts
  print_summary
}

main "$@"
