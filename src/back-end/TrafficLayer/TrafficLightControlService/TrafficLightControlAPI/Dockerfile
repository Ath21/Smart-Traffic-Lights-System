# Base runtime image (final container)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Install required libraries
RUN apt-get update && apt-get install -y libgssapi-krb5-2

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution sources
COPY TrafficLayer TrafficLayer/
COPY LogLayer/LogMessages LogLayer/LogMessages/

# Restore dependencies
RUN dotnet restore TrafficLayer/TrafficLightControlService/TrafficLightControlAPI/TrafficLightControlStore/TrafficLightControlStore.csproj

# Build project
RUN dotnet build TrafficLayer/TrafficLightControlService/TrafficLightControlAPI/TrafficLightControlStore/TrafficLightControlStore.csproj -c $BUILD_CONFIGURATION -o /app/build

# Publish project
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish TrafficLayer/TrafficLightControlService/TrafficLightControlAPI/TrafficLightControlStore/TrafficLightControlStore.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrafficLightControlStore.dll"]
