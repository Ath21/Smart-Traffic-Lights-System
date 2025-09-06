#!/bin/bash

# ==========================================
# Smart Traffic Light System - Docker Script
# ==========================================
# Usage:
#   ./stls-docker.sh up all         # start all services
#   ./stls-docker.sh up user        # start User Layer
#   ./stls-docker.sh down all       # stop all services
#   ./stls-docker.sh down all --clean # stop + prune volumes/images
#   ./stls-docker.sh restart traffic # restart Traffic Layer
#   ./stls-docker.sh logs           # view logs
#   ./stls-docker.sh ps             # list running containers
#   ./stls-docker.sh status         # inspect networks + containers
# ==========================================

DOCKER_COMPOSE_FILE="back-end/RabbitMQ/docker-compose.yaml"
PROJECT_NAME="stls_backend"

DOCKER_USERNAME="ath21"
DOCKER_REPO="stls"

# === All services and their Docker build paths ===
declare -A SERVICES_PATHS=(
    # User Layer
    [user_api]="./UserLayer/UserService/UserAPI"
    [notification_api]="./UserLayer/NotificationService/NotificationAPI"

    # Log Layer
    [log_api]="./LogLayer/LogService/LogAPI"

    # Traffic Layer
    [traffic_analytics_api]="./TrafficLayer/TrafficAnalyticsService/TrafficAnalyticsAPI"
    [traffic_light_controller_api]="./TrafficLayer/TrafficLightControllerService/TrafficLightControllerAPI"
    [traffic_light_coordinator_api]="./TrafficLayer/TrafficLightCoordinatorService/TrafficLightCoordinatorAPI"
    [intersection_controller_api]="./TrafficLayer/IntersectionControllerService/IntersectionControllerAPI"

    # Sensor Layer
    [sensor_api]="./SensorLayer/SensorService/SensorAPI"
    [detection_api]="./SensorLayer/DetectionService/DetectionAPI"
)

# === Service Groups ===
USER_SERVICES="user_api notification_api user_mssql"
TRAFFIC_SERVICES="traffic_analytics_api traffic_light_controller_api traffic_light_coordinator_api intersection_controller_api traffic_light_cache_redis traffic_light_postgres traffic_analytics_postgres"
SENSOR_SERVICES="sensor_api detection_api detection_mongo detection_cache_redis"
LOG_SERVICES="log_api log_mongo"
CORE_BROKER="rabbitmq"

# === Networks ===
NETWORKS=("rabbitmq_network" "user_network" "notification_network" "log_network" "traffic_light_cache_network" "traffic_light_network" "traffic_analytics_network" "detection_cache_network" "detection_network")

# ================================
# 🔧 Helpers
# ================================
print_help() {
    echo "Usage: ./stls-docker.sh {up|down|restart|logs|ps|status} [all|user|traffic|sensor|log|rabbitmq] [--clean]"
}

create_networks() {
    echo "🌐 Ensuring networks exist..."
    for net in "${NETWORKS[@]}"; do
        docker network ls | grep -q "$net" || docker network create "$net"
    done
}

is_container_running() {
    docker ps --format '{{.Names}}' | grep -q "^$1$"
}

build_and_push_image() {
    local service="$1"
    local dir="${SERVICES_PATHS[$service]}"
    local image="$DOCKER_USERNAME/$DOCKER_REPO:${service}"

    if [[ -n "$dir" && -f "$dir/Dockerfile" ]]; then
        echo "🔨 Building image for $service..."
        docker build -t "$image" -f "$dir/Dockerfile" "$dir" || {
            echo "❌ Build failed for $service"
            return 1
        }

        echo "📤 (Optional) Pushing image for $service to Docker Hub..."
        docker push "$image" || echo "⚠️ Push failed or skipped for $service"
    fi
}

check_and_build_missing_images() {
    local services=("$@")
    for service in "${services[@]}"; do
        local container_name="${service}_container"
        if ! is_container_running "$container_name"; then
            echo "❌ $container_name not running — attempting build..."
            build_and_push_image "$service"
        else
            echo "✅ $container_name is already running."
        fi
    done
}

# ================================
# 🐇 RabbitMQ
# ================================
start_rabbitmq() {
    echo "📦 Starting RabbitMQ..."
    docker compose -f $DOCKER_COMPOSE_FILE -p $PROJECT_NAME up -d $CORE_BROKER
    wait_for_rabbitmq
}

wait_for_rabbitmq() {
    echo "⏳ Waiting for RabbitMQ to be ready..."
    local container
    container=$(docker ps --filter "name=$CORE_BROKER" --format "{{.Names}}")

    if [[ -z "$container" ]]; then
        echo "❌ RabbitMQ container not found."
        exit 1
    fi

    until docker exec "$container" rabbitmqctl status &>/dev/null; do
        sleep 2
    done

    echo "✅ RabbitMQ is ready."
}

remove_docker_networks() {
    for net in "${NETWORKS[@]}"; do
        if docker network ls | grep -q "$net"; then
            echo "🔌 Removing Docker network '$net'..."
            docker network rm "$net"
        fi
    done
}

# ================================
# 🚦 Main Execution
# ================================
case "$1" in
  up)
    create_networks
    case "$2" in
      all)
        start_rabbitmq
        check_and_build_missing_images "${!SERVICES_PATHS[@]}"
        docker compose -f $DOCKER_COMPOSE_FILE -p $PROJECT_NAME up -d \
          $USER_SERVICES $TRAFFIC_SERVICES $SENSOR_SERVICES $LOG_SERVICES
        ;;
      user)
        check_and_build_missing_images user_api notification_api
        docker compose -f $DOCKER_COMPOSE_FILE -p $PROJECT_NAME up -d $USER_SERVICES
        ;;
      traffic)
        check_and_build_missing_images traffic_analytics_api traffic_light_controller_api traffic_light_coordinator_api intersection_controller_api
        docker compose -f $DOCKER_COMPOSE_FILE -p $PROJECT_NAME up -d $TRAFFIC_SERVICES
        ;;
      sensor)
        check_and_build_missing_images sensor_api detection_api
        docker compose -f $DOCKER_COMPOSE_FILE -p $PROJECT_NAME up -d $SENSOR_SERVICES
        ;;
      log)
        check_and_build_missing_images log_api
        docker compose -f $DOCKER_COMPOSE_FILE -p $PROJECT_NAME up -d $LOG_SERVICES
        ;;
      rabbitmq)
        start_rabbitmq
        ;;
      *)
        print_help
        ;;
    esac
    ;;
  down)
    DO_CLEAN=false
    if [[ "$3" == "--clean" ]]; then
      DO_CLEAN=true
    fi

    case "$2" in
      all)
        echo "🛑 Stopping all layers..."
        docker compose -f $DOCKER_COMPOSE_FILE -p $PROJECT_NAME stop \
          $LOG_SERVICES $SENSOR_SERVICES $TRAFFIC_SERVICES $USER_SERVICES $CORE_BROKER
        echo "🗑️ Removing containers..."
        docker compose -f $DOCKER_COMPOSE_FILE -p $PROJECT_NAME rm -f
        remove_docker_networks
        ;;
      user) docker compose -f $DOCKER_COMPOSE_FILE -p $PROJECT_NAME stop $USER_SERVICES ;;
      traffic) docker compose -f $DOCKER_COMPOSE_FILE -p $PROJECT_NAME stop $TRAFFIC_SERVICES ;;
      sensor) docker compose -f $DOCKER_COMPOSE_FILE -p $PROJECT_NAME stop $SENSOR_SERVICES ;;
      log) docker compose -f $DOCKER_COMPOSE_FILE -p $PROJECT_NAME stop $LOG_SERVICES ;;
      rabbitmq)
        docker compose -f $DOCKER_COMPOSE_FILE -p $PROJECT_NAME stop $CORE_BROKER
        remove_docker_networks
        ;;
      *) print_help ;;
    esac

    if $DO_CLEAN; then
      echo "🧹 Pruning anonymous Docker volumes..."
      docker volume prune -f
      echo "🧹 Pruning dangling Docker images..."
      docker image prune -f
    fi

    echo "🏁 Shutdown complete."
    ;;
  restart)
    $0 down ${2:-all}
    $0 up ${2:-all}
    ;;
  logs)
    docker compose -f $DOCKER_COMPOSE_FILE -p $PROJECT_NAME logs -f --tail=100
    ;;
  ps)
    docker compose -f $DOCKER_COMPOSE_FILE -p $PROJECT_NAME ps
    ;;
  status)
    echo "📋 Containers Status:"
    docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    echo -e "\n🌐 Networks Overview:"
    docker network ls
    ;;
  *)
    print_help
    ;;
e
