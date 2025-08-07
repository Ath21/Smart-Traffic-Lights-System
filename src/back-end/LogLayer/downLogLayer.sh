#!/bin/bash

# ================================
# 📌 Resolve script directory
# ================================
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# ================================
# 🧠 Try to stop service script
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
# 🛑 Main
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
            echo "❌ Unknown service '$SERVICE' in Log Layer."
            exit 1
        fi

        echo "🛑 Stopping ONLY $SERVICE in Log Layer..."
        try_stop "$SCRIPT_DIR/$SERVICE/down${SERVICE}.sh"
    else
        echo "🛑 Stopping ALL services in Log Layer..."
        try_stop "$SCRIPT_DIR/LogService/downLogService.sh"
    fi

    exit 0
}

main "$@"
