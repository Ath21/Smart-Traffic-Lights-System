#!/bin/bash

set -e  # Exit immediately if a command exits with a non-zero status

# ================================
# 🔧 Configuration
# ================================
NETWORK_COORD="traffic_light_coordinator_network"
NETWORK_RMQ="rabbitmq_network"

COORD_API_DIR="./TrafficLayer/TrafficLightCoordinatorService/TrafficLightCoordinatorAPI"
COORD_DB_DIR="./TrafficLayer/TrafficLightCoordinatorService/PostgreSQL"

DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE="docker-compose.override.yaml"

PROJECT_NAME="traffic_light_coordinator_service"

# ================================
# 🛑 Stop Coordinator Service
# ================================
stop_containers() {
    echo "🛑 Stopping Traffic Light Coordinator containers..."

    docker compose \
        -f "$COORD_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$COORD_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -f "$COORD_DB_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$COORD_DB_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p "$PROJECT_NAME" \
        down

    echo "✅ All Coordinator containers have been stopped."
}

# ================================
# 🔌 Remove Coordinator Network
# (Do NOT remove rabbitmq_network; it may be shared)
# ================================
remove_coord_network() {
    if docker network ls --format '{{.Name}}' | grep -wq "$NETWORK_COORD"; then
        echo "🔌 Removing Docker network '$NETWORK_COORD'..."
        docker network rm "$NETWORK_COORD"
        echo "✅ Network '$NETWORK_COORD' removed."
    else
        echo "⚠️ Docker network '$NETWORK_COORD' not found. Skipping."
    fi
}

# ================================
# 🧩 Main
# ================================
main() {
    stop_containers
    remove_coord_network
    echo "🏁 Coordinator Service shutdown complete."
}

main "$@"
