#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="rabbitmq_network"
RABBITMQ_DIR="./RabbitMQ"
DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE_FILE="docker-compose.override.yaml"

USER_API_DIR="./UserLayer/UserService/UserAPI"
LOG_API_DIR="./LogLayer/LogService/LogAPI"
NOTIFICATION_API_DIR="./UserLayer/NotificationService/NotificationAPI"
TRAFFIC_DATA_API_DIR="./TrafficLayer/TrafficDataAnalyticsService/TrafficDataAnalyticsAPI"
BUILD_CONTEXT_USER="."
BUILD_CONTEXT_LOG="."
BUILD_CONTEXT_NOTIFICATION="."
BUILD_CONTEXT_TRAFFIC_DATA="."

DOCKER_USERNAME="ath21"
DOCKER_REPO="stls"
USER_API_IMAGE="$DOCKER_USERNAME/$DOCKER_REPO:user_api"
LOG_API_IMAGE="$DOCKER_USERNAME/$DOCKER_REPO:log_api"
NOTIFICATION_API_IMAGE="$DOCKER_USERNAME/$DOCKER_REPO:notification_api"
TRAFFIC_DATA_API_IMAGE="$DOCKER_USERNAME/$DOCKER_REPO:traffic_data_analytics_api"

# ================================
# 📦 Build & Push if Needed
# ================================
is_service_running() {
    docker ps --format '{{.Names}}' | grep -q "^$1$"
}

build_and_push_image_for() {
    case "$1" in
        "user_api_container")
            docker build -t "$USER_API_IMAGE" -f "$USER_API_DIR/Dockerfile" "$BUILD_CONTEXT_USER" && docker push "$USER_API_IMAGE"
            ;;
        "log_api_container")
            docker build -t "$LOG_API_IMAGE" -f "$LOG_API_DIR/Dockerfile" "$BUILD_CONTEXT_LOG" && docker push "$LOG_API_IMAGE"
            ;;
        "notification_api_container")
            docker build -t "$NOTIFICATION_API_IMAGE" -f "$NOTIFICATION_API_DIR/Dockerfile" "$BUILD_CONTEXT_NOTIFICATION" && docker push "$NOTIFICATION_API_IMAGE"
            ;;
        "traffic_data_analytics_api_container")
            docker build -t "$TRAFFIC_DATA_API_IMAGE" -f "$TRAFFIC_DATA_API_DIR/Dockerfile" "$BUILD_CONTEXT_TRAFFIC_DATA" && docker push "$TRAFFIC_DATA_API_IMAGE"
            ;;
        *) echo "⚠️ Unknown container: $1" ;;
    esac
}

check_core_containers_and_build_missing() {
    local containers=("log_api_container" "notification_api_container" "traffic_data_analytics_api_container" "user_api_container")
    for c in "${containers[@]}"; do
        if ! is_service_running "$c"; then
            echo "❌ $c not running – building image..."
            build_and_push_image_for "$c"
        fi
    done
}

# ================================
# 🐇 RabbitMQ
# ================================
wait_for_rabbitmq() {
    echo "⏳ Waiting for RabbitMQ..."
    local container=$(docker ps --filter "name=rabbitmq" --format "{{.Names}}")
    if [[ -z "$container" ]]; then
        echo "❌ RabbitMQ container not found."
        exit 1
    fi
    until docker exec "$container" rabbitmqctl status >/dev/null 2>&1; do sleep 2; done
    echo "✅ RabbitMQ is ready."
}

create_network() {
    docker network ls | grep -q "$NETWORK_NAME" || docker network create "$NETWORK_NAME"
}

start_rabbitmq() {
    echo "📦 Starting RabbitMQ..."
    docker compose -f "$RABBITMQ_DIR/$DOCKER_COMPOSE_FILE" -f "$RABBITMQ_DIR/$DOCKER_COMPOSE_OVERRIDE_FILE" -p rabbitmq up -d
    wait_for_rabbitmq
}

# ================================
# 🚀 Start Layers or Services
# ================================
start_log_layer() {
    bash ./LogLayer/upLogLayer.sh "$@"
}
start_user_layer() {
    bash ./UserLayer/upUserLayer.sh "$@"
}
start_traffic_layer() {
    bash ./TrafficLayer/upTrafficLayer.sh "$@"
}
start_application_layers() {
    start_log_layer
    start_user_layer
    start_traffic_layer
}

# ================================
# 🆘 Help
# ================================
print_help() {
    echo "Usage: ./up.sh [--all] [--log|--user|--traffic] [--service=ServiceName] [--rabbitmq] [--help]"
    echo ""
    echo "  --all            Start all layers and RabbitMQ (default)"
    echo "  --log            Start Log Layer (or use --service)"
    echo "  --user           Start User Layer (or use --service)"
    echo "  --traffic        Start Traffic Layer (or use --service)"
    echo "  --service=Name   Start specific service inside a layer"
    echo "  --rabbitmq       Start only RabbitMQ"
    echo "  --help           Show this message"
}

# ================================
# 🚦 Parse Flags
# ================================
MODE=""
TARGET_SERVICE=""

while [[ "$#" -gt 0 ]]; do
    case "$1" in
        --log) MODE="log" ;;
        --user) MODE="user" ;;
        --traffic) MODE="traffic" ;;
        --all) MODE="all" ;;
        --rabbitmq) MODE="rabbitmq" ;;
        --service=*) TARGET_SERVICE="${1#*=}" ;;
        --help) print_help; exit 0 ;;
        *) echo "❌ Unknown option: $1"; print_help; exit 1 ;;
    esac
    shift
done

# ================================
# 🧩 Main Execution
# ================================
main() {
    if [[ -z "$MODE" ]]; then
        print_help
        exit 0
    fi

    create_network

    case "$MODE" in
        log)
            containers=("log_api_container")
            check_containers_and_build_missing containers
            start_log_layer --service="$TARGET_SERVICE"
            ;;
        user)
            if [[ -n "$TARGET_SERVICE" ]]; then
                case "$TARGET_SERVICE" in
                    UserService) containers=("user_api_container") ;;
                    NotificationService) containers=("notification_api_container") ;;
                    *) containers=("user_api_container" "notification_api_container") ;;
                esac
            else
                containers=("user_api_container" "notification_api_container")
            fi
            check_containers_and_build_missing containers
            start_user_layer --service="$TARGET_SERVICE"
            ;;
        traffic)
            containers=("traffic_data_analytics_api_container")
            check_containers_and_build_missing containers
            start_traffic_layer --service="$TARGET_SERVICE"
            ;;
        all)
            start_rabbitmq
            containers=("log_api_container" "notification_api_container" "traffic_data_analytics_api_container" "user_api_container")
            check_containers_and_build_missing containers
            start_application_layers
            ;;
        rabbitmq)
            start_rabbitmq
            ;;
    esac

    echo "🏁 Services started."
    exit 0
}

main "$@"

