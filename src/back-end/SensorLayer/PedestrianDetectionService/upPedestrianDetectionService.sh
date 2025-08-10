#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
NETWORKS=("detection_network" "influxdb_network")

PEDESTRIAN_API_DIR="./SensorLayer/PedestrianDetectionService/PedestrianDetectionAPI"
INFLUX_DB_DIR="./SensorLayer/InfluxDb"

DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE="docker-compose.override.yaml"

# ================================
# 🌐 Create Docker Networks
# ================================
create_networks() 
{
    for network in "${NETWORKS[@]}"; do
        if docker network ls | grep -q "$network"; then
            echo "🔄 Docker network '$network' already exists."
        else
            echo "🌐 Creating Docker network '$network'..."
            docker network create "$network"
        fi
    done
}

# ================================
# 📦 Start Containers
# ================================
start_containers() 
{
    echo "📦 Starting Pedestrian Detection Service containers..."

    docker compose \
        -f "$PEDESTRIAN_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$PEDESTRIAN_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -f "$INFLUX_DB_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$INFLUX_DB_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p pedestrian_detection_service \
        up -d

    echo "✅ Pedestrian Detection Service containers are running!"
}

# ================================
# 🧩 Main
# ================================
main() 
{
    create_networks
    start_containers
    exit 0
}

main "$@"
