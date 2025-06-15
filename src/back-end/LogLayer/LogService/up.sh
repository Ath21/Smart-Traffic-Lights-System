#!/bin/bash

NETWORK="log_network"
API="./LogLayer/LogService/LogAPI"
MONGO="./LogLayer/LogService/Mongo"
BUILD_CONTEXT="./LogLayer"
COMPOSE_FILE="docker-compose.yaml"
COMPOSE_FILE_OVERRIDE="docker-compose.override.yaml"

DOCKER_USERNAME="ath21"
REPO="stls"
TAG="log_api"
IMAGE_NAME="$DOCKER_USERNAME/$REPO:$TAG"

create_network() 
{
    if docker network ls | grep -q "$NETWORK"; then
        echo "🔄  Docker network '$NETWORK' already exists."
    else
        echo "🌐  Creating Docker network '$NETWORK'..."
        docker network create "$NETWORK"
    fi
}

build_and_push_image()
{
    echo "🔨  Building Docker image: $IMAGE_NAME ..."
    docker build -t "$IMAGE_NAME" -f "$API/Dockerfile" "$BUILD_CONTEXT"

    echo "🚀  Pushing image to Docker Hub..."
    docker push "$IMAGE_NAME"

    echo "✅  Image pushed: $IMAGE_NAME"
}

up_containers() 
{
    echo "📦  Bringing up Log Service containers..."

    docker compose \
        -f "$API/$COMPOSE_FILE" \
        -f "$API/$COMPOSE_FILE_OVERRIDE" \
        -f "$MONGO/$COMPOSE_FILE" \
        -f "$MONGO/$COMPOSE_FILE_OVERRIDE" \
        -p log_service \
        up -d

    echo "✅  Log Service containers are running!"
}

create_network
build_and_push_image
up_containers

exit 0
