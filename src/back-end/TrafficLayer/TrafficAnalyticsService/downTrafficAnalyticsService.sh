#!/bin/bash

set -e  # Exit immediately if a command exits with a non-zero status

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="traffic_analytics_network"

TRAFFIC_ANALYTICS_API_DIR="./TrafficLayer/TrafficAnalyticsService/TrafficAnalyticsAPI"
TRAFFIC_ANALYTICS_DB_DIR="./TrafficLayer/TrafficAnalyticsService/PostgreSQL"

DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE="docker-compose.override.yaml"

# ================================
# 🛑 Stop Traffic Analytics Service Containers
# ================================
stop_containers() 
{
    echo "🛑 Stopping Traffic Analytics Service containers..."

    docker compose \
        -f "$TRAFFIC_ANALYTICS_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$TRAFFIC_ANALYTICS_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -f "$TRAFFIC_ANALYTICS_DB_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$TRAFFIC_ANALYTICS_DB_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p traffic_analytics_service \
        down

    echo "✅ All Traffic Analytics Service containers have been stopped."
}

# ================================
# 🔌 Remove Docker Network
# ================================
remove_docker_network() 
{
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
main() 
{
    stop_containers
    remove_docker_network
}

main "$@"
