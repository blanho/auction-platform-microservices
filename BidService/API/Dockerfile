FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Auction.sln", "./"]
COPY ["BidService/API/BidService.API.csproj", "BidService/API/"]
COPY ["BidService/Application/BidService.Application.csproj", "BidService/Application/"]
COPY ["BidService/Domain/BidService.Domain.csproj", "BidService/Domain/"]
COPY ["BidService/Infrastructure/BidService.Infrastructure.csproj", "BidService/Infrastructure/"]
COPY ["Common/Common.Core/Common.Core.csproj", "Common/Common.Core/"]
COPY ["Common/Common.Domain/Common.Domain.csproj", "Common/Common.Domain/"]
COPY ["Common/Common.Logging/Common.Logging.csproj", "Common/Common.Logging/"]
COPY ["Common/Common.Messaging/Common.Messaging.csproj", "Common/Common.Messaging/"]
COPY ["Common/Common.Repository/Common.Repository.csproj", "Common/Common.Repository/"]
COPY ["Common/Common.OpenApi/Common.OpenApi.csproj", "Common/Common.OpenApi/"]

RUN dotnet restore "BidService/API/BidService.API.csproj"

COPY . .

WORKDIR "/src/BidService/API"
RUN dotnet publish "BidService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "BidService.API.dll"]
