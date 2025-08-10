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
# 🛑 Stop Containers
# ================================
stop_containers() 
{
    echo "🛑 Stopping Pedestrian Detection Service containers..."

    docker compose \
        -f "$PEDESTRIAN_API_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$PEDESTRIAN_API_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -f "$INFLUX_DB_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$INFLUX_DB_DIR/$DOCKER_COMPOSE_OVERRIDE" \
        -p pedestrian_detection_service \
        down

    echo "✅ All Pedestrian Detection Service containers have been stopped."
}

# ================================
# 🔌 Remove Docker Networks
# ================================
remove_networks() 
{
    for network in "${NETWORKS[@]}"; do
        if docker network ls | grep -q "$network"; then
            echo "🔌 Removing Docker network '$network'..."
            docker network rm "$network"
            echo "✅ Network '$network' removed."
        else
            echo "⚠️ Docker network '$network' not found. Skipping."
        fi
    done
}

# ================================
# 🧩 Main
# ================================
main() 
{
    stop_containers
    remove_networks
    exit 0
}

main "$@"
