# 🔹 Βάση για το τελικό runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# 🔹 Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 🔹 Αντιγραφή ολόκληρων layers ώστε να τηρούνται τα relative paths
COPY LogLayer LogLayer/
COPY UserLayer UserLayer/

# 🔹 Restore
RUN dotnet restore LogLayer/LogService/LogAPI/LogStore/LogStore.csproj

# 🔹 Build
RUN dotnet build LogLayer/LogService/LogAPI/LogStore/LogStore.csproj -c $BUILD_CONFIGURATION -o /app/build

# 🔹 Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish LogLayer/LogService/LogAPI/LogStore/LogStore.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 🔹 Τελικό image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogStore.dll"]
