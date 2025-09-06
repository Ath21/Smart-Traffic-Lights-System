# Base image for the runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all project layers to keep relative paths
COPY LogLayer LogLayer/

# Restore dependencies
RUN dotnet restore LogLayer/LogService/LogAPI/LogStore/LogStore.csproj

# Build the project
RUN dotnet build LogLayer/LogService/LogAPI/LogStore/LogStore.csproj -c $BUILD_CONFIGURATION -o /app/build

# Publish the project
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish LogLayer/LogService/LogAPI/LogStore/LogStore.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogStore.dll"]
