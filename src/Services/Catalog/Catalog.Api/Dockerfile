# ==============================================================================
# Catalog Service Dockerfile
# Multi-stage build for optimized production image
# ==============================================================================

# Stage 1: Build
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy centralized build files
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

# Copy project files for restore (enables better layer caching)
COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Web/BuildingBlocks.Web.csproj", "src/BuildingBlocks/BuildingBlocks.Web/"]
COPY ["src/Contracts/Common.Contracts/Common.Contracts.csproj", "src/Contracts/Common.Contracts/"]
COPY ["src/Services/Catalog/Catalog.Domain/Catalog.Domain.csproj", "src/Services/Catalog/Catalog.Domain/"]
COPY ["src/Services/Catalog/Catalog.Application/Catalog.Application.csproj", "src/Services/Catalog/Catalog.Application/"]
COPY ["src/Services/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "src/Services/Catalog/Catalog.Infrastructure/"]
COPY ["src/Services/Catalog/Catalog.Api/Catalog.Api.csproj", "src/Services/Catalog/Catalog.Api/"]
COPY ["src/Services/Catalog/Catalog.Contracts/Catalog.Contracts.csproj", "src/Services/Catalog/Catalog.Contracts/"]

# Restore dependencies
RUN dotnet restore "src/Services/Catalog/Catalog.Api/Catalog.Api.csproj" --runtime linux-musl-x64

# Copy source code
COPY ["src/", "src/"]

# Build and publish
WORKDIR "/src/src/Services/Catalog/Catalog.Api"
RUN dotnet publish "Catalog.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --runtime linux-musl-x64 \
    --self-contained false

# Stage 2: Runtime
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
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

ENTRYPOINT ["dotnet", "Catalog.Api.dll"]
