#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
NETWORK="intersection_network"

INTERSECTION_API_DIR="./TrafficLayer/IntersectionControlService/IntersectionControlAPI"

DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE="docker-compose.override.yaml"

# ================================
# 🛑 Stop Containers
# ================================
stop_containers() 
{
    echo "🛑 Stopping Intersection Control Service containers..."

    docker compose \
        -f "$INTERSECTION_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$INTERSECTION_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p intersection_control_service \
        down

    echo "✅ All Intersection Control Service containers have been stopped."
}

# ================================
# 🔌 Remove Docker Networks
# ================================
remove_networks() 
{
    for network in "${NETWORKS[@]}"; do
        if docker network ls | grep -q "$network"; then
            echo "🔌 Removing Docker network '$network'..."
            docker network rm "$network"
            echo "✅ Network '$network' removed."
        else
            echo "⚠️ Docker network '$network' not found. Skipping."
        fi
    done
}

# ================================
# 🧩 Main
# ================================
main() 
{
    stop_containers
    remove_networks
    exit 0
}

main "$@"
