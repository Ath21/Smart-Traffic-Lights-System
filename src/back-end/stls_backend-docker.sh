#!/bin/bash

# ==========================================
# Smart Traffic Light System - Docker Script
# ==========================================

DOCKER_USERNAME="ath21"
DOCKER_REPO="stls"

# Root directory (src/back-end/)
ROOT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# === Service Dockerfile paths ===
declare -A SERVICES_PATHS=(
    # User Layer
    [user_api]="$ROOT_DIR/UserLayer/Services/UserService/Docker/Dockerfile"
    [user_mssql]="$ROOT_DIR/UserLayer/Databases/UserDB/MSSQL/Docker/Dockerfile"
    [notification_api]="$ROOT_DIR/UserLayer/Services/NotificationService/Docker/Dockerfile"
    [notification_mongo]="$ROOT_DIR/UserLayer/Databases/NotificationDB/Mongo/Docker/Dockerfile"

    # Log Layer
    [log_api]="$ROOT_DIR/LogLayer/Services/LogService/Docker/Dockerfile"
    [log_mongo]="$ROOT_DIR/LogLayer/Databases/Mongo/Docker/Dockerfile"

    # Traffic Layer
    [traffic_light_coordinator_api]="$ROOT_DIR/TrafficLayer/Services/TrafficLightCoordinatorService/Docker/Dockerfile"
    [traffic_analytics_api]="$ROOT_DIR/TrafficLayer/Services/TrafficAnalyticsService/Docker/Dockerfile"
    [traffic_light_controller_api]="$ROOT_DIR/TrafficLayer/Services/TrafficLightControllerService/Docker/Dockerfile"
    [intersection_controller_api]="$ROOT_DIR/TrafficLayer/Services/IntersectionControllerService/Docker/Dockerfile"
    [traffic_light_postgres]="$ROOT_DIR/TrafficLayer/Databases/TrafficLightDB/PostgreSQL/Docker/Dockerfile"
    [traffic_analytics_postgres]="$ROOT_DIR/TrafficLayer/Databases/TrafficAnalyticsDB/PostgreSQL/Docker/Dockerfile"
    [traffic_light_cache_redis]="$ROOT_DIR/TrafficLayer/Databases/TrafficLightCacheDB/Redis/Docker/Dockerfile"

    # Sensor Layer
    [detection_api]="$ROOT_DIR/SensorLayer/Services/DetectionService/Docker/Dockerfile"
    [sensor_api]="$ROOT_DIR/SensorLayer/Services/SensorService/Docker/Dockerfile"
    [detection_mongo]="$ROOT_DIR/SensorLayer/Databases/DetectionDB/Mongo/Docker/Dockerfile"
    [detection_cache_redis]="$ROOT_DIR/SensorLayer/Databases/DetectionCacheDB/Redis/Docker/Dockerfile"

    # Message Layer
    [rabbitmq]="$ROOT_DIR/MessageLayer/RabbitMQ/Docker/Dockerfile"
)

# === Groups per Layer ===
USER_LAYER=("user_api" "user_mssql" "notification_api" "notification_mongo")
TRAFFIC_LAYER=("traffic_light_coordinator_api" "traffic_analytics_api" "traffic_light_controller_api" "intersection_controller_api" "traffic_analytics_postgres" "traffic_light_postgres" "traffic_light_cache_redis")
SENSOR_LAYER=("detection_api" "sensor_api" "detection_mongo" "detection_cache_redis")
LOG_LAYER=("log_api" "log_mongo")
MESSAGE_LAYER=("rabbitmq")

# === Networks ===
NETWORKS=(
  rabbitmq_network
  user_network
  notification_network
  log_network
  traffic_light_cache_network
  traffic_light_network
  traffic_analytics_network
  detection_cache_network
  detection_network
)

# === Only build & push API services ===
API_SERVICES=("user_api" "notification_api" "log_api" \
              "traffic_light_coordinator_api" "traffic_analytics_api" \
              "traffic_light_controller_api" "intersection_controller_api" \
              "detection_api" "sensor_api")

# ================================
# 🔧 Helpers
# ================================
print_help() {
    echo "Usage: ./stls_backend-docker.sh {up|down|restart|status|ps} /{user|traffic|sensor|log|message|all} [--build|--clean]"
}

compose_cmd() {
    local base_dir
    base_dir="$(dirname "$1")"
    local compose_yaml="$base_dir/docker-compose.yaml"
    local override_yaml="$base_dir/docker-compose.override.yaml"

    if [[ ! -f "$compose_yaml" ]]; then
        echo "❌ Missing docker-compose.yaml in $base_dir" >&2
        exit 1
    fi

    if [[ -f "$override_yaml" ]]; then
        echo "-f $compose_yaml -f $override_yaml"
    else
        echo "-f $compose_yaml"
    fi
}

create_networks() {
    echo "🌐 Ensuring networks exist..."
    for net in "${NETWORKS[@]}"; do
        if ! docker network ls --format '{{.Name}}' | grep -q "^${net}$"; then
            echo "🔌 Creating network: $net"
            docker network create "$net"
        fi
    done
}

remove_networks() {
    echo "🌐 Removing project networks..."
    for net in "${NETWORKS[@]}"; do
        if docker network ls --format '{{.Name}}' | grep -q "^${net}$"; then
            echo "❌ Removing network: $net"
            docker network rm "$net"
        fi
    done
}

build_service() {
    local service="$1"
    if [[ ! " ${API_SERVICES[*]} " =~ " ${service} " ]]; then
        echo "⏩ Skipping build for $service (DB or broker)"
        return
    fi

    local dockerfile="${SERVICES_PATHS[$service]}"
    if [[ ! -f "$dockerfile" ]]; then
        echo "⚠️  No Dockerfile for $service ($dockerfile), skipping build"
        return
    fi

    local image="${DOCKER_USERNAME}/${DOCKER_REPO}:${service}"

    echo "🔨 Building $image from $dockerfile"
    docker build -f "$dockerfile" -t "$image" "$ROOT_DIR"

    echo "☁️  Pushing $image to Docker Hub"
    docker push "$image"
}

start_layer() {
    local layer_name="$1"
    local build_flag="$2"
    local group_var=""

    case "$layer_name" in
      user)     group_var="USER_LAYER" ;;
      traffic)  group_var="TRAFFIC_LAYER" ;;
      sensor)   group_var="SENSOR_LAYER" ;;
      log)      group_var="LOG_LAYER" ;;
      message)  group_var="MESSAGE_LAYER" ;;
      *) echo "❌ Unknown layer: $layer_name" ; exit 1 ;;
    esac

    local group="$group_var[@]"
    local services=("${!group}")
    echo "🚀 Starting layer: $layer_name"
    compose_args=""
    for s in "${services[@]}"; do
        if [[ "$build_flag" == "--build" ]]; then
            build_service "$s"
        fi
        compose_args="$compose_args $(compose_cmd "${SERVICES_PATHS[$s]}")"
    done

    docker compose $compose_args -p "stls_${layer_name}" up -d
}

stop_layer() {
    local layer_name="$1"
    local group_var=""

    case "$layer_name" in
      user)     group_var="USER_LAYER" ;;
      traffic)  group_var="TRAFFIC_LAYER" ;;
      sensor)   group_var="SENSOR_LAYER" ;;
      log)      group_var="LOG_LAYER" ;;
      message)  group_var="MESSAGE_LAYER" ;;
      *) echo "❌ Unknown layer: $layer_name" ; exit 1 ;;
    esac

    local group="$group_var[@]"
    local services=("${!group}")
    echo "🛑 Stopping layer: $layer_name"
    compose_args=""
    for s in "${services[@]}"; do
        compose_args="$compose_args $(compose_cmd "${SERVICES_PATHS[$s]}")"
    done
    docker compose $compose_args -p "stls_${layer_name}" down
}

# ================================
# 🚦 Main Execution
# ================================
case "$1" in
  up)
    create_networks
    BUILD_FLAG="$3"
    case "$2" in
      /user)    start_layer user "$BUILD_FLAG" ;;
      /traffic) start_layer traffic "$BUILD_FLAG" ;;
      /sensor)  start_layer sensor "$BUILD_FLAG" ;;
      /log)     start_layer log "$BUILD_FLAG" ;;
      /message) start_layer message "$BUILD_FLAG" ;;
      /all)
        start_layer user "$BUILD_FLAG"
        start_layer traffic "$BUILD_FLAG"
        start_layer sensor "$BUILD_FLAG"
        start_layer log "$BUILD_FLAG"
        start_layer message "$BUILD_FLAG"
        ;;
      *) print_help ;;
    esac
    ;;
  down)
    DO_CLEAN=false
    [[ "$3" == "--clean" ]] && DO_CLEAN=true

    case "$2" in
      /user)    stop_layer user ;;
      /traffic) stop_layer traffic ;;
      /sensor)  stop_layer sensor ;;
      /log)     stop_layer log ;;
      /message) stop_layer message ;;
      /all)
        stop_layer user
        stop_layer traffic
        stop_layer sensor
        stop_layer log
        stop_layer message
        remove_networks
        ;;
      *) print_help ;;
    esac

    if $DO_CLEAN; then
      echo "🧹 Pruning anonymous Docker volumes..."
      docker volume prune -f
      echo "🧹 Pruning dangling Docker images..."
      docker image prune -f
    fi
    ;;
  restart)
    $0 down ${2:-/all}
    $0 up ${2:-/all}
    ;;
  status)
    docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    echo -e "\n🌐 Networks Overview:"
    docker network ls
    ;;
  ps)
    docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    ;;
  *)
    print_help
    ;;
esac
