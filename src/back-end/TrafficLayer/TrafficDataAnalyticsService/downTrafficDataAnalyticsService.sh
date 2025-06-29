#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="traffic_data_analytics_network"

TRAFFIC_DATA_API_DIR="./TrafficLayer/TrafficDataAnalyticsService/TrafficDataAnalyticsAPI"
TRAFFIC_DATA_DB_DIR="./TrafficLayer/TrafficDataAnalyticsService/Mongo"
TRAFFIC_DATA_REDIS_DIR="./TrafficLayer/TrafficDataAnalyticsService/Redis"

BUILD_CONTEXT="./TrafficLayer"

DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE="docker-compose.override.yaml"

# ================================
# 🛑 Stop User Service Containers
# ================================
stop_containers() 
{
    echo "🛑 Stopping Traffic Data Analytics Service containers..."

    docker compose \
        -f "$TRAFFIC_DATA_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$TRAFFIC_DATA_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -f "$TRAFFIC_DATA_DB_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$TRAFFIC_DATA_DB_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -f "$TRAFFIC_DATA_REDIS_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$TRAFFIC_DATA_REDIS_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p traffic_data_analytics_service \
        down

    echo "✅ All Traffic Data Analytics Service containers have been stopped."
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
# 🧩 Main Script Execution
# ================================
stop_containers
remove_docker_network

exit 0
