#!/bin/bash

set -e  # Exit on any error

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="notification_network"

NOTIFICATION_API_DIR="./UserLayer/NotificationService/NotificationAPI"
NOTIFICATION_DB_DIR="./UserLayer/NotificationService/Mongo"

DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE="docker-compose.override.yaml"

# ================================
# 🌐 Create Docker Network
# ================================
create_network() 
{
    if docker network ls --format '{{.Name}}' | grep -wq "$NETWORK_NAME"; then
        echo "🔄 Docker network '$NETWORK_NAME' already exists."
    else
        echo "🌐 Creating Docker network '$NETWORK_NAME'..."
        docker network create "$NETWORK_NAME"
    fi
}

# ================================
# 📦 Start Notification Service Containers
# ================================
start_containers() 
{
    echo "📦 Starting Notification Service containers..."

    docker compose \
        -f "$NOTIFICATION_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$NOTIFICATION_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -f "$NOTIFICATION_DB_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$NOTIFICATION_DB_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p notification_service \
        up -d

    echo "✅ Notification Service containers are running!"
}

# ================================
# 🧩 Main
# ================================
main() 
{
    create_network
    start_containers
    echo "🏁 Notification Service started."
}

main "$@"
