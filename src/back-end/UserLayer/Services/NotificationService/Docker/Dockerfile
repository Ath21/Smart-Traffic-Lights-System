# Runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

RUN apt-get update && apt-get install -y libgssapi-krb5-2

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy source projects
COPY UserLayer UserLayer/
COPY TrafficLayer/TrafficMessages TrafficLayer/TrafficMessages/
COPY LogLayer/LogMessages LogLayer/LogMessages/

# Restore dependencies
RUN dotnet restore UserLayer/NotificationService/NotificationAPI/NotificationStore/NotificationStore.csproj

# Build the application
RUN dotnet build UserLayer/NotificationService/NotificationAPI/NotificationStore/NotificationStore.csproj -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish UserLayer/NotificationService/NotificationAPI/NotificationStore/NotificationStore.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationStore.dll"]
