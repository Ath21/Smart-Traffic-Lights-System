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

# -----------------------
# Start / Stop / Reset
# -----------------------
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
    minikube stop
    minikube delete
    minikube start --driver=docker --addons=ingress --cpus=4 --memory=8192
    ;;
  *)
    usage
    exit 1
    ;;
esac

echo "📦 Deploying Smart Traffic Lights System (STLS)..."

# -----------------------
# Create namespace if not exists
# -----------------------
kubectl get namespace $NAMESPACE >/dev/null 2>&1 || kubectl create namespace $NAMESPACE

# -----------------------
# Deploy Databases
# -----------------------
DB_DIRS=(
  "$ROOT_DIR/LogLayer/Databases/LogDB/Mongo/K8s"
  "$ROOT_DIR/SensorLayer/Databases/DetectionDB/Mongo/K8s"
  "$ROOT_DIR/SensorLayer/Databases/DetectionCacheDB/Redis/K8s"
  "$ROOT_DIR/TrafficLayer/Databases/TrafficAnalyticsDB/PostgreSQL/K8s"
  "$ROOT_DIR/TrafficLayer/Databases/TrafficLightDB/MSSQL/K8s"
  "$ROOT_DIR/TrafficLayer/Databases/TrafficLightCacheDB/Redis/K8s"
  "$ROOT_DIR/UserLayer/Databases/UserDB/MSSQL/K8s"
  "$ROOT_DIR/UserLayer/Databases/NotificationDB/Mongo/K8s"
)

for db in "${DB_DIRS[@]}"; do
  echo "Applying database manifests in $db..."
  kubectl apply -f "$db" -n $NAMESPACE
done

# -----------------------
# Deploy RabbitMQ
# -----------------------
echo "Deploying RabbitMQ..."
kubectl apply -f "$ROOT_DIR/MessageLayer/RabbitMQ/K8s/" -n $NAMESPACE

# -----------------------
# Wait for ready pods
# -----------------------
wait_for_ready() {
  local label="$1"
  local name="$2"
  echo "⏳ Waiting for $name pod..."
  until kubectl get pods -n $NAMESPACE -l "$label" -o 'jsonpath={..status.conditions[?(@.type=="Ready")].status}' 2>/dev/null | grep -q True; do
    echo "   → $name not ready yet..."
    sleep 5
  done
  echo "✅ $name is ready."
}

echo "⏳ Waiting for all databases and RabbitMQ to be ready..."
wait_for_ready "app=log-db" "LogDB"
wait_for_ready "app=detection-db" "DetectionDB"
wait_for_ready "app=detection-cache-db" "DetectionCacheDB"
wait_for_ready "app=traffic-analytics-db" "TrafficAnalyticsDB"
wait_for_ready "app=traffic-light-db" "TrafficLightDB"
wait_for_ready "app=traffic-light-cache-db" "TrafficLightCacheDB"
wait_for_ready "app=user-db" "UserDB"
wait_for_ready "app=notification-db" "NotificationDB"
wait_for_ready "app=rabbitmq" "RabbitMQ"
echo "✅ All Databases and RabbitMQ are ready."

# -----------------------
# Function to apply YAMLs recursively
# -----------------------
apply_yaml_recursively() {
  local dir="$1"
  echo "Applying YAMLs under $dir..."
  find "$dir" -type f -name '*.yaml' | while read -r yaml; do
    echo "Applying $yaml..."
    kubectl apply -f "$yaml" -n $NAMESPACE
  done
}

# -----------------------
# Deploy Sensor + Detection Services
# -----------------------
SENSOR_DETECTION_DIRS=(
  "$ROOT_DIR/SensorLayer/Services/SensorService/K8s"
  "$ROOT_DIR/SensorLayer/Services/DetectionService/K8s"
)

for dir in "${SENSOR_DETECTION_DIRS[@]}"; do
  apply_yaml_recursively "$dir"
done

# -----------------------
# Deploy Intersection + Traffic Light Controller Services
# -----------------------
CONTROLLER_DIRS=(
  "$ROOT_DIR/TrafficLayer/Services/IntersectionControllerService/K8s"
  "$ROOT_DIR/TrafficLayer/Services/TrafficLightControllerService/K8s"
  "$ROOT_DIR/TrafficLayer/Services/TrafficLightCoordinatorService/K8s"
)

for dir in "${CONTROLLER_DIRS[@]}"; do
  apply_yaml_recursively "$dir"
done

# -----------------------
# Deploy other services (UserService, NotificationService, TrafficAnalyticsService, LogService)
# -----------------------
OTHER_SERVICES=(
  "$ROOT_DIR/UserLayer/Services/UserService/K8s"
  "$ROOT_DIR/UserLayer/Services/NotificationService/K8s"
  "$ROOT_DIR/TrafficLayer/Services/TrafficAnalyticsService/K8s"
  "$ROOT_DIR/LogLayer/Services/LogService/K8s"
)

for dir in "${OTHER_SERVICES[@]}"; do
  apply_yaml_recursively "$dir"
done

# -----------------------
# Ensure UserService starts last
# -----------------------
echo "⏳ Ensuring UserDB and RabbitMQ ready before UserService..."
wait_for_ready "app=user-db" "UserDB"
wait_for_ready "app=rabbitmq" "RabbitMQ"
echo "✅ UserService dependencies confirmed."

# -----------------------
# Ingress
# -----------------------
echo "⏳ Waiting for ingress-nginx controller to be ready..."
until [[ $(kubectl get pods -n ingress-nginx -l app.kubernetes.io/component=controller -o 'jsonpath={..status.conditions[?(@.type=="Ready")].status}') == "True" ]]; do
  echo "   → ingress-nginx still starting..."
  sleep 5
done

kubectl apply -f "$ROOT_DIR/k8s.ingress.yaml" -n $NAMESPACE

echo ""
echo "✅ STLS deployed successfully on Minikube."
echo "👉 Add to /etc/hosts if not already:"
echo "   $(minikube ip) uniwa-stls.local"
echo ""
echo "Test example:"
echo "   curl http://uniwa-stls.local/sensor/agiou-spyridonos/"
