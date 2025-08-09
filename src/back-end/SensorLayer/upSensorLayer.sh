#!/bin/bash

# ================================
# 📌 Resolve script path
# ================================
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# ================================
# 🧠 Run script if it exists
# ================================
try_start() 
{
    local script="$1"
    if [ -x "$script" ]; then
        bash "$script"
    else
        echo "⚠️ Cannot execute $script. Skipping."
    fi
}

# ================================
# 🚀 Main Execution
# ================================
main() 
{
    SERVICE=""

    while [[ "$#" -gt 0 ]]; do
        case "$1" in
            --service=*) SERVICE="${1#*=}" ;;
            *) echo "❌ Unknown option: $1"; exit 1 ;;
        esac
        shift
    done

    if [[ -n "$SERVICE" ]]; then
        if [[ ! -d "$SCRIPT_DIR/$SERVICE" ]]; then
            echo "❌ Unknown service '$SERVICE' in Sensor Layer."
            exit 1
        fi
        echo "🚀 Starting ONLY $SERVICE in Sensor Layer..."
        try_start "$SCRIPT_DIR/$SERVICE/up$SERVICE.sh"
    else
        echo "🚀 Starting ALL services in Sensor Layer..."
        try_start "$SCRIPT_DIR/VehicleDetectionService/upVehicleDetectionService.sh"
        try_start "$SCRIPT_DIR/EmergencyVehicleDetectionService/upEmergencyVehicleDetectionService.sh"
        try_start "$SCRIPT_DIR/PublicTransportDetectionService/upPublicTransportDetectionService.sh"
        try_start "$SCRIPT_DIR/PedestrianDetectionService/upPedestrianDetectionService.sh"
        try_start "$SCRIPT_DIR/CyclistDetectionService/upCyclistDetectionService.sh"
        try_start "$SCRIPT_DIR/IncidentDetectionService/upIncidentDetectionService.sh"
    fi
}

main "$@"
