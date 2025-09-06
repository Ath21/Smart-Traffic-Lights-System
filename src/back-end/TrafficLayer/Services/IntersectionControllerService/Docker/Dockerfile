# Base runtime image (final container)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

RUN apt-get update && apt-get install -y libgssapi-krb5-2

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY SensorLayer/SensorMessages SensorLayer/SensorMessages/
COPY TrafficLayer TrafficLayer/
COPY LogLayer/LogMessages LogLayer/LogMessages/

# Restore dependencies
RUN dotnet restore TrafficLayer/IntersectionControllerService/IntersectionControllerAPI/IntersectionControllerStore/IntersectionControllerStore.csproj

# Build project
RUN dotnet build TrafficLayer/IntersectionControllerService/IntersectionControllerAPI/IntersectionControllerStore/IntersectionControllerStore.csproj -c $BUILD_CONFIGURATION -o /app/build

# Publish project
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish TrafficLayer/IntersectionControllerService/IntersectionControllerAPI/IntersectionControllerStore/IntersectionControllerStore.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IntersectionControllerStore.dll"]
