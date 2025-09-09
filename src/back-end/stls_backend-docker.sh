#!/bin/bash
set -e

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "==================================================="
echo "   Deploying Smart Traffic Lights System (STLS)   "
echo "        via Docker Compose (layered setup)        "
echo "==================================================="

# ----------------------------------------------------
# 0. Create required networks
# ----------------------------------------------------
echo "[0] Creating Docker networks..."
docker network create user_network        || true
docker network create notification_network || true
docker network create log_network          || true
docker network create sensor_network       || true
docker network create detection_network    || true
docker network create detection_cache_network || true
docker network create traffic_network      || true
docker network create traffic_light_network || true
docker network create traffic_light_cache_network || true
docker network create traffic_analytics_network  || true
docker network create rabbitmq_network     || true

# ----------------------------------------------------
# Helper function
# ----------------------------------------------------
deploy_compose() {
  local project="$1"
  local path="$2"
  if [ -f "$path/docker-compose.yaml" ]; then
    echo "▶ Deploying $project: $path"
    docker compose -p "$project" -f "$path/docker-compose.yaml" -f "$path/docker-compose.override.yaml" up -d
  fi
}

# ----------------------------------------------------
# 1. User Layer (User + Notification)
# ----------------------------------------------------
echo "[1/4] Deploying User Layer (stls_user)..."
deploy_compose "stls_user" "$ROOT_DIR/UserLayer/Databases/UserDB/MSSQL/Docker"
deploy_compose "stls_user" "$ROOT_DIR/UserLayer/Databases/NotificationDB/Mongo/Docker"
deploy_compose "stls_user" "$ROOT_DIR/UserLayer/Services/UserService/Docker"
deploy_compose "stls_user" "$ROOT_DIR/UserLayer/Services/NotificationService/Docker"

# ----------------------------------------------------
# 2. Message Layer (RabbitMQ)
# ----------------------------------------------------
echo "[2/4] Deploying Message Layer (stls_message)..."
deploy_compose "stls_message" "$ROOT_DIR/MessageLayer/RabbitMQ/Docker"

# ----------------------------------------------------
# 3. Traffic Layer (APIs + DBs)
# ----------------------------------------------------
echo "[3/4] Deploying Traffic Layer (stls_traffic)..."
deploy_compose "stls_traffic" "$ROOT_DIR/TrafficLayer/Databases/TrafficAnalyticsDB/PostgreSQL/Docker"
deploy_compose "stls_traffic" "$ROOT_DIR/TrafficLayer/Databases/TrafficLightDB/PostgreSQL/Docker"
deploy_compose "stls_traffic" "$ROOT_DIR/TrafficLayer/Databases/TrafficLightCacheDB/Redis/Docker"
deploy_compose "stls_traffic" "$ROOT_DIR/TrafficLayer/Services/TrafficAnalyticsService/Docker"
deploy_compose "stls_traffic" "$ROOT_DIR/TrafficLayer/Services/TrafficLightCoordinatorService/Docker"
deploy_compose "stls_traffic" "$ROOT_DIR/TrafficLayer/Services/IntersectionControllerService/Docker"
deploy_compose "stls_traffic" "$ROOT_DIR/TrafficLayer/Services/TrafficLightControllerService/Docker"

# ----------------------------------------------------
# 4. Sensor Layer (APIs + DBs)
# ----------------------------------------------------
echo "[4/4] Deploying Sensor Layer (stls_sensor)..."
deploy_compose "stls_sensor" "$ROOT_DIR/SensorLayer/Databases/DetectionDB/Mongo/Docker"
deploy_compose "stls_sensor" "$ROOT_DIR/SensorLayer/Databases/DetectionCacheDB/Redis/Docker"
deploy_compose "stls_sensor" "$ROOT_DIR/SensorLayer/Services/DetectionService/Docker"
deploy_compose "stls_sensor" "$ROOT_DIR/SensorLayer/Services/SensorService/Docker"

echo "==================================================="
echo " ✅ STLS system deployed with Docker Compose!"
echo "==================================================="
