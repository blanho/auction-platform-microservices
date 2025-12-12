FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY ["Auction.sln", "./"]
COPY ["NotificationService/API/NotificationService.API.csproj", "NotificationService/API/"]
COPY ["NotificationService/Application/NotificationService.Application.csproj", "NotificationService/Application/"]
COPY ["NotificationService/Domain/NotificationService.Domain.csproj", "NotificationService/Domain/"]
COPY ["NotificationService/Infrastructure/NotificationService.Infrastructure.csproj", "NotificationService/Infrastructure/"]
COPY ["Common/Common.Core/Common.Core.csproj", "Common/Common.Core/"]
COPY ["Common/Common.Domain/Common.Domain.csproj", "Common/Common.Domain/"]
COPY ["Common/Common.Logging/Common.Logging.csproj", "Common/Common.Logging/"]
COPY ["Common/Common.Messaging/Common.Messaging.csproj", "Common/Common.Messaging/"]
COPY ["Common/Common.Repository/Common.Repository.csproj", "Common/Common.Repository/"]
COPY ["Common/Common.OpenApi/Common.OpenApi.csproj", "Common/Common.OpenApi/"]
COPY ["Common/Common.Security/Common.Security.csproj", "Common/Common.Security/"]
COPY ["Common/Common.Scheduling/Common.Scheduling.csproj", "Common/Common.Scheduling/"]

RUN dotnet restore "NotificationService/API/NotificationService.API.csproj"

COPY . .

WORKDIR "/src/NotificationService/API"
RUN dotnet publish "NotificationService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "NotificationService.API.dll"]
