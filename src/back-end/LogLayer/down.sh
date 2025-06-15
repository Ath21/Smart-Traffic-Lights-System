#!/bin/bash

down_log_layer()
{
    echo "🛑  ------- Stopping Log Service... -------"
    bash ./LogLayer/LogService/down.sh
}

down_log_layer