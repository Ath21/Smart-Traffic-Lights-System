#!/bin/bash

NETWORK="user_network"

API="./UserLayer/UserService/UserAPI"
MSSQL="./UserLayer/UserService/MSSQL"

# 🔧 Context πάει ένα επίπεδο πάνω για να περιλαμβάνει και το UserMessages
BUILD_CONTEXT="./UserLayer"

COMPOSE_FILE="docker-compose.yaml"
COMPOSE_FILE_OVERRIDE="docker-compose.override.yaml"

DOCKER_USERNAME="ath21"
REPO="stls"
TAG="user_api"
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
    echo "📦  Bringing up User Service containers..."

    docker compose \
        -f "$API/$COMPOSE_FILE" \
        -f "$API/$COMPOSE_FILE_OVERRIDE" \
        -f "$MSSQL/$COMPOSE_FILE" \
        -f "$MSSQL/$COMPOSE_FILE_OVERRIDE" \
        -p user_service \
        up -d

    echo "✅  User Service containers are running!"
}

create_network
#build_and_push_image
up_containers

exit 0
