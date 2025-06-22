# 🔹 Βάση runtime image (για το τελικό container)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# 🔹 Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY UserLayer UserLayer/

# 🔹 Restore
RUN dotnet restore UserLayer/NotificationService/NotificationAPI/NotificationStore/NotificationStore.csproj

# 🔹 Build
RUN dotnet build UserLayer/NotificationService/NotificationAPI/NotificationStore/NotificationStore.csproj -c $BUILD_CONFIGURATION -o /app/build

# 🔹 Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish UserLayer/NotificationService/NotificationAPI/NotificationStore/NotificationStore.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 🔹 Τελικό image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationStore.dll"]
