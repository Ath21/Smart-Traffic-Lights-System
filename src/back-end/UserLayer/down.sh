#!/bin/bash

down_user_layer()
{
    echo "🛑  ------- Stopping User Service... -------"
    bash ./UserLayer/UserService/down.sh

    echo "🛑  ------- Stopping Log Service... -------"
    bash ./UserLayer/LogService/down.sh
}

down_user_layer