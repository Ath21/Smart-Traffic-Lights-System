#!/bin/bash

# ================================
# 🛑 Stop Log Layer
# ================================
stop_log_layer() 
{
    echo "🛑 Stopping Log Service..."
    bash ./LogLayer/LogService/downLogService.sh
}

# ================================
# 🧩 Main Script Execution
# ================================
stop_log_layer

exit 0
