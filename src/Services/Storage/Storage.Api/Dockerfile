# =============================================================================
# Storage Service Dockerfile
# Multi-stage build for optimized production image
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:2205fd3a7cf5e422c58398a9968aa8b5041fdbb47c76574fe631082e4cbfa2b8 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Web/BuildingBlocks.Web.csproj", "src/BuildingBlocks/BuildingBlocks.Web/"]
COPY ["src/Services/Storage/Storage.Api/Storage.Api.csproj", "src/Services/Storage/Storage.Api/"]
COPY ["src/Services/Storage/Storage.Application/Storage.Application.csproj", "src/Services/Storage/Storage.Application/"]
COPY ["src/Services/Storage/Storage.Domain/Storage.Domain.csproj", "src/Services/Storage/Storage.Domain/"]
COPY ["src/Services/Storage/Storage.Infrastructure/Storage.Infrastructure.csproj", "src/Services/Storage/Storage.Infrastructure/"]
COPY ["src/Services/Storage/Storage.Contracts/StorageService.Contracts.csproj", "src/Services/Storage/Storage.Contracts/"]
COPY ["src/Contracts/Common.Contracts/Common.Contracts.csproj", "src/Contracts/Common.Contracts/"]
COPY ["src/Services/Job/Job.Contracts/JobService.Contracts.csproj", "src/Services/Job/Job.Contracts/"]

RUN dotnet restore "src/Services/Storage/Storage.Api/Storage.Api.csproj"

COPY ["src/", "src/"]

WORKDIR "/src/src/Services/Storage/Storage.Api"
RUN dotnet publish "Storage.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine@sha256:1bd70a86abe101145238b07e7462e5f9e5ab0fa50c6fdeff3c3ac6941da56939 AS runtime
ARG BUILD_VERSION=dev
ARG VCS_REF=unknown
LABEL org.opencontainers.image.version=$BUILD_VERSION \
      org.opencontainers.image.revision=$VCS_REF
WORKDIR /app

RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -s /bin/sh -D appuser

RUN apk add --no-cache curl tzdata icu-libs

RUN mkdir -p /app/uploads && chown appuser:appgroup /app/uploads

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

USER appuser

ENTRYPOINT ["dotnet", "Storage.Api.dll"]
