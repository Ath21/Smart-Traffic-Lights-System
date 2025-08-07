#!/bin/bash

set -e  # Exit immediately on error

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="user_network"

USER_API_DIR="./UserLayer/UserService/UserAPI"
USER_DB_DIR="./UserLayer/UserService/MSSQL"

DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE="docker-compose.override.yaml"

# ================================
# 🌐 Ensure Docker Network Exists
# ================================
create_network() 
{
    if docker network ls --format '{{.Name}}' | grep -wq "$NETWORK_NAME"; then
        echo "🔄 Docker network '$NETWORK_NAME' already exists."
    else
        echo "🌐 Creating Docker network '$NETWORK_NAME'..."
        docker network create "$NETWORK_NAME"
        echo "✅ Network '$NETWORK_NAME' created."
    fi
}

# ================================
# 🚀 Start User Service Containers
# ================================
start_containers()
{
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
main() 
{
    create_network
    start_containers
    echo "🏁 User Service startup complete."
}

main "$@"
