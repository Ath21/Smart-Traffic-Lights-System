#!/bin/bash

set -e  # Exit on any error

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="traffic_data_analytics_network"

TRAFFIC_DATA_API_DIR="./TrafficLayer/TrafficDataAnalyticsService/TrafficDataAnalyticsAPI"
TRAFFIC_DATA_DB_DIR="./TrafficLayer/TrafficDataAnalyticsService/Mongo"
TRAFFIC_DATA_REDIS_DIR="./TrafficLayer/TrafficDataAnalyticsService/Redis"

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
    echo "📦 Starting Traffic Data Analytics Service containers..."

    docker compose \
        -f "$TRAFFIC_DATA_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$TRAFFIC_DATA_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -f "$TRAFFIC_DATA_DB_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$TRAFFIC_DATA_DB_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -f "$TRAFFIC_DATA_REDIS_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$TRAFFIC_DATA_REDIS_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p traffic_data_analytics_service \
        up -d

    echo "✅ Traffic Data Analytics Service containers are running!"
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
