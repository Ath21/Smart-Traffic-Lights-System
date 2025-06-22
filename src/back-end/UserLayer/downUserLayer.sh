#!/bin/bash

# ================================
# 🛑 Stop User Layer
# ================================
stop_user_layer() 
{
    echo "🛑 Stopping User Service..."
    bash ./UserLayer/UserService/downUserService.sh

    echo "🛑 Stopping Notification Service..."
    bash ./UserLayer/NotificationService/downNotificationService.sh
}

# ================================
# 🧩 Main Script Execution
# ================================
stop_user_layer

exit 0
