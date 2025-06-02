FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 🔹 Copy project files (από root project context!)
COPY ["UserStore/UserStore.csproj", "UserStore/"]
COPY ["UserData/UserData.csproj", "UserData/"]

# 🔹 Restore dependencies
RUN dotnet restore "UserStore/UserStore.csproj"

# 🔹 Copy all files (must come AFTER restore)
COPY . .

# 🔹 Build project
RUN dotnet build "UserStore/UserStore.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UserStore/UserStore.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserStore.dll"]
