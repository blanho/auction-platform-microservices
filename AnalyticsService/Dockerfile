FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and project files for restore
COPY ["Auction.sln", "./"]
COPY ["AnalyticsService/AnalyticsService.csproj", "AnalyticsService/"]
COPY ["Common/Common.Core/Common.Core.csproj", "Common/Common.Core/"]
COPY ["Common/Common.Domain/Common.Domain.csproj", "Common/Common.Domain/"]
COPY ["Common/Common.Messaging/Common.Messaging.csproj", "Common/Common.Messaging/"]
COPY ["Common/Common.Audit/Common.Audit.csproj", "Common/Common.Audit/"]
COPY ["Common/Common.Storage/Common.Storage.csproj", "Common/Common.Storage/"]
COPY ["Common/Common.Scheduling/Common.Scheduling.csproj", "Common/Common.Scheduling/"]

# Restore packages
RUN dotnet restore "AnalyticsService/AnalyticsService.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/AnalyticsService"
RUN dotnet build "AnalyticsService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AnalyticsService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create uploads directory
RUN mkdir -p /app/uploads/files /app/uploads/temp

COPY --from=publish /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "AnalyticsService.dll"]
