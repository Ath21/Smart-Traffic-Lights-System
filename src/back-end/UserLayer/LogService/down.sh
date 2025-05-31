#!/bin/bash

NETWORK="log_network"
API="./UserLayer/LogService/LogAPI"
MONGO="./UserLayer/LogService/Mongo"
COMPOSE_FILE="docker-compose.yaml"
COMPOSE_FILE_OVERRIDE="docker-compose.override.yaml"

stop_containers() 
{
    echo "📦  Stopping Log Service containers..."

    docker compose \
        -f "$API/$COMPOSE_FILE" \
        -f "$API/$COMPOSE_FILE_OVERRIDE" \
        -f "$MONGO/$COMPOSE_FILE" \
        -f "$MONGO/$COMPOSE_FILE_OVERRIDE" \
        -p log_service \
        down

    echo "✅  All Log Service containers have been stopped."
}

remove_network() 
{
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
