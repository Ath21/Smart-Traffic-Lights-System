#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="intersection_network"

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
remove_network() 
{
    if docker network ls --format '{{.Name}}' | grep -wq "$NETWORK_NAME"; then
        echo "🔌 Removing Docker network '$NETWORK_NAME'..."
        docker network rm "$NETWORK_NAME"
        echo "✅ Network '$NETWORK_NAME' removed."
    else
        echo "⚠️ Docker network '$NETWORK_NAME' not found. Skipping."
    fi
}

# ================================
# 🧩 Main
# ================================
main() 
{
    stop_containers
    remove_network
    exit 0
}

main "$@"
