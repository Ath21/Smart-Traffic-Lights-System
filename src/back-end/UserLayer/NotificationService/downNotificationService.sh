#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="user_network"

NOTIFICATION_API_DIR="./UserLayer/NotificationService/NotificationAPI"
NOTIFICATION_DB_DIR="./UserLayer/NotificationService/Mongo"

DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE="docker-compose.override.yaml"

# ================================
# 🛑 Stop Notification Service Containers
# ================================
stop_containers() 
{
    echo "🛑 Stopping Notification Service containers..."

    docker compose \
        -f "$NOTIFICATION_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$NOTIFICATION_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -f "$NOTIFICATION_DB_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$NOTIFICATION_DB_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p notification_service \
        down

    echo "✅ All Notification Service containers have been stopped."
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
