#!/bin/bash

set -e  # Exit on any command failure

# ================================
# 📌 Get script directory
# ================================
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# ================================
# 🧠 Helper: Run service script if it exists
# ================================
try_start() 
{
    local script="$1"
    if [ -x "$script" ]; then
        bash "$script"
    else
        echo "⚠️ Cannot execute $script. Skipping."
    fi
}

wait_for_redis() 
{
    echo "⏳ Waiting for Redis to be ready..."
    local container
    container=$(docker ps --filter "name=redis" --format "{{.Names}}")

    if [[ -z "$container" ]]; then
        echo "❌ Redis container not found."
        exit 1
    fi

    until docker exec "$container" redis-cli ping | grep -q PONG; do
        sleep 2
    done

    echo "✅ Redis is ready."
}

start_redis() 
{
    docker network ls | grep -q redis_network || docker network create redis_network

    echo "📦 Starting Redis..."
    docker compose \
        -f ./TrafficLayer/TrafficLightMemoryDB/Redis/docker-compose.yaml \
        -f ./TrafficLayer/TrafficLightMemoryDB/Redis/docker-compose.override.yaml \
        -p redis \
        up -d

    wait_for_redis
}


# ================================
# 🚀 Main Execution
# ================================
main() 
{
    SERVICE=""

    while [[ "$#" -gt 0 ]]; do
        case "$1" in
            --service=*) SERVICE="${1#*=}" ;;
            *) echo "❌ Unknown option: $1"; exit 1 ;;
        esac
        shift
    done

    if [[ -n "$SERVICE" ]]; then
        if [[ ! -d "$SCRIPT_DIR/$SERVICE" ]]; then
            echo "❌ Unknown service '$SERVICE' in Traffic Layer."
            exit 1
        fi
        echo "🚀 Starting ONLY $SERVICE in Traffic Layer..."
        start_redis
        try_start "$SCRIPT_DIR/$SERVICE/up$SERVICE.sh"
    else
        echo "🚀 Starting ALL services in Traffic Layer..."
        start_redis
        try_start "$SCRIPT_DIR/IntersectionControllerService/upIntersectionControllerService.sh"
        try_start "$SCRIPT_DIR/TrafficLightControllerService/upTrafficLightControllerService.sh"
        try_start "$SCRIPT_DIR/TrafficLightCoordinatorService/upTrafficLightCoordinatorService.sh"
        try_start "$SCRIPT_DIR/TrafficAnalyticsService/upTrafficAnalyticsService.sh"
    fi
}


main "$@"
