# ==============================================================================
# Storage Service Dockerfile
# ==============================================================================

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Web/BuildingBlocks.Web.csproj", "src/BuildingBlocks/BuildingBlocks.Web/"]
COPY ["src/Services/Storage/Storage.Domain/Storage.Domain.csproj", "src/Services/Storage/Storage.Domain/"]
COPY ["src/Services/Storage/Storage.Application/Storage.Application.csproj", "src/Services/Storage/Storage.Application/"]
COPY ["src/Services/Storage/Storage.Infrastructure/Storage.Infrastructure.csproj", "src/Services/Storage/Storage.Infrastructure/"]
COPY ["src/Services/Storage/Storage.Api/Storage.Api.csproj", "src/Services/Storage/Storage.Api/"]
COPY ["src/Services/Storage/Storage.Contracts/Storage.Contracts.csproj", "src/Services/Storage/Storage.Contracts/"]
COPY ["src/Contracts/AuctionService.Contracts/AuctionService.Contracts.csproj", "src/Contracts/AuctionService.Contracts/"]

RUN dotnet restore "src/Services/Storage/Storage.Api/Storage.Api.csproj" --runtime linux-musl-x64

COPY ["src/", "src/"]

WORKDIR "/src/src/Services/Storage/Storage.Api"
RUN dotnet publish "Storage.Api.csproj" \
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

ENTRYPOINT ["dotnet", "Storage.Api.dll"]
