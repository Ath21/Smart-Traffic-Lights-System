#!/bin/bash
set -e

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
NAMESPACE="uniwa-stls"

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
    echo "🚀 Starting Minikube with Ingress..."
    minikube start --driver=docker --addons=ingress --cpus=4 --memory=8192
    ;;
  down)
    echo "🛑 Stopping Minikube..."
    minikube stop
    exit 0
    ;;
  reset)
    echo "♻️ Resetting Minikube cluster..."
    minikube delete
    minikube start --driver=docker --addons=ingress --cpus=4 --memory=8192
    ;;
  *)
    usage
    exit 1
    ;;
esac

echo "📦 Deploying Smart Traffic Lights System (STLS)..."

# Ensure namespace exists
kubectl get namespace $NAMESPACE >/dev/null 2>&1 || kubectl create namespace $NAMESPACE

# Apply global config (ConfigMap, Secrets)
kubectl apply -f $ROOT_DIR/k8s.config-map.yaml -n $NAMESPACE

# RabbitMQ
kubectl apply -f $ROOT_DIR/MessageLayer/RabbitMQ/K8s/ -n $NAMESPACE

# Databases
kubectl apply -f $ROOT_DIR/SensorLayer/Databases/DetectionDB/Mongo/K8s/ -n $NAMESPACE
kubectl apply -f $ROOT_DIR/SensorLayer/Databases/DetectionCacheDB/Redis/K8s/ -n $NAMESPACE

# Sensor Layer
kubectl apply -f $ROOT_DIR/SensorLayer/Services/DetectionService/K8s/ -n $NAMESPACE
kubectl apply -f $ROOT_DIR/SensorLayer/Services/SensorService/K8s/ -n $NAMESPACE

# Commented out (not ready yet)
#kubectl apply -f $ROOT_DIR/UserLayer/Services/UserService/K8s/ -n $NAMESPACE
#kubectl apply -f $ROOT_DIR/UserLayer/Services/NotificationService/K8s/ -n $NAMESPACE
#kubectl apply -f $ROOT_DIR/LogLayer/Services/LogService/K8s/ -n $NAMESPACE
#kubectl apply -f $ROOT_DIR/TrafficLayer/Services/TrafficAnalyticsService/K8s/ -n $NAMESPACE
#kubectl apply -f $ROOT_DIR/TrafficLayer/Services/TrafficLightCoordinatorService/K8s/ -n $NAMESPACE
#kubectl apply -f $ROOT_DIR/TrafficLayer/Services/IntersectionControllerService/K8s/ -n $NAMESPACE
#kubectl apply -f $ROOT_DIR/TrafficLayer/Services/TrafficLightControllerService/K8s/ -n $NAMESPACE

# Wait for ingress-nginx
echo "⏳ Waiting for ingress-nginx controller to be ready..."
until [[ $(kubectl get pods -n ingress-nginx -l app.kubernetes.io/component=controller -o 'jsonpath={..status.conditions[?(@.type=="Ready")].status}') == "True" ]]; do
  echo "   → ingress-nginx still starting..."
  sleep 5
done

# Apply ingress
kubectl apply -f $ROOT_DIR/k8s.ingress.yaml -n $NAMESPACE

echo ""
echo "✅ STLS deployed successfully on Minikube."
echo "👉 Add to /etc/hosts if not already:"
echo "   $(minikube ip) uniwa-stls.local"
echo ""
echo "Test example:"
echo "   curl http://uniwa-stls.local/sensor/agiou-spyridonos/"
