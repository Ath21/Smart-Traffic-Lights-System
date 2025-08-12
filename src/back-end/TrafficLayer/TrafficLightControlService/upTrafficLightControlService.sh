#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="traffic_light_network"

TRAFFIC_LIGHT_API_DIR="./TrafficLayer/TrafficLightControlService/TrafficLightControlAPI"

DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE="docker-compose.override.yaml"

# ================================
# 🌐 Create Docker Networks
# ================================
create_network() 
{
    if docker network ls --format '{{.Name}}' | grep -wq "$NETWORK_NAME"; then
        echo "🔄 Docker network '$NETWORK_NAME' already exists."
    else
        echo "🌐 Creating Docker network '$NETWORK_NAME'..."
        docker network create "$NETWORK_NAME"
        echo "✅ Network '$NETWORK_NAME' created."
    fi
}

# ================================
# 📦 Start Containers
# ================================
start_containers() 
{
    echo "📦 Starting Traffic Light Control Service containers..."

    docker compose \
        -f "$TRAFFIC_LIGHT_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$TRAFFIC_LIGHT_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p traffic_light_control_service \
        up -d

    echo "✅ Traffic Light Control Service containers are running!"
}

# ================================
# 🧩 Main
# ================================
main() 
{
    create_network
    start_containers
    exit 0
}

main "$@"
