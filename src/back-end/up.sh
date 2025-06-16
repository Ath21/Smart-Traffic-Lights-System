#!/bin/bash

RABBITMQ_NETWORK="rabbitmq_network"
RABBITMQ="./RabbitMQ"
COMPOSE_FILE="docker-compose.yaml"
COMPOSE_FILE_OVERRIDE="docker-compose.override.yaml"


# 🔧 Context πάει ένα επίπεδο πάνω για να περιλαμβάνει και το UserMessages
API1="./UserLayer/UserService/UserAPI"
API2="./LogLayer/LogService/LogAPI"

BUILD_CONTEXT1="."
BUILD_CONTEXT2="."


DOCKER_USERNAME="ath21"
REPO="stls"
TAG1="user_api"
TAG2="log_api"
IMAGE_NAME1="$DOCKER_USERNAME/$REPO:$TAG1"
IMAGE_NAME2="$DOCKER_USERNAME/$REPO:$TAG2"

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

build_and_push_image()
{
    echo "🔨  Building Docker image: $IMAGE_NAME1 ..."
    docker build -t "$IMAGE_NAME1" -f "$API1/Dockerfile" "$BUILD_CONTEXT1"

    echo "🚀  Pushing image to Docker Hub..."
    docker push "$IMAGE_NAME1"
    echo "✅  Image pushed: $IMAGE_NAME1"

    
    
    echo "🔨  Building Docker image: $IMAGE_NAME2 ..."
    docker build -t "$IMAGE_NAME2" -f "$API2/Dockerfile" "$BUILD_CONTEXT2"

    echo "🚀  Pushing image to Docker Hub..."
    docker push "$IMAGE_NAME2"
    echo "✅  Image pushed: $IMAGE_NAME2"
}


# Run everything
create_network
up_rabbitmq
build_and_push_image
up_layers

exit 0
