# Base runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage with SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy required project folders
COPY UserLayer UserLayer/
COPY TrafficLayer/TrafficMessages TrafficLayer/TrafficMessages/
COPY LogLayer/LogMessages LogLayer/LogMessages/

# Restore dependencies
RUN dotnet restore UserLayer/UserService/UserAPI/UserStore/UserStore.csproj

# Build the application
RUN dotnet build UserLayer/UserService/UserAPI/UserStore/UserStore.csproj -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish UserLayer/UserService/UserAPI/UserStore/UserStore.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserStore.dll"]
