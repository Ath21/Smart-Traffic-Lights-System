#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
RABBITMQ_NETWORK="rabbitmq_network"
RABBITMQ_DIR="./RabbitMQ"
DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE_FILE="docker-compose.override.yaml"

# ================================
# 🛑 Safe Layer Stop
# ================================
try_stop_layer() {
    local script="$1"
    shift
    if [ -x "$script" ]; then
        bash "$script" "$@"
    else
        echo "⚠️ Cannot execute $script. Skipping."
    fi
}

stop_log_layer() {
    echo "🛑 Stopping Log Layer..."
    try_stop_layer ./LogLayer/downLogLayer.sh "$@"
}

stop_user_layer() {
    echo "🛑 Stopping User Layer..."
    try_stop_layer ./UserLayer/downUserLayer.sh "$@"
}

stop_traffic_layer() {
    echo "🛑 Stopping Traffic Layer..."
    try_stop_layer ./TrafficLayer/downTrafficLayer.sh "$@"
}

stop_application_layers() {
    stop_log_layer
    stop_user_layer
    stop_traffic_layer
}

# ================================
# 🐇 Stop RabbitMQ
# ================================
stop_rabbitmq() {
    echo "📦 Shutting down RabbitMQ service..."
    docker compose \
        -f "$RABBITMQ_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$RABBITMQ_DIR/$DOCKER_COMPOSE_OVERRIDE_FILE" \
        -p rabbitmq \
        down
    echo "✅ RabbitMQ service stopped."
}

remove_docker_network() {
    if docker network ls | grep -q "$RABBITMQ_NETWORK"; then
        echo "🔌 Removing Docker network '$RABBITMQ_NETWORK'..."
        docker network rm "$RABBITMQ_NETWORK"
        echo "✅ Network removed."
    else
        echo "⚠️ Docker network '$RABBITMQ_NETWORK' not found. Skipping."
    fi
}

# ================================
# 🧹 Cleanup
# ================================
prune_docker_volumes() {
    echo "🧹 Pruning anonymous Docker volumes..."
    docker volume prune -f
    echo "✅ Anonymous volumes removed."
}

prune_dangling_images() {
    echo "🧹 Pruning dangling Docker images..."
    docker image prune -f
    echo "✅ Dangling images removed."
}

# ================================
# 🆘 Help
# ================================
print_help() {
    echo "Usage: ./down.sh [--all] [--log|--user|--traffic] [--service=ServiceName] [--rabbitmq] [--clean] [--help]"
    echo ""
    echo "  --all                      Stop all layers and RabbitMQ"
    echo "  --log                      Stop Log Layer (or use --service)"
    echo "  --user                     Stop User Layer (or use --service)"
    echo "  --traffic                  Stop Traffic Layer (or use --service)"
    echo "  --service=ServiceName      Target specific service inside a layer"
    echo "  --rabbitmq                 Stop only RabbitMQ and remove its network"
    echo "  --clean                    Prune anonymous volumes and dangling images"
    echo "  --help                     Show this message"
}

# ================================
# 🚦 Parse & Execute in Main
# ================================
main() {
    MODE=""
    DO_CLEAN=false
    TARGET_SERVICE=""

    while [[ "$#" -gt 0 ]]; do
        case "$1" in
            --log) MODE="log" ;;
            --user) MODE="user" ;;
            --traffic) MODE="traffic" ;;
            --all) MODE="all" ;;
            --rabbitmq) MODE="rabbitmq" ;;
            --clean) DO_CLEAN=true ;;
            --service=*) TARGET_SERVICE="${1#*=}" ;;
            --help) print_help; exit 0 ;;
            *) echo "❌ Unknown option: $1"; print_help; exit 1 ;;
        esac
        shift
    done

    if [[ "$MODE" == "" && "$DO_CLEAN" == false ]]; then
        print_help
        exit 0
    fi

    case "$MODE" in
        log) stop_log_layer "--service=$TARGET_SERVICE" ;;
        user) stop_user_layer "--service=$TARGET_SERVICE" ;;
        traffic) stop_traffic_layer "--service=$TARGET_SERVICE" ;;
        all)
            stop_application_layers
            if docker ps -a --format '{{.Names}}' | grep -q "rabbitmq"; then
                stop_rabbitmq
            else
                echo "⚠️ RabbitMQ container not found. Skipping shutdown."
            fi
            remove_docker_network
            ;;
        rabbitmq)
            if docker ps -a --format '{{.Names}}' | grep -q "rabbitmq"; then
                stop_rabbitmq
            else
                echo "⚠️ RabbitMQ container not found. Skipping shutdown."
            fi
            remove_docker_network
            ;;
    esac

    if $DO_CLEAN; then
        prune_docker_volumes
        prune_dangling_images
    fi

    echo "🏁 Shutdown complete."
    exit 0
}

main "$@"
