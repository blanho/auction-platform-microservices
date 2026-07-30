# ==============================================================================
# Payment Service Dockerfile
# ==============================================================================

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Web/BuildingBlocks.Web.csproj", "src/BuildingBlocks/BuildingBlocks.Web/"]
COPY ["src/Contracts/Common.Contracts/Common.Contracts.csproj", "src/Contracts/Common.Contracts/"]
COPY ["src/Services/Payment/Payment.Domain/Payment.Domain.csproj", "src/Services/Payment/Payment.Domain/"]
COPY ["src/Services/Payment/Payment.Application/Payment.Application.csproj", "src/Services/Payment/Payment.Application/"]
COPY ["src/Services/Payment/Payment.Infrastructure/Payment.Infrastructure.csproj", "src/Services/Payment/Payment.Infrastructure/"]
COPY ["src/Services/Payment/Payment.Api/Payment.Api.csproj", "src/Services/Payment/Payment.Api/"]
COPY ["src/Services/Payment/Payment.Contracts/PaymentService.Contracts.csproj", "src/Services/Payment/Payment.Contracts/"]
COPY ["src/Services/Auction/Auction.Contracts/AuctionService.Contracts.csproj", "src/Services/Auction/Auction.Contracts/"]
COPY ["src/Services/Bidding/Bidding.Contracts/BidService.Contracts.csproj", "src/Services/Bidding/Bidding.Contracts/"]
COPY ["src/Services/Job/Job.Contracts/JobService.Contracts.csproj", "src/Services/Job/Job.Contracts/"]
COPY ["src/Orchestration/Orchestration.Contracts/OrchestrationService.Contracts.csproj", "src/Orchestration/Orchestration.Contracts/"]

RUN dotnet restore "src/Services/Payment/Payment.Api/Payment.Api.csproj" --runtime linux-musl-x64

COPY ["src/", "src/"]

WORKDIR "/src/src/Services/Payment/Payment.Api"
RUN dotnet publish "Payment.Api.csproj" \
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

ENTRYPOINT ["dotnet", "Payment.Api.dll"]
