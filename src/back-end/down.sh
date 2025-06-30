#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
RABBITMQ_NETWORK="rabbitmq_network"
RABBITMQ_DIR="../back-end/RabbitMQ"
DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE_FILE="docker-compose.override.yaml"

# ================================
# ⛔ Stop Application Layers
# ================================
stop_application_layers() 
{
    echo "🛑 Stopping Log Layer..."
    bash ./LogLayer/downLogLayer.sh

    echo "🛑 Stopping User Layer..."
    bash ./UserLayer/downUserLayer.sh

    echo "🛑 Stopping Traffic Layer..."
    bash ./TrafficLayer/downTrafficLayer.sh
}

# ================================
# 🐇 Stop RabbitMQ Service
# ================================
stop_rabbitmq() 
{
    echo "📦 Shutting down RabbitMQ service..."

    docker compose \
        -f "$RABBITMQ_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$RABBITMQ_DIR/$DOCKER_COMPOSE_OVERRIDE_FILE" \
        -p rabbitmq \
        down

    echo "✅ RabbitMQ service stopped."
}

# ================================
# 🔌 Remove Docker Network
# ================================
remove_docker_network() 
{
    if docker network ls | grep -q "$RABBITMQ_NETWORK"; then
        echo "🔌 Removing Docker network '$RABBITMQ_NETWORK'..."
        docker network rm "$RABBITMQ_NETWORK"
        echo "✅ Network removed."
    else
        echo "⚠️ Docker network '$RABBITMQ_NETWORK' not found. Skipping."
    fi
}

# ================================
# 🧹 Prune Anonymous Volumes
# ================================
prune_docker_volumes() 
{
    echo "🧹 Pruning anonymous Docker volumes..."
    docker volume prune -f
    echo "✅ Anonymous volumes removed."
}

# ================================
# 🧩 Main Script Execution
# ================================
stop_application_layers
stop_rabbitmq
remove_docker_network
prune_docker_volumes

echo "🏁 All services and resources have been stopped and cleaned."
exit 0
