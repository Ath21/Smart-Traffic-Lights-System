# Base runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy required project folders
COPY TrafficLayer TrafficLayer/
COPY SensorLayer/SensorMessages SensorLayer/SensorMessages/
COPY LogLayer/LogMessages LogLayer/LogMessages/

# Restore dependencies
RUN dotnet restore TrafficLayer/TrafficAnalyticsService/TrafficAnalyticsAPI/TrafficAnalyticsStore/TrafficAnalyticsStore.csproj

# Build the project
RUN dotnet build TrafficLayer/TrafficAnalyticsService/TrafficAnalyticsAPI/TrafficAnalyticsStore/TrafficAnalyticsStore.csproj -c $BUILD_CONFIGURATION -o /app/build

# Publish the project
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish TrafficLayer/TrafficAnalyticsService/TrafficAnalyticsAPI/TrafficAnalyticsStore/TrafficAnalyticsStore.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrafficAnalyticsStore.dll"]
