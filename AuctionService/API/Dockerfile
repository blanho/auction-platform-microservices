FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Auction.sln", "./"]
COPY ["AuctionService/API/AuctionService.API.csproj", "AuctionService/API/"]
COPY ["AuctionService/Application/AuctionService.Application.csproj", "AuctionService/Application/"]
COPY ["AuctionService/Domain/AuctionService.Domain.csproj", "AuctionService/Domain/"]
COPY ["AuctionService/Infrastructure/AuctionService.Infrastructure.csproj", "AuctionService/Infrastructure/"]
COPY ["Common/Common.Core/Common.Core.csproj", "Common/Common.Core/"]
COPY ["Common/Common.Domain/Common.Domain.csproj", "Common/Common.Domain/"]
COPY ["Common/Common.Caching/Common.Caching.csproj", "Common/Common.Caching/"]
COPY ["Common/Common.Logging/Common.Logging.csproj", "Common/Common.Logging/"]
COPY ["Common/Common.Messaging/Common.Messaging.csproj", "Common/Common.Messaging/"]
COPY ["Common/Common.Repository/Common.Repository.csproj", "Common/Common.Repository/"]
COPY ["Common/Common.OpenApi/Common.OpenApi.csproj", "Common/Common.OpenApi/"]
COPY ["Common/Common.Security/Common.Security.csproj", "Common/Common.Security/"]

RUN dotnet restore "AuctionService/API/AuctionService.API.csproj"

COPY . .

WORKDIR "/src/AuctionService/API"
RUN dotnet publish "AuctionService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "AuctionService.API.dll"]
