FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 🔹 Copy project files (LogStore, LogData, UserMessages)
COPY ["LogStore/LogStore.csproj", "LogStore/"]
COPY ["LogData/LogData.csproj", "LogData/"]
COPY ["../../UserMessages/UserMessages.csproj", "../UserMessages/"]

# 🔹 Restore dependencies
RUN dotnet restore "LogStore/LogStore.csproj"

# 🔹 Copy all source code (AFTER restore)
COPY . .
COPY ../../UserMessages ../UserMessages

# 🔹 Build
RUN dotnet build "LogStore/LogStore.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LogStore/LogStore.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogStore.dll"]
