# Base runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all required folders
# Includes the entire UserLayer to ensure relative paths work correctly
COPY SensorLayer SensorLayer/
COPY LogLayer/LogMessages LogLayer/LogMessages/

# Restore dependencies
RUN dotnet restore SensorLayer/EmergencyVehicleDetectionService/EmergencyVehicleDetectionAPI/EmergencyVehicleDetectionStore/EmergencyVehicleDetectionStore.csproj

# Build project
RUN dotnet build SensorLayer/EmergencyVehicleDetectionService/EmergencyVehicleDetectionAPI/EmergencyVehicleDetectionStore/EmergencyVehicleDetectionStore.csproj -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish SensorLayer/EmergencyVehicleDetectionService/EmergencyVehicleDetectionAPI/EmergencyVehicleDetectionStore/EmergencyVehicleDetectionStore.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmergencyVehicleDetectionStore.dll"]
