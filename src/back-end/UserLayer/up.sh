#!/bin/bash

up_user_layer()
{
    echo "🚀  +++++++ Starting User Service... +++++++"
    bash ./UserLayer/UserService/up.sh

    echo "🚀  +++++++ Starting Log Service... +++++++"
    bash ./UserLayer/LogService/up.sh
}

up_user_layer