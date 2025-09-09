#!/bin/bash
set -e

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "==================================================="
echo "   Deploying Smart Traffic Lights System (STLS)   "
echo "==================================================="

# 1. Apply RabbitMQ first (backbone for messaging)
echo "[1/5] Deploying RabbitMQ..."
kubectl apply -f $ROOT_DIR/MessageLayer/RabbitMQ/K8s/

# 2. Deploy all databases (order: User, Notification, Log, Detection, Traffic)
echo "[2/5] Deploying Databases..."
find $ROOT_DIR -type f -path "*/K8s/k8s.*.yaml" \
  | grep -E "DB|Mongo|Postgres|MSSQL|Redis" \
  | sort \
  | while read file; do
      echo "Applying DB manifest: $file"
      kubectl apply -f "$file"
    done

# 3. Deploy UserLayer services
echo "[3/5] Deploying User Layer APIs..."
kubectl apply -f $ROOT_DIR/UserLayer/Services/UserService/K8s/
kubectl apply -f $ROOT_DIR/UserLayer/Services/NotificationService/K8s/

# 4. Deploy LogLayer + SensorLayer services
echo "[4/5] Deploying Log & Sensor Layer APIs..."
kubectl apply -f $ROOT_DIR/LogLayer/Services/LogService/K8s/
kubectl apply -f $ROOT_DIR/SensorLayer/Services/DetectionService/K8s/
kubectl apply -f $ROOT_DIR/SensorLayer/Services/SensorService/K8s/

# 5. Deploy TrafficLayer services
echo "[5/5] Deploying Traffic Layer APIs..."
kubectl apply -f $ROOT_DIR/TrafficLayer/Services/TrafficAnalyticsService/K8s/
kubectl apply -f $ROOT_DIR/TrafficLayer/Services/TrafficLightCoordinatorService/K8s/
kubectl apply -f $ROOT_DIR/TrafficLayer/Services/IntersectionControllerService/K8s/
kubectl apply -f $ROOT_DIR/TrafficLayer/Services/TrafficLightControllerService/K8s/

echo "==================================================="
echo " STLS system deployed on Minikube successfully!"
echo "==================================================="
