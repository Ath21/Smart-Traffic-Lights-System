#!/bin/bash

set -e  # Exit immediately on error

# ================================
# 📌 Resolve script path
# ================================
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# ================================
# 🧠 Run child script if valid
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
# 🧩 Main
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
            echo "❌ Unknown service '$SERVICE' in Traffic Layer."
            exit 1
        fi
        echo "🛑 Stopping ONLY $SERVICE in Traffic Layer..."
        try_stop "$SCRIPT_DIR/$SERVICE/down$SERVICE.sh"
    else
        echo "🛑 Stopping ALL services in Traffic Layer..."
        try_stop "$SCRIPT_DIR/IntersectionControllerService/downIntersectionControllerService.sh"
        try_stop "$SCRIPT_DIR/TrafficLightControllerService/downTrafficLightControllerService.sh"
        try_stop "$SCRIPT_DIR/TrafficLightCoordinatorService/downTrafficLightCoordinatorService.sh"
        try_stop "$SCRIPT_DIR/TrafficAnalyticsService/downTrafficAnalyticsService.sh"
    fi
}

main "$@"
