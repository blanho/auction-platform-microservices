# ==============================================================================
# Auction Service Dockerfile
# Multi-stage build for optimized production image
# ==============================================================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy project files for restore (enables better layer caching)
COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Web/BuildingBlocks.Web.csproj", "src/BuildingBlocks/BuildingBlocks.Web/"]
COPY ["src/Services/Auction/Auction.Domain/Auctions.Domain.csproj", "src/Services/Auction/Auction.Domain/"]
COPY ["src/Services/Auction/Auction.Application/Auctions.Application.csproj", "src/Services/Auction/Auction.Application/"]
COPY ["src/Services/Auction/Auction.Infrastructure/Auctions.Infrastructure.csproj", "src/Services/Auction/Auction.Infrastructure/"]
COPY ["src/Services/Auction/Auction.Api/Auctions.Api.csproj", "src/Services/Auction/Auction.Api/"]
COPY ["src/Services/Auction/Auction.Contracts/Auctions.Contracts.csproj", "src/Services/Auction/Auction.Contracts/"]
COPY ["src/Contracts/BidService.Contracts/BidService.Contracts.csproj", "src/Contracts/BidService.Contracts/"]

# Restore dependencies
RUN dotnet restore "src/Services/Auction/Auction.Api/Auctions.Api.csproj" --runtime linux-musl-x64

# Copy source code
COPY ["src/", "src/"]

# Build and publish
WORKDIR "/src/src/Services/Auction/Auction.Api"
RUN dotnet publish "Auctions.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --runtime linux-musl-x64 \
    --self-contained false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# Security: Run as non-root user
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -s /bin/sh -D appuser

# Install runtime dependencies
RUN apk add --no-cache curl tzdata icu-libs

# Environment variables
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

USER appuser

ENTRYPOINT ["dotnet", "Auctions.Api.dll"]

