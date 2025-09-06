# Base runtime image (final container)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Install dependencies required for Npgsql (Kerberos libraries)
RUN apt-get update && apt-get install -y libgssapi-krb5-2

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy source code
COPY TrafficLayer TrafficLayer/
COPY LogLayer/LogMessages LogLayer/LogMessages/

# Restore NuGet packages
RUN dotnet restore TrafficLayer/TrafficLightCoordinatorService/TrafficLightCoordinatorAPI/TrafficLightCoordinatorStore/TrafficLightCoordinatorStore.csproj

# Build project
RUN dotnet build TrafficLayer/TrafficLightCoordinatorService/TrafficLightCoordinatorAPI/TrafficLightCoordinatorStore/TrafficLightCoordinatorStore.csproj -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish TrafficLayer/TrafficLightCoordinatorService/TrafficLightCoordinatorAPI/TrafficLightCoordinatorStore/TrafficLightCoordinatorStore.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrafficLightCoordinatorStore.dll"]
