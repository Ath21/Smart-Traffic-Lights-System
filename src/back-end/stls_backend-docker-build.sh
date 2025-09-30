#!/bin/bash
set -e

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
DOCKER_USERNAME="ath21"
DOCKER_REPO="stls"

declare -A SERVICES_PATHS=(
    [user_api]="UserLayer/Services/UserService/Docker/Dockerfile"
    [notification_api]="UserLayer/Services/NotificationService/Docker/Dockerfile"
    [log_api]="LogLayer/Services/LogService/Docker/Dockerfile"
    [traffic_analytics_api]="TrafficLayer/Services/TrafficAnalyticsService/Docker/Dockerfile"
    [traffic_light_controller_api]="TrafficLayer/Services/TrafficLightControllerService/Docker/Dockerfile"
    [traffic_light_coordinator_api]="TrafficLayer/Services/TrafficLightCoordinatorService/Docker/Dockerfile"
    [intersection_controller_api]="TrafficLayer/Services/IntersectionControllerService/Docker/Dockerfile"
    [sensor_api]="SensorLayer/Services/SensorService/Docker/Dockerfile"
    [detection_api]="SensorLayer/Services/DetectionService/Docker/Dockerfile"
)

usage() {
    echo "Usage: $0 [--all] [service...]"
    echo ""
    echo "Options:"
    echo "  --all        Build and push all services"
    echo "  service...   Build and push one or more specific services"
    echo ""
    echo "Available services:"
    for s in "${!SERVICES_PATHS[@]}"; do
        echo "  - $s"
    done
    echo ""
    echo "Examples:"
    echo "  ./stls_backend-build.sh --all"
    echo "  ./stls_backend-build.sh user_api sensor_api"
}

build_and_push_image() {
    local service="$1"
    local dockerfile="${SERVICES_PATHS[$service]}"
    local image="$DOCKER_USERNAME/$DOCKER_REPO:$service"

    if [[ -z "$dockerfile" ]]; then
        echo "Unknown service: $service"
        exit 1
    fi

    local path="$ROOT_DIR/$dockerfile"
    if [[ -f "$path" ]]; then
        echo "Building image for $service..."
        docker build -t "$image" -f "$path" "$ROOT_DIR"
        echo "Pushing image for $service to Docker Hub..."
        docker push "$image"
    else
        echo "No Dockerfile found at $path"
    fi
}

main() {
    if [[ $# -eq 0 ]]; then
        usage
        exit 1
    fi

    if [[ "$1" == "--all" ]]; then
        for service in "${!SERVICES_PATHS[@]}"; do
            build_and_push_image "$service"
        done
    else
        for service in "$@"; do
            build_and_push_image "$service"
        done
    fi

    echo "Done."
}

main "$@"
