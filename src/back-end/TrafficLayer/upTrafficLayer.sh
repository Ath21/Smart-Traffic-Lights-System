#!/bin/bash

# ================================
# 🚀 Start Traffic Layer
# ================================
start_traffic_layer() 
{
    echo "🚀 Starting Traffic Data Analytics Service..."
    bash ./TrafficLayer/TrafficDataAnalyticsService/upTrafficDataAnalyticsService.sh
}

# ================================
# 🧩 Main Script Execution
# ================================
start_traffic_layer

exit 0
