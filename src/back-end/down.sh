#!/bin/bash

# ================================
# 🔧 Configuration
# ================================
RABBITMQ_NETWORK="rabbitmq_network"
RABBITMQ_DIR="./RabbitMQ"
DOCKER_COMPOSE_FILE="docker-compose.yaml"
DOCKER_COMPOSE_OVERRIDE_FILE="docker-compose.override.yaml"

# === All services and their Docker build paths ===
declare -A SERVICES_PATHS=(
    # User Layer
    [user_api]="./UserLayer/UserService/UserAPI"
    [notification_api]="./UserLayer/NotificationService/NotificationAPI"

    # Log Layer
    [log_api]="./LogLayer/LogService/LogAPI"

    # Traffic Layer
    [traffic_data_analytics_api]="./TrafficLayer/TrafficDataAnalyticsService/TrafficDataAnalyticsAPI"
    [traffic_light_control_api]="./TrafficLayer/TrafficLightControlService/TrafficLightControlAPI"
    [traffic_light_coordination_api]="./TrafficLayer/TrafficLightCoordinationService/TrafficLightCoordinationAPI"
    [intersection_control_api]="./TrafficLayer/IntersectionControlService/IntersectionControlAPI"

    # Sensor Layer
    [vehicle_detection_api]="./SensorLayer/VehicleDetectionService/VehicleDetectionAPI"
    [emergency_vehicle_detection_api]="./SensorLayer/EmergencyVehicleDetectionService/EmergencyVehicleDetectionAPI"
    [public_transport_detection_api]="./SensorLayer/PublicTransportDetectionService/PublicTransportDetectionAPI"
    [pedestrian_detection_api]="./SensorLayer/PedestrianDetectionService/PedestrianDetectionAPI"
    [cyclist_detection_api]="./SensorLayer/CyclistDetectionService/CyclistDetectionAPI"
    [incident_detection_api]="./SensorLayer/IncidentDetectionService/IncidentDetectionAPI"
)

# ================================
# 🆘 Help
# ================================
print_help() {
    echo "Usage: ./down.sh [--all] [--log|--user|--traffic|--sensor] [--service=ServiceName] [--rabbitmq] [--clean] [--help]"
    echo ""
    echo "  --all                      Stop all layers and RabbitMQ"
    echo "  --log                      Stop Log Layer (or use --service)"
    echo "  --user                     Stop User Layer (or use --service)"
    echo "  --traffic                  Stop Traffic Layer (or use --service)"
    echo "  --sensor                   Stop Sensor Layer (or use --service)"
    echo "  --service=ServiceName      Target specific service inside a layer"
    echo "  --rabbitmq                 Stop only RabbitMQ and remove its network"
    echo "  --clean                    Prune anonymous volumes and dangling images"
    echo "  --help                     Show this message"
}

# ================================
# 🛑 Stop Helpers
# ================================
try_stop_layer() {
    local script="$1"
    shift
    if [ -x "$script" ]; then
        bash "$script" "$@"
    else
        echo "⚠️ Cannot execute $script. Skipping."
    fi
}

stop_log_layer()       { try_stop_layer ./LogLayer/downLogLayer.sh "$@"; }
stop_user_layer()      { try_stop_layer ./UserLayer/downUserLayer.sh "$@"; }
stop_traffic_layer()   { try_stop_layer ./TrafficLayer/downTrafficLayer.sh "$@"; }
stop_sensor_layer()    { try_stop_layer ./SensorLayer/downSensorLayer.sh "$@"; }

stop_all_layers() {
    stop_log_layer
    stop_user_layer
    stop_traffic_layer
    stop_sensor_layer
}

stop_single_service() {
    local target="$1"
    local key="${target,,}_api" # convert to lowercase + "_api"

    if [[ -v SERVICES_PATHS["$key"] ]]; then
        local service_dir="${SERVICES_PATHS[$key]}"
        echo "🛑 Stopping container and volumes for $target..."
        docker compose -f "$service_dir/docker-compose.yaml" down
        echo "✅ $target stopped."
    else
        echo "❌ Unknown service: $target"
        exit 1
    fi
}

# ================================
# 🐇 Stop RabbitMQ
# ================================
stop_rabbitmq() {
    echo "📦 Shutting down RabbitMQ service..."
    docker compose \
        -f "$RABBITMQ_DIR/$DOCKER_COMPOSE_FILE" \
        -f "$RABBITMQ_DIR/$DOCKER_COMPOSE_OVERRIDE_FILE" \
        -p rabbitmq \
        down
    echo "✅ RabbitMQ service stopped."
}

remove_docker_network() {
    if docker network ls | grep -q "$RABBITMQ_NETWORK"; then
        echo "🔌 Removing Docker network '$RABBITMQ_NETWORK'..."
        docker network rm "$RABBITMQ_NETWORK"
        echo "✅ Network removed."
    else
        echo "⚠️ Docker network '$RABBITMQ_NETWORK' not found. Skipping."
    fi
}

# ================================
# 🧹 Docker Cleanup
# ================================
prune_docker_volumes() {
    echo "🧹 Pruning anonymous Docker volumes..."
    docker volume prune -f
    echo "✅ Anonymous volumes removed."
}

prune_dangling_images() {
    echo "🧹 Pruning dangling Docker images..."
    docker image prune -f
    echo "✅ Dangling images removed."
}

# ================================
# 🚦 Parse Arguments & Run
# ================================
main() {
    MODE=""
    TARGET_SERVICE=""
    DO_CLEAN=false

    while [[ "$#" -gt 0 ]]; do
        case "$1" in
            --log) MODE="log" ;;
            --user) MODE="user" ;;
            --traffic) MODE="traffic" ;;
            --sensor) MODE="sensor" ;;
            --all) MODE="all" ;;
            --rabbitmq) MODE="rabbitmq" ;;
            --clean) DO_CLEAN=true ;;
            --service=*) TARGET_SERVICE="${1#*=}" ;;
            --help) print_help; exit 0 ;;
            *) echo "❌ Unknown option: $1"; print_help; exit 1 ;;
        esac
        shift
    done

    if [[ -z "$MODE" && $DO_CLEAN == false ]]; then
        print_help
        exit 0
    fi

    case "$MODE" in
        log)
            if [[ -n "$TARGET_SERVICE" ]]; then
                stop_single_service "$TARGET_SERVICE"
            else
                stop_log_layer
            fi
            ;;
        user)
            if [[ -n "$TARGET_SERVICE" ]]; then
                stop_single_service "$TARGET_SERVICE"
            else
                stop_user_layer
            fi
            ;;
        traffic)
            if [[ -n "$TARGET_SERVICE" ]]; then
                stop_single_service "$TARGET_SERVICE"
            else
                stop_traffic_layer
            fi
            ;;
        sensor)
            if [[ -n "$TARGET_SERVICE" ]]; then
                stop_single_service "$TARGET_SERVICE"
            else
                stop_sensor_layer
            fi
            ;;
        all)
            stop_all_layers
            stop_rabbitmq
            remove_docker_network
            ;;
        rabbitmq)
            stop_rabbitmq
            remove_docker_network
            ;;
    esac

    if $DO_CLEAN; then
        prune_docker_volumes
        prune_dangling_images
    fi

    echo "🏁 Shutdown complete."
    exit 0
}

main "$@"
