#!/bin/bash

# ================================
# 🛑 Stop User Layer
# ================================
stop_user_layer() 
{
    echo "🛑 Stopping User Service..."
    bash ./UserLayer/UserService/downUserService.sh
}

# ================================
# 🧩 Main Script Execution
# ================================
stop_user_layer

exit 0
