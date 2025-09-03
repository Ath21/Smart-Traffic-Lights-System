#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="intersection_network"

TRAFFIC_LIGHT_API_DIR="./TrafficLayer/TrafficLightControllerService/TrafficLightControllerAPI"

DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE="docker-compose.override.yaml"

# ================================
# 🛑 Stop Containers
# ================================
stop_containers() 
{
    echo "🛑 Stopping Traffic Light Controller Service containers..."

    docker compose \
        -f "$TRAFFIC_LIGHT_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$TRAFFIC_LIGHT_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p traffic_light_controller_service \
        down

    echo "✅ All Traffic Light Controller Service containers have been stopped."
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
