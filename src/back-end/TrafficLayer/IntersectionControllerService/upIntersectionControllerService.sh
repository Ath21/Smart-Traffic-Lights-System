#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="intersection_network"

INTERSECTION_API_DIR="./TrafficLayer/IntersectionControllerService/IntersectionControllerAPI"

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
    echo "📦 Starting Intersection Controller Service containers..."

    docker compose \
        -f "$INTERSECTION_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$INTERSECTION_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p intersection_controller_service \
        up -d

    echo "✅ Intersection Controller Service containers are running!"
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
