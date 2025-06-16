#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="user_network"

USER_API_DIR="./UserLayer/UserService/UserAPI"
USER_DB_DIR="./UserLayer/UserService/MSSQL"

DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE="docker-compose.override.yaml"

# ================================
# 🛑 Stop User Service Containers
# ================================
stop_containers() 
{
    echo "🛑 Stopping User Service containers..."

    docker compose \
        -f "$USER_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$USER_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -f "$USER_DB_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$USER_DB_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p user_service \
        down

    echo "✅ All User Service containers have been stopped."
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
