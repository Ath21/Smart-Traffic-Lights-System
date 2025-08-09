#!/bin/bash

# ================================
# 📌 Resolve script path
# ================================
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# ================================
# 🧠 Helper: Run script if exists
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
    local SERVICE=""

    while [[ "$#" -gt 0 ]]; do
        case "$1" in
            --service=*) SERVICE="${1#*=}" ;;
            *) echo "❌ Unknown option: $1"; exit 1 ;;
        esac
        shift
    done

    if [[ -n "$SERVICE" ]]; then
        local path="$SCRIPT_DIR/$SERVICE/down$SERVICE.sh"
        if [[ ! -f "$path" ]]; then
            echo "❌ Unknown service '$SERVICE' in User Layer."
            exit 1
        fi
        echo "🛑 Stopping ONLY $SERVICE in User Layer..."
        try_stop "$path"
    else
        echo "🛑 Stopping ALL services in User Layer..."
        try_stop "$SCRIPT_DIR/UserService/downUserService.sh"
        try_stop "$SCRIPT_DIR/NotificationService/downNotificationService.sh"
    fi

    echo "✅ User Layer shutdown complete."
    exit 0
}

main "$@"
