#!/bin/bash
set -e

# ================================
# 🔧 CONFIGURATION
# ================================
DOCKER_USERNAME="ath21"
DOCKER_REPO="stls"

# === All services and their Docker build paths ===
declare -A SERVICES_PATHS=(
    # User Layer
    [user_api]="./UserLayer/Services/UserService/Docker"
    [notification_api]="./UserLayer/Services/NotificationService/Docker"

    # Log Layer
    [log_api]="./LogLayer/Services/LogService/Docker"

    # Traffic Layer
    [traffic_analytics_api]="./TrafficLayer/Services/TrafficAnalyticsService/Docker"
    [traffic_light_controller_api]="./TrafficLayer/Services/TrafficLightControllerService/Docker"
    [traffic_light_coordinator_api]="./TrafficLayer/Services/TrafficLightCoordinatorService/Docker"
    [intersection_controller_api]="./TrafficLayer/Services/IntersectionControllerService/Docker"

    # Sensor Layer
    [sensor_api]="./SensorLayer/Services/SensorService/Docker"
    [detection_api]="./SensorLayer/Services/DetectionService/Docker"
)

# ================================
# 🔧 HELPER FUNCTIONS
# ================================
print_help() {
    echo "Usage: ./stls_backend-build.sh [--all] [service...]"
    echo ""
    echo "  --all       Build & push all API images"
    echo "  service...  Build & push specific service(s) by name (e.g. user_api, log_api)"
    echo ""
    echo "Examples:"
    echo "  ./stls_backend-build.sh --all"
    echo "  ./stls_backend-build.sh user_api log_api"
}

build_and_push_image() {
    local service="$1"
    local dir="${SERVICES_PATHS[$service]}"
    local image="$DOCKER_USERNAME/$DOCKER_REPO:$service"

    if [[ -z "$dir" ]]; then
        echo "❌ Unknown service: $service"
        exit 1
    fi

    if [[ -f "$dir/Dockerfile" ]]; then
        echo "🔨 Building image for $service..."
        docker build -t "$image" -f "$dir/Dockerfile" . || {
            echo "❌ Build failed for $service"
            return 1
        }

        echo "📤 Pushing image for $service to Docker Hub..."
        docker push "$image" || {
            echo "⚠️ Push failed for $service"
            return 1
        }
    else
        echo "⚠️ No Dockerfile found for $service in $dir"
    fi
}

# ================================
# 🚀 MAIN EXECUTION
# ================================
main() {
    if [[ $# -eq 0 ]]; then
        print_help
        exit 0
    fi

    if [[ "$1" == "--all" ]]; then
        shift
        echo "🚀 Building & pushing ALL services..."
        for service in "${!SERVICES_PATHS[@]}"; do
            build_and_push_image "$service"
        done
    else
        echo "🚀 Building & pushing selected services: $*"
        for service in "$@"; do
            build_and_push_image "$service"
        done
    fi

    echo ""
    echo "✅ Done!"
}

main "$@"
