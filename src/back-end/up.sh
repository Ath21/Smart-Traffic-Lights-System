#!/bin/bash

RABBITMQ_NETWORK="rabbitmq_network"
RABBITMQ="./RabbitMQ"
COMPOSE_FILE="docker-compose.yaml"
COMPOSE_FILE_OVERRIDE="docker-compose.override.yaml"

# Waits until RabbitMQ is ready on port 5672
wait_for_rabbitmq() 
{
    echo "⏳ Waiting for RabbitMQ to be ready on port 5672..."

    RABBITMQ_CONTAINER=$(docker ps --filter "name=rabbitmq" --format "{{.Names}}")

    if [ -z "$RABBITMQ_CONTAINER" ]; then
        echo "❌ RabbitMQ container not found. Exiting."
        exit 1
    fi

    until docker exec "$RABBITMQ_CONTAINER" rabbitmqctl status >/dev/null 2>&1; do
        printf "."
        sleep 2
    done

    echo ""
    echo "✅ RabbitMQ is ready!"
}

create_network() 
{
    if docker network ls | grep -q "$RABBITMQ_NETWORK"; then
        echo "🔄  Docker network '$RABBITMQ_NETWORK' already exists."
    else
        echo "🌐  Creating Docker network '$RABBITMQ_NETWORK'..."
        docker network create "$RABBITMQ_NETWORK"
    fi
}

up_rabbitmq() 
{
    echo "🐇  Bringing up RabbitMQ..."

    docker compose \
        -f "$RABBITMQ/$COMPOSE_FILE" \
        -f "$RABBITMQ/$COMPOSE_FILE_OVERRIDE" \
        -p rabbitmq \
        up -d

    wait_for_rabbitmq
}

up_layers() 
{
    echo "🚀  +++++++ Starting Log Layer... +++++++"
    bash ./LogLayer/up.sh
    
    echo "🚀  +++++++ Starting User Layer... +++++++"
    bash ./UserLayer/up.sh
}

# Run everything
create_network
up_rabbitmq
up_layers

exit 0
