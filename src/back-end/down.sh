#!/bin/bash

RABBITMQ_NETWORK="rabbitmq_network"
RABBITMQ="../back-end/RabbitMQ"
COMPOSE_FILE="docker-compose.yaml"
COMPOSE_FILE_OVERRIDE="docker-compose.override.yaml"

down_layers() 
{
    echo "🛑  ------- Stopping Log Layer... -------"
    bash ./LogLayer/down.sh

    echo "🛑  ------- Stopping User Layer... -------"
    bash ./UserLayer/down.sh
}

stop_rabbitmq() 
{
    echo "🐇  Shutting down RabbitMQ..."

    docker compose \
        -f "$RABBITMQ/$COMPOSE_FILE" \
        -f "$RABBITMQ/$COMPOSE_FILE_OVERRIDE" \
        -p rabbitmq \
        down

    echo "✅  RabbitMQ has been stopped."
}

remove_network() 
{
    if docker network ls | grep -q "$RABBITMQ_NETWORK"; then
        echo "🔌  Removing Docker network '$RABBITMQ_NETWORK'..."
        docker network rm "$RABBITMQ_NETWORK"
    else
        echo "⚠️  Docker network '$RABBITMQ_NETWORK' does not exist. Skipping removal."
    fi
}

prune_volumes()
{
    echo "🧹  Pruning anonymous Docker volumes..."
    docker volume prune -f
    echo "✅  Anonymous volumes removed."
}

down_layers
stop_rabbitmq
remove_network
prune_volumes

exit 0
