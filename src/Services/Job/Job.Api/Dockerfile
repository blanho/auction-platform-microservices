# =============================================================================
# Job Service Dockerfile
# Multi-stage build for optimized production image
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Web/BuildingBlocks.Web.csproj", "src/BuildingBlocks/BuildingBlocks.Web/"]
COPY ["src/Services/Job/Job.Domain/Job.Domain.csproj", "src/Services/Job/Job.Domain/"]
COPY ["src/Services/Job/Job.Application/Job.Application.csproj", "src/Services/Job/Job.Application/"]
COPY ["src/Services/Job/Job.Infrastructure/Job.Infrastructure.csproj", "src/Services/Job/Job.Infrastructure/"]
COPY ["src/Services/Job/Job.Api/Job.Api.csproj", "src/Services/Job/Job.Api/"]
COPY ["src/Contracts/JobService.Contracts/JobService.Contracts.csproj", "src/Contracts/JobService.Contracts/"]
COPY ["src/Contracts/Common.Contracts/Common.Contracts.csproj", "src/Contracts/Common.Contracts/"]

RUN dotnet restore "src/Services/Job/Job.Api/Job.Api.csproj" --runtime linux-musl-x64

COPY ["src/", "src/"]

WORKDIR "/src/src/Services/Job/Job.Api"
RUN dotnet publish "Job.Api.csproj" \
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

ENTRYPOINT ["dotnet", "Job.Api.dll"]
