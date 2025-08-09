# 🔹 Βάση runtime image (για το τελικό container)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# 🔹 Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 🔹 Αντιγραφή όλων των απαραίτητων φακέλων
# ⚠️ Περιλαμβάνει ολόκληρο το UserLayer ώστε να πιάσει σωστά όλα τα relative paths
COPY SensorLayer SensorLayer/

# 🔹 Restore
RUN dotnet restore SensorLayer/PublicTransportDetectionService/EPublicTransportDetectionAPI/PublicTransportDetectionStore/PublicTransportDetectionStore.csproj

# 🔹 Build
RUN dotnet build SensorLayer/PublicTransportDetectionService/EPublicTransportDetectionAPI/PublicTransportDetectionStore/PublicTransportDetectionStore.csproj -c $BUILD_CONFIGURATION -o /app/build

# 🔹 Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish SensorLayer/PublicTransportDetectionService/EPublicTransportDetectionAPI/PublicTransportDetectionStore/PublicTransportDetectionStore.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 🔹 Τελικό image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PublicTransportDetectionStore.dll"]
