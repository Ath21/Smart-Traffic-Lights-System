#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="user_network"

USER_API_DIR="./UserLayer/UserService/UserAPI"
USER_DB_DIR="./UserLayer/UserService/MSSQL"

BUILD_CONTEXT="./UserLayer"

DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE="docker-compose.override.yaml"

# ================================
# 🌐 Create Docker Network
# ================================
create_network() {
    if docker network ls | grep -q "$NETWORK_NAME"; then
        echo "🔄 Docker network '$NETWORK_NAME' already exists."
    else
        echo "🌐 Creating Docker network '$NETWORK_NAME'..."
        docker network create "$NETWORK_NAME"
    fi
}

# ================================
# 📦 Start User Service Containers
# ================================
start_containers() {
    echo "📦 Starting User Service containers..."

    docker compose \
        -f "$USER_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$USER_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -f "$USER_DB_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$USER_DB_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p user_service \
        up -d

    echo "✅ User Service containers are running!"
}

# ================================
# 🧩 Main
# ================================
main() {
    create_network
    start_containers
    exit 0
}

main "$@"
