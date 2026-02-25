# ==============================================================================
# Identity Service Dockerfile
# ==============================================================================

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Web/BuildingBlocks.Web.csproj", "src/BuildingBlocks/BuildingBlocks.Web/"]
COPY ["src/Services/Identity/Identity.Api/Identity.Api.csproj", "src/Services/Identity/Identity.Api/"]
COPY ["src/Services/Identity/Identity.Contracts/Identity.Contracts.csproj", "src/Services/Identity/Identity.Contracts/"]
COPY ["src/Contracts/NotificationService.Contracts/NotificationService.Contracts.csproj", "src/Contracts/NotificationService.Contracts/"]

RUN dotnet restore "src/Services/Identity/Identity.Api/Identity.Api.csproj" --runtime linux-musl-x64

COPY ["src/", "src/"]

WORKDIR "/src/src/Services/Identity/Identity.Api"
RUN dotnet publish "Identity.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --runtime linux-musl-x64 \
    --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
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

ENTRYPOINT ["dotnet", "Identity.Api.dll"]
