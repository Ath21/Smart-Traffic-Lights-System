#!/bin/bash

# ================================
# 🚀 Start User Layer
# ================================
start_user_layer() 
{
    echo "🚀 Starting User Service..."
    bash ./UserLayer/UserService/upUserService.sh
}

# ================================
# 🧩 Main Script Execution
# ================================
start_user_layer

exit 0
