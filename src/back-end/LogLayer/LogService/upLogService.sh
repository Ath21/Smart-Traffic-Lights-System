#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="log_network"

LOG_API_DIR="./LogLayer/LogService/LogAPI"
LOG_DB_DIR="./LogLayer/LogService/Mongo"

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
# 📦 Start Log Service Containers
# ================================
start_containers() 
{
    echo "📦 Starting Log Service containers..."

    docker compose \
        -f "$LOG_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$LOG_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -f "$LOG_DB_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$LOG_DB_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p log_service \
        up -d

    echo "✅ Log Service containers are running!"
}

# ================================
# 🧩 Main
# ================================
main() 
{
    create_network
    start_containers
    exit 0
}

main "$@"
