#!/bin/bash

set -e  # Exit immediately on error

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
# 🌐 Ensure Docker Networks Exist
# ================================
ensure_network() {
    local net="$1"
    if docker network ls --format '{{.Name}}' | grep -wq "$net"; then
        echo "🔄 Docker network '$net' already exists."
    else
        echo "🌐 Creating Docker network '$net'..."
        docker network create "$net"
        echo "✅ Network '$net' created."
    fi
}

create_networks() {
    ensure_network "$NETWORK_COORD"
    ensure_network "$NETWORK_RMQ"
}

# ================================
# 🚀 Start Coordinator Service
# ================================
start_containers() {
    echo "📦 Starting Traffic Light Coordinator containers..."

    docker compose \
        -f "$COORD_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$COORD_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -f "$COORD_DB_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$COORD_DB_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p "$PROJECT_NAME" \
        up -d

    echo "✅ Traffic Light Coordinator containers are running!"
}

# ================================
# 🧩 Main
# ================================
main() {
    create_networks
    start_containers
    echo "🏁 Coordinator Service startup complete."
}

main "$@"
