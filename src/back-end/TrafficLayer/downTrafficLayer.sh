#!/bin/bash

# ================================
# 🛑 Stop Traffic Layer
# ================================
stop_traffic_layer() 
{
    echo "🛑 Stopping Traffic Data Analytics Service..."
    bash ./TrafficLayer/TrafficDataAnalyticsService/downTrafficDataAnalyticsService.sh
}

# ================================
# 🧩 Main Script Execution
# ================================
stop_traffic_layer

exit 0
