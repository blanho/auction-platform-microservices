
FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:e70cdb7f80b0348f5cb85f19a8f670fca061f033d57eed12fa003d58b0e06317 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Web/BuildingBlocks.Web.csproj", "src/BuildingBlocks/BuildingBlocks.Web/"]
COPY ["src/Orchestration/Orchestration.Contracts/OrchestrationService.Contracts.csproj", "src/Orchestration/Orchestration.Contracts/"]
COPY ["src/Orchestration/Orchestration.Sagas/Orchestration.Sagas.csproj", "src/Orchestration/Orchestration.Sagas/"]
COPY ["src/Orchestration/Orchestration.Infrastructure/Orchestration.Infrastructure.csproj", "src/Orchestration/Orchestration.Infrastructure/"]
COPY ["src/Orchestration/Orchestration.Api/Orchestration.Api.csproj", "src/Orchestration/Orchestration.Api/"]
COPY ["src/Contracts/Common.Contracts/Common.Contracts.csproj", "src/Contracts/Common.Contracts/"]

RUN dotnet restore "src/Orchestration/Orchestration.Api/Orchestration.Api.csproj"

COPY ["src/", "src/"]

WORKDIR "/src/src/Orchestration/Orchestration.Api"
RUN dotnet publish "Orchestration.Api.csproj" \
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

ENTRYPOINT ["dotnet", "Orchestration.Api.dll"]
