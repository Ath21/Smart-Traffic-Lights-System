#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
NETWORK="intersection_network"

INTERSECTION_API_DIR="./TrafficLayer/IntersectionControlService/IntersectionControlAPI"

DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE="docker-compose.override.yaml"

# ================================
# 🌐 Create Docker Networks
# ================================
create_networks() 
{
    for network in "${NETWORKS[@]}"; do
        if docker network ls | grep -q "$network"; then
            echo "🔄 Docker network '$network' already exists."
        else
            echo "🌐 Creating Docker network '$network'..."
            docker network create "$network"
        fi
    done
}

# ================================
# 📦 Start Containers
# ================================
start_containers() 
{
    echo "📦 Starting Intersection Control Service containers..."

    docker compose \
        -f "$INTERSECTION_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$INTERSECTION_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p intersection_control_service \
        up -d

    echo "✅ Intersection Control Service containers are running!"
}

# ================================
# 🧩 Main
# ================================
main() 
{
    create_networks
    start_containers
    exit 0
}

main "$@"
