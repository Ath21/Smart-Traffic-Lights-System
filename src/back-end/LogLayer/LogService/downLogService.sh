#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="log_network"

LOG_API_DIR="./LogLayer/LogService/LogAPI"
LOG_DB_DIR="./LogLayer/LogService/Mongo"

DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE="docker-compose.override.yaml"

# ================================
# 🛑 Stop Log Service Containers
# ================================
stop_containers() {
    echo "🛑 Stopping Log Service containers..."

    docker compose \
        -f "$LOG_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$LOG_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -f "$LOG_DB_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$LOG_DB_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p log_service \
        down

    echo "✅ All Log Service containers have been stopped."
}

# ================================
# 🔌 Remove Docker Network
# ================================
remove_docker_network() {
    if docker network ls | grep -q "$NETWORK_NAME"; then
        echo "🔌 Removing Docker network '$NETWORK_NAME'..."
        docker network rm "$NETWORK_NAME"
        echo "✅ Network removed."
    else
        echo "⚠️ Docker network '$NETWORK_NAME' not found. Skipping."
    fi
}

# ================================
# 🧩 Main
# ================================
main() {
    stop_containers
    remove_docker_network
    exit 0
}

main "$@"
