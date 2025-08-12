# 🔹 Βάση runtime image (για το τελικό container)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

RUN apt-get update && apt-get install -y libgssapi-krb5-2

# 🔹 Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY TrafficLayer TrafficLayer/

# 🔹 Restore
RUN dotnet restore TrafficLayer/TrafficLightControlService/TrafficLightControlAPI/TrafficLightControlStore/TrafficLightControlStore.csproj

# 🔹 Build
RUN dotnet build TrafficLayer/TrafficLightControlService/TrafficLightControlAPI/TrafficLightControlStore/TrafficLightControlStore.csproj -c $BUILD_CONFIGURATION -o /app/build

# 🔹 Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish TrafficLayer/TrafficLightControlService/TrafficLightControlAPI/TrafficLightControlStore/TrafficLightControlStore.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 🔹 Τελικό image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrafficLightControlStore.dll"]
