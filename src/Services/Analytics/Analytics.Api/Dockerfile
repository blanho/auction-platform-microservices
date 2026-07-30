# =============================================================================
# Analytics Service Dockerfile
# Multi-stage build for optimized production image
# =============================================================================

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Web/BuildingBlocks.Web.csproj", "src/BuildingBlocks/BuildingBlocks.Web/"]
COPY ["src/Services/Analytics/Analytics.Domain/Analytics.Domain.csproj", "src/Services/Analytics/Analytics.Domain/"]
COPY ["src/Services/Analytics/Analytics.Application/Analytics.Application.csproj", "src/Services/Analytics/Analytics.Application/"]
COPY ["src/Services/Analytics/Analytics.Infrastructure/Analytics.Infrastructure.csproj", "src/Services/Analytics/Analytics.Infrastructure/"]
COPY ["src/Services/Analytics/Analytics.Api/Analytics.Api.csproj", "src/Services/Analytics/Analytics.Api/"]
COPY ["src/Contracts/Common.Contracts/Common.Contracts.csproj", "src/Contracts/Common.Contracts/"]
COPY ["src/Services/Auction/Auction.Contracts/AuctionService.Contracts.csproj", "src/Services/Auction/Auction.Contracts/"]
COPY ["src/Services/Identity/Identity.Contracts/IdentityService.Contracts.csproj", "src/Services/Identity/Identity.Contracts/"]
COPY ["src/Services/Bidding/Bidding.Contracts/BidService.Contracts.csproj", "src/Services/Bidding/Bidding.Contracts/"]
COPY ["src/Services/Payment/Payment.Contracts/PaymentService.Contracts.csproj", "src/Services/Payment/Payment.Contracts/"]

RUN dotnet restore "src/Services/Analytics/Analytics.Api/Analytics.Api.csproj" --runtime linux-musl-x64

COPY ["src/", "src/"]

WORKDIR "/src/src/Services/Analytics/Analytics.Api"
RUN dotnet publish "Analytics.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --runtime linux-musl-x64 \
    --self-contained false

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
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

ENTRYPOINT ["dotnet", "Analytics.Api.dll"]
