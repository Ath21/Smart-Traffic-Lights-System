#!/bin/bash

# ================================
# 🚀 Start Log Layer
# ================================
start_log_layer() 
{
    echo "🚀 Starting Log Service..."
    bash ./LogLayer/LogService/upLogService.sh
}

# ================================
# 🧩 Main Script Execution
# ================================
start_log_layer

exit 0
