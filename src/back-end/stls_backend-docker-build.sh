#!/bin/bash
set -e

# ================================
# 🔧 CONFIGURATION
# ================================
ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"   # always src/back-end
DOCKER_USERNAME="ath21"
DOCKER_REPO="stls"

# === All services and their Dockerfile paths (relative to ROOT_DIR) ===
declare -A SERVICES_PATHS=(
    # User Layer
    [user_api]="UserLayer/Services/UserService/Docker/Dockerfile"
    [notification_api]="UserLayer/Services/NotificationService/Docker/Dockerfile"

    # Log Layer
    [log_api]="LogLayer/Services/LogService/Docker/Dockerfile"

    # Traffic Layer
    [traffic_analytics_api]="TrafficLayer/Services/TrafficAnalyticsService/Docker/Dockerfile"
    [traffic_light_controller_api]="TrafficLayer/Services/TrafficLightControllerService/Docker/Dockerfile"
    [traffic_light_coordinator_api]="TrafficLayer/Services/TrafficLightCoordinatorService/Docker/Dockerfile"
    [intersection_controller_api]="TrafficLayer/Services/IntersectionControllerService/Docker/Dockerfile"

    # Sensor Layer
    [sensor_api]="SensorLayer/Services/SensorService/Docker/Dockerfile"
    [detection_api]="SensorLayer/Services/DetectionService/Docker/Dockerfile"
)

# ================================
# 🔧 HELPER FUNCTIONS
# ================================
print_help() {
    echo "Usage: $0 {all} {user_api|notification_api|log_api|traffic_analytics_api|traffic_light_controller_api|traffic_light_coordinator_api|intersection_controller_api|sensor_api|detection_api}"
}

build_and_push_image() {
    local service="$1"
    local dockerfile="${SERVICES_PATHS[$service]}"
    local image="$DOCKER_USERNAME/$DOCKER_REPO:$service"

    if [[ -z "$dockerfile" ]]; then
        echo "❌ Unknown service: $service"
        exit 1
    fi

    local path="$ROOT_DIR/$dockerfile"
    if [[ -f "$path" ]]; then
        echo "🔨 Building image for $service..."
        docker build -t "$image" -f "$path" "$ROOT_DIR" || {
            echo "❌ Build failed for $service"
            return 1
        }

        echo "📤 Pushing image for $service to Docker Hub..."
        docker push "$image" || {
            echo "⚠️ Push failed for $service"
            return 1
        }
    else
        echo "⚠️ No Dockerfile found at $path"
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
