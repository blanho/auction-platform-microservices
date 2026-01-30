# ==============================================================================
# Search Service Dockerfile
# Multi-stage build for optimized production image
# ==============================================================================

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy project files for restore
COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Web/BuildingBlocks.Web.csproj", "src/BuildingBlocks/BuildingBlocks.Web/"]
COPY ["src/Services/Search/Search.Api/Search.Api.csproj", "src/Services/Search/Search.Api/"]
COPY ["src/Contracts/AuctionService.Contracts/AuctionService.Contracts.csproj", "src/Contracts/AuctionService.Contracts/"]
COPY ["src/Contracts/BidService.Contracts/BidService.Contracts.csproj", "src/Contracts/BidService.Contracts/"]
COPY ["src/Contracts/SearchService.Contracts/SearchService.Contracts.csproj", "src/Contracts/SearchService.Contracts/"]

# Restore dependencies
RUN dotnet restore "src/Services/Search/Search.Api/Search.Api.csproj" --runtime linux-musl-x64

# Copy source code
COPY ["src/", "src/"]

# Build and publish
WORKDIR "/src/src/Services/Search/Search.Api"
RUN dotnet publish "Search.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --runtime linux-musl-x64 \
    --self-contained false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
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

ENTRYPOINT ["dotnet", "Search.Api.dll"]
