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
  "LogLayer/Databases/LogDB/Mongo/K8s"
  "SensorLayer/Databases/DetectionDB/Mongo/K8s"
  "SensorLayer/Databases/DetectionCacheDB/Redis/K8s"
  "TrafficLayer/Databases/TrafficAnalyticsDB/PostgreSQL/K8s"
  "TrafficLayer/Databases/TrafficLightDB/MSSQL/K8s"
  "TrafficLayer/Databases/TrafficLightCacheDB/Redis/K8s"
  "UserLayer/Databases/UserDB/MSSQL/K8s"
  "UserLayer/Databases/NotificationDB/Mongo/K8s"
)

for db in "${DB_DIRS[@]}"; do
    echo "Applying database manifests in $db..."
    kubectl apply -f "$ROOT_DIR/$db" -n $NAMESPACE
done

# -----------------------
# Deploy RabbitMQ
# -----------------------
echo "Deploying RabbitMQ..."
kubectl apply -f "$ROOT_DIR/MessageLayer/RabbitMQ/K8s/" -n $NAMESPACE

# -----------------------
# Wait for DBs and RabbitMQ to be ready
# -----------------------
echo "⏳ Waiting for database and RabbitMQ pods..."
for pod in detection-db detection-cache-db traffic-analytics-db traffic-light-db traffic-light-cache-db user-db log-db rabbitmq; do
    until kubectl get pods -n $NAMESPACE -l app.kubernetes.io/name=$pod -o 'jsonpath={..status.conditions[?(@.type=="Ready")].status}' 2>/dev/null | grep -q True; do
        echo "   → $pod not ready yet..."
        sleep 5
    done
done

echo "✅ Databases and RabbitMQ are ready."

# -----------------------
# Deploy Sensor & Detection Services (per intersection)
# -----------------------
SENSOR_DETECTION_DIRS=(
  "SensorLayer/Services/SensorService/Compose"
  "SensorLayer/Services/DetectionService/Compose"
)

for dir in "${SENSOR_DETECTION_DIRS[@]}"; do
    for yaml in $ROOT_DIR/$dir/*.yaml; do
        echo "Applying $yaml..."
        kubectl apply -f "$yaml" -n $NAMESPACE
    done
done

# -----------------------
# Deploy Intersection Controller Service (per intersection)
# -----------------------
IC_DIR="$ROOT_DIR/TrafficLayer/Services/IntersectionControllerService/K8s/Deployment"
for inter_dir in $IC_DIR/*; do
    for yaml in $inter_dir/*.yaml; do
        echo "Applying $yaml..."
        kubectl apply -f "$yaml" -n $NAMESPACE
    done
done

# -----------------------
# Deploy Traffic Light Controller Service (per light)
# -----------------------
TLC_DIR="$ROOT_DIR/TrafficLayer/Services/TrafficLightControllerService/K8s/Deployment"
for intersection in $TLC_DIR/*; do
    for yaml in $intersection/*.yaml; do
        echo "Applying $yaml..."
        kubectl apply -f "$yaml" -n $NAMESPACE
    done
done

# -----------------------
# Deploy other services
# -----------------------
OTHER_SERVICES=(
  "TrafficLayer/Services/TrafficLightCoordinatorService/K8s/"
  "TrafficLayer/Services/TrafficAnalyticsService/K8s/"
  "UserLayer/Services/UserService/K8s/"
  "UserLayer/Services/NotificationService/K8s/"
  "LogLayer/Services/LogService/K8s/"
)

for svc in "${OTHER_SERVICES[@]}"; do
    echo "Applying $svc..."
    kubectl apply -f "$ROOT_DIR/$svc" -n $NAMESPACE
done

# -----------------------
# Wait for ingress-nginx
# -----------------------
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
