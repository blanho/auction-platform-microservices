# ==============================================================================
# Bidding Service Dockerfile
# ==============================================================================

FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:2205fd3a7cf5e422c58398a9968aa8b5041fdbb47c76574fe631082e4cbfa2b8 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

# Copy project files for restore
COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Web/BuildingBlocks.Web.csproj", "src/BuildingBlocks/BuildingBlocks.Web/"]
COPY ["src/Contracts/Common.Contracts/Common.Contracts.csproj", "src/Contracts/Common.Contracts/"]
COPY ["src/Services/Bidding/Bidding.Domain/Bidding.Domain.csproj", "src/Services/Bidding/Bidding.Domain/"]
COPY ["src/Services/Bidding/Bidding.Application/Bidding.Application.csproj", "src/Services/Bidding/Bidding.Application/"]
COPY ["src/Services/Bidding/Bidding.Infrastructure/Bidding.Infrastructure.csproj", "src/Services/Bidding/Bidding.Infrastructure/"]
COPY ["src/Services/Bidding/Bidding.Api/Bidding.Api.csproj", "src/Services/Bidding/Bidding.Api/"]
COPY ["src/Services/Bidding/Bidding.Contracts/BidService.Contracts.csproj", "src/Services/Bidding/Bidding.Contracts/"]
COPY ["src/Services/Auction/Auction.Contracts/AuctionService.Contracts.csproj", "src/Services/Auction/Auction.Contracts/"]

RUN dotnet restore "src/Services/Bidding/Bidding.Api/Bidding.Api.csproj"

COPY ["src/", "src/"]

WORKDIR "/src/src/Services/Bidding/Bidding.Api"
RUN dotnet publish "Bidding.Api.csproj" \
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
