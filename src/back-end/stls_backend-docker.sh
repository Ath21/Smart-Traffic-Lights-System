#!/bin/bash
set -e

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "==================================================="
echo "   Deploying Smart Traffic Lights System (STLS)   "
echo "          via Docker Compose (local)              "
echo "==================================================="

# Helper to deploy a docker-compose folder
deploy_compose() {
  local path="$1"
  if [ -f "$path/docker-compose.yaml" ]; then
    echo "▶ Deploying: $path"
    docker compose -f "$path/docker-compose.yaml" -f "$path/docker-compose.override.yaml" up -d
  fi
}

# 1. RabbitMQ
echo "[1/5] Deploying RabbitMQ..."
deploy_compose "$ROOT_DIR/MessageLayer/RabbitMQ/Docker"

# 2. Databases (User, Notification, Log, Detection, Traffic)
echo "[2/5] Deploying Databases..."
deploy_compose "$ROOT_DIR/UserLayer/Databases/UserDB/MSSQL/Docker"
deploy_compose "$ROOT_DIR/UserLayer/Databases/NotificationDB/Mongo/Docker"
deploy_compose "$ROOT_DIR/LogLayer/Databases/Mongo/Docker"
deploy_compose "$ROOT_DIR/SensorLayer/Databases/DetectionDB/Mongo/Docker"
deploy_compose "$ROOT_DIR/SensorLayer/Databases/DetectionCacheDB/Redis/Docker"
deploy_compose "$ROOT_DIR/TrafficLayer/Databases/TrafficAnalyticsDB/PostgreSQL/Docker"
deploy_compose "$ROOT_DIR/TrafficLayer/Databases/TrafficLightDB/PostgreSQL/Docker"
deploy_compose "$ROOT_DIR/TrafficLayer/Databases/TrafficLightCacheDB/Redis/Docker"

# 3. UserLayer APIs
echo "[3/5] Deploying User Layer APIs..."
deploy_compose "$ROOT_DIR/UserLayer/Services/UserService/Docker"
deploy_compose "$ROOT_DIR/UserLayer/Services/NotificationService/Docker"

# 4. LogLayer + SensorLayer APIs
echo "[4/5] Deploying Log & Sensor Layer APIs..."
deploy_compose "$ROOT_DIR/LogLayer/Services/LogService/Docker"
deploy_compose "$ROOT_DIR/SensorLayer/Services/DetectionService/Docker"
deploy_compose "$ROOT_DIR/SensorLayer/Services/SensorService/Docker"

# 5. TrafficLayer APIs
echo "[5/5] Deploying Traffic Layer APIs..."
deploy_compose "$ROOT_DIR/TrafficLayer/Services/TrafficAnalyticsService/Docker"
deploy_compose "$ROOT_DIR/TrafficLayer/Services/TrafficLightCoordinatorService/Docker"
deploy_compose "$ROOT_DIR/TrafficLayer/Services/IntersectionControllerService/Docker"
deploy_compose "$ROOT_DIR/TrafficLayer/Services/TrafficLightControllerService/Docker"

echo "==================================================="
echo " STLS system deployed with Docker Compose!"
echo "==================================================="
