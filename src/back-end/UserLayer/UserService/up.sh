#!/bin/bash

NETWORK="user_network"
RABBITMQ_NETWORK="rabbitmq_network"
API="./UserLayer/UserService/UserAPI"
MSSQL="./UserLayer/UserService/MSSQL"
RABBITMQ="./UserLayer/UserService/RabbitMQ"
COMPOSE_FILE="docker-compose.yaml"
COMPOSE_FILE_OVERRIDE="docker-compose.override.yaml"

create_network() {
    if docker network ls | grep -q "$NETWORK"; then
        echo "🔄  Docker network '$NETWORK' already exists."
    else
        echo "🌐  Creating Docker network '$NETWORK'..."
        docker network create "$NETWORK"
    fi

    if docker network ls | grep -q "$RABBITMQ_NETWORK"; then
        echo "🔄  Docker network '$RABBITMQ_NETWORK' already exists."
    else
        echo "🌐  Creating Docker network '$RABBITMQ_NETWORK'..."
        docker network create "$RABBITMQ_NETWORK"
    fi
}

up_containers() {
    echo "📦  Bringing up User Service containers..."

    docker compose \
        -f "$API/$COMPOSE_FILE" \
        -f "$API/$COMPOSE_FILE_OVERRIDE" \
        -f "$MSSQL/$COMPOSE_FILE" \
        -f "$MSSQL/$COMPOSE_FILE_OVERRIDE" \
        -f "$RABBITMQ/$COMPOSE_FILE" \
        -f "$RABBITMQ/$COMPOSE_FILE_OVERRIDE" \
        -p user_service \
        up -d

    echo "✅  User Service containers are running!"
}

create_network
up_containers

exit 0