#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
NETWORK_NAME="rabbitmq_network"
RABBITMQ_DIR="./RabbitMQ"
DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE_FILE="docker-compose.override.yaml"

# Build Contexts
USER_API_DIR="./UserLayer/UserService/UserAPI"
LOG_API_DIR="./LogLayer/LogService/LogAPI"
BUILD_CONTEXT_USER="."
BUILD_CONTEXT_LOG="."

# Docker Hub Details
DOCKER_USERNAME="ath21"
DOCKER_REPO="stls"
USER_API_TAG="user_api"
LOG_API_TAG="log_api"
USER_API_IMAGE="$DOCKER_USERNAME/$DOCKER_REPO:$USER_API_TAG"
LOG_API_IMAGE="$DOCKER_USERNAME/$DOCKER_REPO:$LOG_API_TAG"

# ================================
# 🕓 Wait for RabbitMQ to be Ready
# ================================
wait_for_rabbitmq() 
{
    echo "⏳ Waiting for RabbitMQ to be ready on port 5672..."

    RABBITMQ_CONTAINER=$(docker ps --filter "name=rabbitmq" --format "{{.Names}}")

    if [ -z "$RABBITMQ_CONTAINER" ]; then
        echo "❌ RabbitMQ container not found. Aborting."
        exit 1
    fi

    until docker exec "$RABBITMQ_CONTAINER" rabbitmqctl status >/dev/null 2>&1; do
        printf "."
        sleep 2
    done

    echo ""
    echo "✅ RabbitMQ is ready!"
}

# ================================
# 🌐 Create Docker Network
# ================================
create_network() 
{
    if docker network ls | grep -q "$NETWORK_NAME"; then
        echo "🔄 Docker network '$NETWORK_NAME' already exists."
    else
        echo "🌐 Creating Docker network '$NETWORK_NAME'..."
        docker network create "$NETWORK_NAME"
    fi
}

# ================================
# 🐇 Launch RabbitMQ Service
# ================================
start_rabbitmq() 
{
    echo "📦 Starting RabbitMQ service..."

    docker compose \
        -f "$RABBITMQ_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$RABBITMQ_DIR/$DOCKER_COMPOSE_OVERRIDE_FILE" \
        -p rabbitmq \
        up -d

    wait_for_rabbitmq
}

# ================================
# 🔨 Build & Push Docker Images
# ================================
build_and_push_images() 
{
    echo "🔨 Building Docker image: $USER_API_IMAGE"
    docker build -t "$USER_API_IMAGE" -f "$USER_API_DIR/Dockerfile" "$BUILD_CONTEXT_USER"

    echo "🚀 Pushing image to Docker Hub: $USER_API_IMAGE"
    docker push "$USER_API_IMAGE"
    echo "✅ Successfully pushed: $USER_API_IMAGE"

    echo "🔨 Building Docker image: $LOG_API_IMAGE"
    docker build -t "$LOG_API_IMAGE" -f "$LOG_API_DIR/Dockerfile" "$BUILD_CONTEXT_LOG"

    echo "🚀 Pushing image to Docker Hub: $LOG_API_IMAGE"
    docker push "$LOG_API_IMAGE"
    echo "✅ Successfully pushed: $LOG_API_IMAGE"
}

# ================================
# 🔄 Start Application Layers
# ================================
start_application_layers() 
{
    echo "🚀 Starting Log Layer..."
    bash ./LogLayer/upLogLayer.sh

    echo "🚀 Starting User Layer..."
    bash ./UserLayer/upUserLayer.sh
}

# ================================
# 🧩 Main Script Execution
# ================================
create_network
start_rabbitmq
build_and_push_images
start_application_layers

echo "🏁 All services started successfully!"
exit 0
