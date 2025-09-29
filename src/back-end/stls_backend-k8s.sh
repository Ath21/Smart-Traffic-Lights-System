#!/bin/bash
set -e

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "==================================================="
echo "   Deploying Smart Traffic Lights System (STLS)    "
echo "==================================================="

# 1. Deploy RabbitMQ (messaging backbone)
echo "[1/6] Deploying RabbitMQ..."
kubectl apply -f $ROOT_DIR/MessageLayer/RabbitMQ/K8s/

# 2. Deploy Databases (Mongo, Redis, MSSQL, Postgres as needed)
echo "[2/6] Deploying Databases..."
kubectl apply -f $ROOT_DIR/SensorLayer/Databases/DetectionDB/Mongo/K8s/
kubectl apply -f $ROOT_DIR/SensorLayer/Databases/DetectionCacheDB/Redis/K8s/
# (Add UserDB, LogDB, TrafficDB if you have them)

# 3. Deploy User Layer APIs (User + Notification)
echo "[3/6] Deploying User Layer APIs..."
kubectl apply -f $ROOT_DIR/UserLayer/Services/UserService/K8s/
kubectl apply -f $ROOT_DIR/UserLayer/Services/NotificationService/K8s/

# 4. Deploy Log Layer
echo "[4/6] Deploying Log Layer..."
kubectl apply -f $ROOT_DIR/LogLayer/Services/LogService/K8s/

# 5. Deploy Sensor Layer (Detection + Sensor)
echo "[5/6] Deploying Sensor Layer APIs..."
kubectl apply -f $ROOT_DIR/SensorLayer/Services/DetectionService/K8s/
kubectl apply -f $ROOT_DIR/SensorLayer/Services/SensorService/K8s/

# 6. Deploy Traffic Layer APIs
echo "[6/6] Deploying Traffic Layer APIs..."
kubectl apply -f $ROOT_DIR/TrafficLayer/Services/TrafficAnalyticsService/K8s/
kubectl apply -f $ROOT_DIR/TrafficLayer/Services/TrafficLightCoordinatorService/K8s/
kubectl apply -f $ROOT_DIR/TrafficLayer/Services/IntersectionControllerService/K8s/
kubectl apply -f $ROOT_DIR/TrafficLayer/Services/TrafficLightControllerService/K8s/

# Apply Ingress
echo "Applying STLS Ingress..."
kubectl apply -f $ROOT_DIR/k8s.ingress.yaml

echo "==================================================="
echo " ✅ STLS system deployed on Minikube successfully!"
echo "==================================================="
echo ""
echo "ℹ️  Remember to add this to your /etc/hosts file:"
echo "    127.0.0.1 stls.local"
echo ""
echo "Then test with: curl http://stls.local/sensor/agiou-spyridonos/api/sensors/local"
