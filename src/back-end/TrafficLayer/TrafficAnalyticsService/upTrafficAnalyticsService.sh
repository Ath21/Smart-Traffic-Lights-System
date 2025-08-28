#!/bin/bash

set -e  # Exit on any error

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="traffic_analytics_network"

TRAFFIC_ANALYTICS_API_DIR="./TrafficLayer/TrafficAnalyticsService/TrafficAnalyticsAPI"
TRAFFIC_ANALYTICS_DB_DIR="./TrafficLayer/TrafficAnalyticsService/PostgreSQL"

DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE="docker-compose.override.yaml"

# ================================
# 🌐 Create Docker Network
# ================================
create_network() 
{
    if docker network ls | grep -q "$NETWORK_NAME"; then
        echo "🔄 Docker network '$NETWORK_NAME' already exists."
    else
        echo "🌐 Creating Docker network '$NETWORK_NAME'..."
        docker network create "$NETWORK_NAME"
    fi
}

# ================================
# 📦 Start Containers
# ================================
start_containers() 
{
    echo "📦 Starting Traffic Analytics Service containers..."

    docker compose \
        -f "$TRAFFIC_ANALYTICS_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$TRAFFIC_ANALYTICS_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -f "$TRAFFIC_ANALYTICS_DB_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$TRAFFIC_ANALYTICS_DB_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p traffic_analytics_service \
        up -d

    echo "✅ Traffic Analytics Service containers are running!"
}

# ================================
# 🧩 Main
# ================================
main() 
{
    create_network
    start_containers
}

main "$@"
