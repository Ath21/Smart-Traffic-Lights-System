#!/bin/bash

down_user_layer()
{
    echo "🛑  ------- Stopping User Service... -------"
    bash ./UserLayer/UserService/down.sh
}

down_user_layer