#!/bin/bash
set -e

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"

# Layers (short flags)
LAYERS=("message" "user" "traffic" "sensor" "log")

# Networks
NETWORKS=(
  user_network
  notification_network
  log_network
  sensor_network
  detection_network
  detection_cache_network
  traffic_network
  traffic_light_network
  traffic_light_cache_network
  traffic_analytics_network
  rabbitmq_network
)

# ----------------------------------------------------
# Helpers
# ----------------------------------------------------

create_networks() {
  echo "[*] Creating Docker networks..."
  for net in "${NETWORKS[@]}"; do
    docker network create "$net" >/dev/null 2>&1 || true
  done
}

remove_networks() {
  echo "[*] Removing Docker networks..."
  for net in "${NETWORKS[@]}"; do
    docker network rm "$net" >/dev/null 2>&1 || true
  done
}

wait_for_rabbitmq() {
    echo "⏳ Waiting for RabbitMQ to be ready..."
    local container
    container=$(docker ps --filter "name=rabbitmq" --format "{{.Names}}" | head -n1)

    if [[ -z "$container" ]]; then
        echo "❌ RabbitMQ container not found."
        exit 1
    fi

    until docker exec "$container" rabbitmqctl status &>/dev/null; do
        sleep 2
    done

    echo "✅ RabbitMQ is ready."
}

wait_for_redis() {
    local redis_name=$1
    local friendly=$2
    echo "⏳ Waiting for $friendly to be ready..."
    local container
    container=$(docker ps --filter "name=$redis_name" --format "{{.Names}}" | head -n1)

    if [[ -z "$container" ]]; then
        echo "❌ $friendly container not found."
        exit 1
    fi

    until docker exec "$container" redis-cli ping 2>/dev/null | grep -q "PONG"; do
        sleep 2
    done

    echo "✅ $friendly is ready."
}

deploy_layer() {
  local layer=$1
  case $layer in
    message)
      docker compose -p stls_message \
        -f "$ROOT_DIR/MessageLayer/RabbitMQ/Docker/docker-compose.yaml" \
        -f "$ROOT_DIR/MessageLayer/RabbitMQ/Docker/docker-compose.override.yaml" up -d
      wait_for_rabbitmq
      ;;
    user)
      docker compose -p stls_user \
        -f "$ROOT_DIR/UserLayer/Databases/UserDB/MSSQL/Docker/docker-compose.yaml" -f "$ROOT_DIR/UserLayer/Databases/UserDB/MSSQL/Docker/docker-compose.override.yaml" \
        -f "$ROOT_DIR/UserLayer/Databases/NotificationDB/Mongo/Docker/docker-compose.yaml" -f "$ROOT_DIR/UserLayer/Databases/NotificationDB/Mongo/Docker/docker-compose.override.yaml" \
        -f "$ROOT_DIR/UserLayer/Services/UserService/Docker/docker-compose.yaml" -f "$ROOT_DIR/UserLayer/Services/UserService/Docker/docker-compose.override.yaml" \
        -f "$ROOT_DIR/UserLayer/Services/NotificationService/Docker/docker-compose.yaml" -f "$ROOT_DIR/UserLayer/Services/NotificationService/Docker/docker-compose.override.yaml" \
        up -d
      ;;
    traffic)
      # Shared Redis first
      docker compose -p stls_traffic \
        -f "$ROOT_DIR/TrafficLayer/Databases/TrafficLightCacheDB/Redis/Docker/docker-compose.yaml" \
        -f "$ROOT_DIR/TrafficLayer/Databases/TrafficLightCacheDB/Redis/Docker/docker-compose.override.yaml" up -d
      wait_for_redis "trafficlightcachedb_container" "TrafficLightCacheDB"

      # Then Postgres DBs + APIs
      docker compose -p stls_traffic \
        -f "$ROOT_DIR/TrafficLayer/Databases/TrafficAnalyticsDB/PostgreSQL/Docker/docker-compose.yaml" -f "$ROOT_DIR/TrafficLayer/Databases/TrafficAnalyticsDB/PostgreSQL/Docker/docker-compose.override.yaml" \
        -f "$ROOT_DIR/TrafficLayer/Databases/TrafficLightDB/MSSQL/Docker/docker-compose.yaml" -f "$ROOT_DIR/TrafficLayer/Databases/TrafficLightDB/MSSQL/Docker/docker-compose.override.yaml" \
        -f "$ROOT_DIR/TrafficLayer/Services/TrafficAnalyticsService/Docker/docker-compose.yaml" -f "$ROOT_DIR/TrafficLayer/Services/TrafficAnalyticsService/Docker/docker-compose.override.yaml" \
        -f "$ROOT_DIR/TrafficLayer/Services/TrafficLightCoordinatorService/Docker/docker-compose.yaml" -f "$ROOT_DIR/TrafficLayer/Services/TrafficLightCoordinatorService/Docker/docker-compose.override.yaml" \
        -f "$ROOT_DIR/TrafficLayer/Services/IntersectionControllerService/Docker/docker-compose.yaml" -f "$ROOT_DIR/TrafficLayer/Services/IntersectionControllerService/Docker/docker-compose.override.yaml" \
        -f "$ROOT_DIR/TrafficLayer/Services/TrafficLightControllerService/Docker/docker-compose.yaml" -f "$ROOT_DIR/TrafficLayer/Services/TrafficLightControllerService/Docker/docker-compose.override.yaml" \
        up -d
      ;;
    sensor)
      # Shared Redis first
      docker compose -p stls_sensor \
        -f "$ROOT_DIR/SensorLayer/Databases/DetectionCacheDB/Redis/Docker/docker-compose.yaml" \
        -f "$ROOT_DIR/SensorLayer/Databases/DetectionCacheDB/Redis/Docker/docker-compose.override.yaml" up -d
      wait_for_redis "detectioncachedb_container" "DetectionCacheDB"

      # Then Mongo DB + APIs
      docker compose -p stls_sensor \
        -f "$ROOT_DIR/SensorLayer/Databases/DetectionDB/Mongo/Docker/docker-compose.yaml" -f "$ROOT_DIR/SensorLayer/Databases/DetectionDB/Mongo/Docker/docker-compose.override.yaml" \
        -f "$ROOT_DIR/SensorLayer/Services/DetectionService/Docker/docker-compose.yaml" -f "$ROOT_DIR/SensorLayer/Services/DetectionService/Docker/docker-compose.override.yaml" \
        -f "$ROOT_DIR/SensorLayer/Services/SensorService/Docker/docker-compose.yaml" -f "$ROOT_DIR/SensorLayer/Services/SensorService/Docker/docker-compose.override.yaml" \
        up -d
      ;;
    log)
      docker compose -p stls_log \
        -f "$ROOT_DIR/LogLayer/Databases/LogDB/Mongo/Docker/docker-compose.yaml" -f "$ROOT_DIR/LogLayer/Databases/LogDB/Mongo/Docker/docker-compose.override.yaml" \
        -f "$ROOT_DIR/LogLayer/Services/LogService/Docker/docker-compose.yaml" -f "$ROOT_DIR/LogLayer/Services/LogService/Docker/docker-compose.override.yaml" \
        up -d
  ;;
    *)
      echo "❌ Unknown layer: $layer"
      exit 1
      ;;
  esac
}

stop_layer() {
  local layer=$1
  docker compose -p "stls_$layer" down
  echo "🧹 Cleaning up unused volumes and images..."
  docker volume prune -f >/dev/null 2>&1 || true
  docker image prune -f >/dev/null 2>&1 || true
}


# ----------------------------------------------------
# Main logic
# ----------------------------------------------------

ACTION=$1
LAYER=${2#/}   # remove leading "/"

if [[ "$ACTION" == "up" ]]; then
  create_networks
  if [[ "$LAYER" == "all" ]]; then
    # Always deploy message first
    echo "▶ Bringing up message (RabbitMQ)..."
    deploy_layer "message"
    for layer in "log" "traffic" "user" "sensor"; do
      echo "▶ Bringing up $layer..."
      deploy_layer "$layer"
    done
  else
    deploy_layer "$LAYER"
  fi

elif [[ "$ACTION" == "down" ]]; then
  if [[ "$LAYER" == "all" ]]; then
    for (( idx=${#LAYERS[@]}-1 ; idx>=0 ; idx-- )) ; do
      stop_layer "${LAYERS[idx]}"
    done
    remove_networks
  else
    stop_layer "$LAYER"
  fi

else
  echo "Usage: $0 {up|down} {/all|/user|/message|/traffic|/sensor|/log}"
  exit 1
fi
