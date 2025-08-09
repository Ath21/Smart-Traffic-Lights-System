#!/bin/bash

# ================================
# 📌 Resolve script path
# ================================
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# ================================
# 🧠 Run script if it exists
# ================================
try_stop() 
{
    local script="$1"
    if [ -x "$script" ]; then
        bash "$script"
    else
        echo "⚠️ Cannot execute $script. Skipping."
    fi
}

# ================================
# 🛑 Main Execution
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
        echo "🛑 Stopping ONLY $SERVICE in Sensor Layer..."
        try_stop "$SCRIPT_DIR/$SERVICE/down$SERVICE.sh"
    else
        echo "🛑 Stopping ALL services in Sensor Layer..."
        try_stop "$SCRIPT_DIR/VehicleDetectionService/downVehicleDetectionService.sh"
        try_stop "$SCRIPT_DIR/EmergencyVehicleDetectionService/downEmergencyVehicleDetectionService.sh"
        try_stop "$SCRIPT_DIR/PublicTransportDetectionService/downPublicTransportDetectionService.sh"
        try_stop "$SCRIPT_DIR/PedestrianDetectionService/downPedestrianDetectionService.sh"
        try_stop "$SCRIPT_DIR/CyclistDetectionService/downCyclistDetectionService.sh"
        try_stop "$SCRIPT_DIR/IncidentDetectionService/downIncidentDetectionService.sh"
    fi
}

main "$@"
