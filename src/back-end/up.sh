#!/bin/bash

# ================================
# 🔧 CONFIGURATION
# ================================
NETWORK_NAME="rabbitmq_network"
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
    [traffic_light_coordinator_api]="./TrafficLayer/TrafficLightCoordinatorService/TrafficLightCoordinatorAPI"
    [intersection_control_api]="./TrafficLayer/IntersectionControlService/IntersectionControlAPI"

    # Sensor Layer
    [vehicle_detection_api]="./SensorLayer/VehicleDetectionService/VehicleDetectionAPI"
    [emergency_vehicle_detection_api]="./SensorLayer/EmergencyVehicleDetectionService/EmergencyVehicleDetectionAPI"
    [public_transport_detection_api]="./SensorLayer/PublicTransportDetectionService/PublicTransportDetectionAPI"
    [pedestrian_detection_api]="./SensorLayer/PedestrianDetectionService/PedestrianDetectionAPI"
    [cyclist_detection_api]="./SensorLayer/CyclistDetectionService/CyclistDetectionAPI"
    [incident_detection_api]="./SensorLayer/IncidentDetectionService/IncidentDetectionAPI"
)

DOCKER_USERNAME="ath21"
DOCKER_REPO="stls"

# ================================
# 🔧 HELPER FUNCTIONS
# ================================
print_help() 
{
    echo "Usage: ./up.sh [--all] [--log|--user|--traffic|--sensor] [--service=Name] [--rabbitmq] [--help]"
    echo ""
    echo "  --all            Start all layers and RabbitMQ"
    echo "  --log            Start Log Layer"
    echo "  --user           Start User Layer"
    echo "  --traffic        Start Traffic Layer"
    echo "  --sensor         Start Sensor Layer"
    echo "  --service=Name   Start specific service inside a layer"
    echo "  --rabbitmq       Start only RabbitMQ"
    echo "  --help           Show this message"
}

create_network() 
{
    docker network ls | grep -q "$NETWORK_NAME" || docker network create "$NETWORK_NAME"
}

is_container_running() 
{
    docker ps --format '{{.Names}}' | grep -q "^$1$"
}

build_and_push_image() 
{
    local service="$1"
    local dir="${SERVICES_PATHS[$service]}"
    local image="$DOCKER_USERNAME/$DOCKER_REPO:${service}"

    if [[ -f "$dir/Dockerfile" ]]; then
        echo "🔨 Building image for $service..."
        docker build -t "$image" -f "$dir/Dockerfile" . || {
            echo "❌ Build failed for $service"
            return 1
        }

        echo "📤 (Optional) Pushing image for $service to Docker Hub..."
        docker push "$image" || echo "⚠️ Push failed or skipped for $service"
    else
        echo "⚠️ No Dockerfile found for $service in $dir"
    fi
}

check_and_build_missing_images() 
{
    local services=("$@")
    for service in "${services[@]}"; do
        local container_name="${service}_container"
        if ! is_container_running "$container_name"; then
            echo "❌ $container_name not running — attempting build..."
            build_and_push_image "$service"
        else
            echo "✅ $container_name is already running."
        fi
    done
}

# ================================
# 🐇 RABBITMQ FUNCTIONS
# ================================
start_rabbitmq() 
{
    echo "📦 Starting RabbitMQ..."
    docker compose \
        -f ./RabbitMQ/$DOCKER_COMPOSE_FILE \
        -f ./RabbitMQ/$DOCKER_COMPOSE_OVERRIDE_FILE \
        -p rabbitmq \
        up -d

    wait_for_rabbitmq
}

wait_for_rabbitmq() 
{
    echo "⏳ Waiting for RabbitMQ to be ready..."
    local container
    container=$(docker ps --filter "name=rabbitmq" --format "{{.Names}}")

    if [[ -z "$container" ]]; then
        echo "❌ RabbitMQ container not found."
        exit 1
    fi

    until docker exec "$container" rabbitmqctl status &>/dev/null; do
        sleep 2
    done

    echo "✅ RabbitMQ is ready."
}

# ================================
# 🎯 LAYER STARTERS
# ================================
start_layer() 
{
    local layer_script="$1"
    shift
    if [[ -x "$layer_script" ]]; then
        bash "$layer_script" "$@"
    else
        echo "⚠️ Cannot execute $layer_script"
    fi
}

start_log_layer()     { start_layer ./LogLayer/upLogLayer.sh "$@"; }
start_user_layer()    { start_layer ./UserLayer/upUserLayer.sh "$@"; }
start_traffic_layer() { start_layer ./TrafficLayer/upTrafficLayer.sh "$@"; }
start_sensor_layer()  { start_layer ./SensorLayer/upSensorLayer.sh "$@"; }

start_all_layers() 
{
    start_log_layer
    start_user_layer
    start_traffic_layer
    start_sensor_layer
}

# ================================
# 🚦 ARGUMENT PARSING
# ================================
MODE=""
TARGET_SERVICE=""

while [[ "$#" -gt 0 ]]; do
    case "$1" in
        --log) MODE="log" ;;
        --user) MODE="user" ;;
        --traffic) MODE="traffic" ;;
        --sensor) MODE="sensor" ;;
        --all) MODE="all" ;;
        --rabbitmq) MODE="rabbitmq" ;;
        --service=*) TARGET_SERVICE="${1#*=}" ;;
        --help) print_help; exit 0 ;;
        *) echo "❌ Unknown option: $1"; print_help; exit 1 ;;
    esac
    shift
done

# ================================
# 🚀 MAIN EXECUTION
# ================================
main() 
{
    if [[ -z "$MODE" ]]; then
        print_help
        exit 0
    fi

    create_network

    case "$MODE" in
        log)
            check_and_build_missing_images log_api
            start_log_layer --service="$TARGET_SERVICE"
            ;;

        user)
            case "$TARGET_SERVICE" in
                "") check_and_build_missing_images user_api notification_api ;;
                UserService) check_and_build_missing_images user_api ;;
                NotificationService) check_and_build_missing_images notification_api ;;
                *) echo "❌ Unknown service: $TARGET_SERVICE"; exit 1 ;;
            esac
            start_user_layer --service="$TARGET_SERVICE"
            ;;

        traffic)
            services_to_build=()
            case "$TARGET_SERVICE" in
                "") services_to_build+=(traffic_data_analytics_api traffic_light_control_api traffic_light_coordinator_api intersection_control_api) ;;
                TrafficDataAnalyticsService) services_to_build+=(traffic_data_analytics_api) ;;
                TrafficLightControlService) services_to_build+=(traffic_light_control_api) ;;
                TrafficLightCoordinatorService) services_to_build+=(traffic_light_coordinator_api) ;;
                IntersectionControlService) services_to_build+=(intersection_control_api) ;;
                *) echo "❌ Unknown traffic service: $TARGET_SERVICE"; exit 1 ;;
            esac
            check_and_build_missing_images "${services_to_build[@]}"
            start_traffic_layer --service="$TARGET_SERVICE"
            ;;

        sensor)
            services_to_build=()
            case "$TARGET_SERVICE" in
                "") services_to_build+=(vehicle_detection_api emergency_vehicle_detection_api public_transport_detection_api pedestrian_detection_api cyclist_detection_api incident_detection_api) ;;
                VehicleDetectionService) services_to_build+=(vehicle_detection_api) ;;
                EmergencyVehicleDetectionService) services_to_build+=(emergency_vehicle_detection_api) ;;
                PublicTransportDetectionService) services_to_build+=(public_transport_detection_api) ;;
                PedestrianDetectionService) services_to_build+=(pedestrian_detection_api) ;;
                CyclistDetectionService) services_to_build+=(cyclist_detection_api) ;;
                IncidentDetectionService) services_to_build+=(incident_detection_api) ;;
                *) echo "❌ Unknown sensor service: $TARGET_SERVICE"; exit 1 ;;
            esac
            check_and_build_missing_images "${services_to_build[@]}"
            start_sensor_layer --service="$TARGET_SERVICE"
            ;;

        all)
            start_rabbitmq
            check_and_build_missing_images "${!SERVICES_PATHS[@]}"
            start_all_layers
            ;;

        rabbitmq)
            start_rabbitmq
            ;;
    esac

    echo "🏁 Services started."
}

main
