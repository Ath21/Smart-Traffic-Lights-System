#!/bin/bash

# ================================
# 📌 Get script path
# ================================
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# ================================
# 🧠 Run sub-script safely
# ================================
try_start() {
    local script="$1"
    if [ -x "$script" ]; then
        bash "$script"
    else
        echo "⚠️ Cannot execute $script. Skipping."
    fi
}

# ================================
# 🚀 Main
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
        if [[ ! -d "$SCRIPT_DIR/$SERVICE" ]]; then
            echo "❌ Unknown service '$SERVICE' in Log Layer."
            exit 1
        fi
        echo "🚀 Starting ONLY $SERVICE in Log Layer..."
        try_start "$SCRIPT_DIR/$SERVICE/up$SERVICE.sh"
    else
        echo "🚀 Starting ALL services in Log Layer..."
        try_start "$SCRIPT_DIR/LogService/upLogService.sh"
    fi

    exit 0
}

main "$@"
