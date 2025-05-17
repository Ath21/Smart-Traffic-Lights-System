#!/bin/bash

NETWORK="user_network"
API="./UserLayer/UserService/UserAPI"
MSSQL="./UserLayer/UserService/MSSQL"
COMPOSE_FILE="docker-compose.yaml"
COMPOSE_FILE_OVERRIDE="docker-compose.override.yaml"

stop_containers() {
    echo "📦  Stopping User Service containers..."

    docker compose \
        -f "$API/$COMPOSE_FILE" \
        -f "$API/$COMPOSE_FILE_OVERRIDE" \
        -f "$MSSQL/$COMPOSE_FILE" \
        -f "$MSSQL/$COMPOSE_FILE_OVERRIDE" \
        -p user_service \
        down

    echo "✅  All User Service containers have been stopped."
}

remove_network() {
    if docker network ls | grep -q "$NETWORK"; then
        echo "🔌  Removing Docker network '$NETWORK'..."
        docker network rm "$NETWORK"
    else
        echo "⚠️  Docker network '$NETWORK' does not exist. Skipping removal."
    fi
}

stop_containers
remove_network

exit 0
