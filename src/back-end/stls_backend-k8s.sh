#!/bin/bash
set -e

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"

usage() {
  echo "Usage: $0 {up|down|reset}"
  echo ""
  echo "Commands:"
  echo "  up       Start Minikube (with ingress) and deploy all STLS services"
  echo "  down     Stop Minikube cluster (pods paused, not deleted)"
  echo "  reset    Delete and recreate Minikube cluster, then redeploy everything"
  echo ""
  echo "Examples:"
  echo "  ./stls_backend-k8s.sh up"
  echo "  ./stls_backend-k8s.sh down"
  echo "  ./stls_backend-k8s.sh reset"
}

case "$1" in
  up)
    echo "Starting Minikube with Ingress..."
    minikube start --driver=docker --addons=ingress --memory=6g --cpus=4
    ;;
  down)
    echo "Stopping Minikube..."
    minikube stop
    exit 0
    ;;
  reset)
    echo "Resetting Minikube cluster..."
    minikube delete
    minikube start --driver=docker --addons=ingress --memory=6g --cpus=4
    ;;
  *)
    usage
    exit 1
    ;;
esac

echo "Deploying Smart Traffic Lights System (STLS)..."

# RabbitMQ
kubectl apply -f $ROOT_DIR/MessageLayer/RabbitMQ/K8s/

# Databases
kubectl apply -f $ROOT_DIR/SensorLayer/Databases/DetectionDB/Mongo/K8s/
kubectl apply -f $ROOT_DIR/SensorLayer/Databases/DetectionCacheDB/Redis/K8s/

# User Layer
kubectl apply -f $ROOT_DIR/UserLayer/Services/UserService/K8s/
kubectl apply -f $ROOT_DIR/UserLayer/Services/NotificationService/K8s/

# Log Layer
kubectl apply -f $ROOT_DIR/LogLayer/Services/LogService/K8s/

# Sensor Layer
kubectl apply -f $ROOT_DIR/SensorLayer/Services/DetectionService/K8s/
kubectl apply -f $ROOT_DIR/SensorLayer/Services/SensorService/K8s/

# Traffic Layer
kubectl apply -f $ROOT_DIR/TrafficLayer/Services/TrafficAnalyticsService/K8s/
kubectl apply -f $ROOT_DIR/TrafficLayer/Services/TrafficLightCoordinatorService/K8s/
kubectl apply -f $ROOT_DIR/TrafficLayer/Services/IntersectionControllerService/K8s/
kubectl apply -f $ROOT_DIR/TrafficLayer/Services/TrafficLightControllerService/K8s/

# Ingress
kubectl apply -f $ROOT_DIR/k8s.ingress.yaml

echo ""
echo "STLS deployed successfully on Minikube."
echo "Add to /etc/hosts if not already:"
echo "  127.0.0.1 stls.local"
echo ""
echo "Test example:"
echo "  curl http://stls.local/sensor/agiou-spyridonos/api/sensors/local"
