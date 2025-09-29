#!/bin/bash
set -e

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"

usage() {
  echo "Usage: $0 {up|down|reset}"
  echo ""
  echo "  up     - Start Minikube (with ingress) and deploy all STLS services"
  echo "  down   - Stop Minikube cluster (all pods paused, not deleted)"
  echo "  reset  - Delete and recreate Minikube cluster, then redeploy everything"
  echo ""
  echo "Examples:"
  echo "  ./stls_backend-k8s.sh up"
  echo "  ./stls_backend-k8s.sh down"
  echo "  ./stls_backend-k8s.sh reset"
}

case "$1" in
  up)
    echo "==================================================="
    echo "   Starting Minikube with Ingress for STLS system   "
    echo "==================================================="
    minikube start --driver=docker --addons=ingress --memory=6g --cpus=4
    ;;

  down)
    echo "==================================================="
    echo "   Stopping Minikube for STLS system               "
    echo "==================================================="
    minikube stop
    exit 0
    ;;

  reset)
    echo "==================================================="
    echo "   Resetting Minikube cluster                      "
    echo "==================================================="
    minikube delete
    minikube start --driver=docker --addons=ingress --memory=6g --cpus=4
    ;;

  *)
    usage
    exit 1
    ;;
esac

echo "==================================================="
echo "   Deploying Smart Traffic Lights System (STLS)    "
echo "==================================================="

# 1. Deploy RabbitMQ
echo "[1/6] Deploying RabbitMQ..."
kubectl apply -f $ROOT_DIR/MessageLayer/RabbitMQ/K8s/

# 2. Deploy Databases
echo "[2/6] Deploying Databases..."
kubectl apply -f $ROOT_DIR/SensorLayer/Databases/DetectionDB/Mongo/K8s/
kubectl apply -f $ROOT_DIR/SensorLayer/Databases/DetectionCacheDB/Redis/K8s/

# 3. User Layer
echo "[3/6] Deploying User Layer APIs..."
kubectl apply -f $ROOT_DIR/UserLayer/Services/UserService/K8s/
kubectl apply -f $ROOT_DIR/UserLayer/Services/NotificationService/K8s/

# 4. Log Layer
echo "[4/6] Deploying Log Layer..."
kubectl apply -f $ROOT_DIR/LogLayer/Services/LogService/K8s/

# 5. Sensor Layer
echo "[5/6] Deploying Sensor Layer APIs..."
kubectl apply -f $ROOT_DIR/SensorLayer/Services/DetectionService/K8s/
kubectl apply -f $ROOT_DIR/SensorLayer/Services/SensorService/K8s/

# 6. Traffic Layer
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
echo "ℹ️  Add to /etc/hosts if not already:"
echo "    127.0.0.1 stls.local"
echo ""
echo "Test with:"
echo "    curl http://stls.local/sensor/agiou-spyridonos/api/sensors/local"
