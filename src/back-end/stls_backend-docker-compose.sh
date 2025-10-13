#!/bin/bash
set -e

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
export ROOT_DIR
echo "ROOT_DIR resolved to: $ROOT_DIR"

LAYERS=("message" "user" "traffic" "sensor" "log")

# ----------------------------------------------------
# Usage
# ----------------------------------------------------
usage() {
  echo "Usage: $0 {up|down} {/all|/user|/message|/traffic|/sensor|/log}"
  echo ""
  echo "Commands:"
  echo "  up       Start one or more layers"
  echo "  down     Stop one or more layers"
  echo ""
  echo "Layers:"
  echo "  /all      All layers in the correct order"
  echo "  /message  Messaging layer (RabbitMQ)"
  echo "  /user     User layer (User + Notification)"
  echo "  /traffic  Traffic layer (controllers + analytics)"
  echo "  /sensor   Sensor layer (sensors + detection)"
  echo "  /log      Log layer"
  echo ""
  echo "Examples:"
  echo "  ./stls_backend-compose.sh up /all"
  echo "  ./stls_backend-compose.sh down /traffic"
}

# ----------------------------------------------------
# Networks
# ----------------------------------------------------
NETWORKS=(
  user-network
  notification-network
  log-network
  sensor-network
  detection-network
  detection-cache-network
  traffic-light-network
  traffic-light-cache-network
  traffic-analytics-network
  rabbitmq-network
)

create_networks() {
  echo "Creating Docker networks..."
  for net in "${NETWORKS[@]}"; do
    docker network create "$net" >/dev/null 2>&1 || true
  done
}

remove_networks() {
  echo "Removing Docker networks..."
  for net in "${NETWORKS[@]}"; do
    docker network rm "$net" >/dev/null 2>&1 || true
  done
}

# ----------------------------------------------------
# Helpers
# ----------------------------------------------------
wait_for_rabbitmq() {
  echo "Waiting for RabbitMQ to be ready..."
  local container
  container=$(docker ps --filter "name=rabbitmq" --format "{{.Names}}" | head -n1)
  if [[ -z "$container" ]]; then
    echo "RabbitMQ container not found."
    exit 1
  fi
  until docker exec "$container" rabbitmqctl status &>/dev/null; do
    sleep 2
  done
  echo "RabbitMQ is ready."
}

wait_for_redis() {
  local redis_name=$1
  local friendly=$2
  echo "Waiting for $friendly to be ready..."
  local container
  container=$(docker ps --filter "name=$redis_name" --format "{{.Names}}" | head -n1)
  if [[ -z "$container" ]]; then
    echo "$friendly container not found."
    exit 1
  fi
  until docker exec "$container" redis-cli ping 2>/dev/null | grep -q "PONG"; do
    sleep 2
  done
  echo "$friendly is ready."
}

# ----------------------------------------------------
# Deploy and stop layers
# ----------------------------------------------------
deploy_layer() {
  local layer=$1
  case $layer in
    message)
      docker compose -p stls_message \
        -f "$ROOT_DIR/MessageLayer/RabbitMQ/Compose/compose.rabbitmq.yaml" \
        -f "$ROOT_DIR/MessageLayer/RabbitMQ/Compose/compose.rabbitmq.override.yaml" up -d
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
      docker compose -p stls_traffic \
        -f "$ROOT_DIR/TrafficLayer/Databases/TrafficLightCacheDB/Redis/Docker/docker-compose.yaml" \
        -f "$ROOT_DIR/TrafficLayer/Databases/TrafficLightCacheDB/Redis/Docker/docker-compose.override.yaml" up -d
      wait_for_redis "trafficlightcachedb_container" "TrafficLightCacheDB"

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
      docker compose -p stls_sensor \
        -f "$ROOT_DIR/SensorLayer/Databases/DetectionCacheDB/Redis/Compose/compose.detection-cache-db.yaml" \
        -f "$ROOT_DIR/SensorLayer/Databases/DetectionCacheDB/Redis/Compose/compose.detection-cache-db.override.yaml" \
        up -d
      wait_for_redis "detection-cache-db" "DetectionCacheDB"

      docker compose -p stls_sensor \
        -f "$ROOT_DIR/SensorLayer/Databases/DetectionDB/Mongo/Compose/compose.detection-db.yaml" \
        -f "$ROOT_DIR/SensorLayer/Databases/DetectionDB/Mongo/Compose/compose.detection-db.override.yaml" \
        up -d

      docker compose -p stls_sensor \
        -f "$ROOT_DIR/SensorLayer/Services/DetectionService/Compose/compose.detection-api.agiou-spyridonos.yaml" \
        -f "$ROOT_DIR/SensorLayer/Services/DetectionService/Compose/compose.detection-api.anatoliki-pyli.yaml" \
        -f "$ROOT_DIR/SensorLayer/Services/DetectionService/Compose/compose.detection-api.dytiki-pyli.yaml" \
        -f "$ROOT_DIR/SensorLayer/Services/DetectionService/Compose/compose.detection-api.ekklisia.yaml" \
        -f "$ROOT_DIR/SensorLayer/Services/DetectionService/Compose/compose.detection-api.kentriki-pyli.yaml" \
        up -d

      docker compose -p stls_sensor \
        -f "$ROOT_DIR/SensorLayer/Services/SensorService/Compose/compose.sensor-api.agiou-spyridonos.yaml" \
        -f "$ROOT_DIR/SensorLayer/Services/SensorService/Compose/compose.sensor-api.anatoliki-pyli.yaml" \
        -f "$ROOT_DIR/SensorLayer/Services/SensorService/Compose/compose.sensor-api.dytiki-pyli.yaml" \
        -f "$ROOT_DIR/SensorLayer/Services/SensorService/Compose/compose.sensor-api.ekklisia.yaml" \
        -f "$ROOT_DIR/SensorLayer/Services/SensorService/Compose/compose.sensor-api.kentriki-pyli.yaml" \
        up -d
      ;;
    log)
      docker compose -p stls_log \
        -f "$ROOT_DIR/LogLayer/Databases/LogDB/Mongo/Compose/compose.log-db.yaml" \
        -f "$ROOT_DIR/LogLayer/Databases/LogDB/Mongo/Compose/compose.log-db.override.yaml" \
        -f "$ROOT_DIR/LogLayer/Services/LogService/Compose/compose.log-api.yaml" \
        -f "$ROOT_DIR/LogLayer/Services/LogService/Compose/compose.log-api.override.yaml" \
        up -d
      ;;
    *)
      echo "Unknown layer: $layer"
      exit 1
      ;;
  esac
}

stop_layer() {
  local layer=$1
  docker compose -p "stls_$layer" down
  echo "Cleaning up unused volumes and images..."
  docker volume prune -f >/dev/null 2>&1 || true
  docker image prune -f >/dev/null 2>&1 || true
}

# ----------------------------------------------------
# Main logic
# ----------------------------------------------------
ACTION=$1
LAYER=${2#/}

if [[ "$ACTION" == "up" ]]; then
  create_networks
  if [[ "$LAYER" == "all" ]]; then
    echo "Bringing up message layer first..."
    deploy_layer "message"
    for layer in "log" "traffic" "user" "sensor"; do
      echo "Bringing up $layer layer..."
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
  usage
  exit 1
fi
