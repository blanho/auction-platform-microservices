# ==============================================================================
# Bidding Service Dockerfile
# ==============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy project files for restore
COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Web/BuildingBlocks.Web.csproj", "src/BuildingBlocks/BuildingBlocks.Web/"]
COPY ["src/Services/Bidding/Bidding.Domain/Bidding.Domain.csproj", "src/Services/Bidding/Bidding.Domain/"]
COPY ["src/Services/Bidding/Bidding.Application/Bidding.Application.csproj", "src/Services/Bidding/Bidding.Application/"]
COPY ["src/Services/Bidding/Bidding.Infrastructure/Bidding.Infrastructure.csproj", "src/Services/Bidding/Bidding.Infrastructure/"]
COPY ["src/Services/Bidding/Bidding.Api/Bidding.Api.csproj", "src/Services/Bidding/Bidding.Api/"]
COPY ["src/Services/Bidding/Bidding.Contracts/Bidding.Contracts.csproj", "src/Services/Bidding/Bidding.Contracts/"]
COPY ["src/Contracts/AuctionService.Contracts/AuctionService.Contracts.csproj", "src/Contracts/AuctionService.Contracts/"]

RUN dotnet restore "src/Services/Bidding/Bidding.Api/Bidding.Api.csproj" --runtime linux-musl-x64

COPY ["src/", "src/"]

WORKDIR "/src/src/Services/Bidding/Bidding.Api"
RUN dotnet publish "Bidding.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --runtime linux-musl-x64 \
    --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -s /bin/sh -D appuser

RUN apk add --no-cache curl tzdata icu-libs

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

USER appuser

ENTRYPOINT ["dotnet", "Bidding.Api.dll"]
