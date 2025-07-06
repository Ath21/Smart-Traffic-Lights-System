#!/bin/bash

# ================================
# 🧠 Helper: Run script if exists
# ================================
try_stop() {
    local script="$1"
    if [ -x "$script" ]; then
        bash "$script"
    else
        echo "⚠️ Cannot execute $script. Skipping."
    fi
}

# ================================
# 🧩 Main Execution
# ================================
main() {
    SERVICE=""

    while [[ "$#" -gt 0 ]]; do
        case "$1" in
            --service=*) SERVICE="${1#*=}" ;;
            *) echo "❌ Unknown option: $1"; exit 1 ;;
        esac
        shift
    done

    if [[ -n "$SERVICE" ]]; then
        echo "🛑 Stopping ONLY $SERVICE in User Layer..."
        try_stop "./UserLayer/$SERVICE/down$SERVICE.sh"
    else
        echo "🛑 Stopping ALL services in User Layer..."
        try_stop ./UserLayer/UserService/downUserService.sh
        try_stop ./UserLayer/NotificationService/downNotificationService.sh
    fi

    exit 0
}

main "$@"
