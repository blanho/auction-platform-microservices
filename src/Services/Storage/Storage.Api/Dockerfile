FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Services/Storage/Storage.Api/Storage.Api.csproj", "src/Services/Storage/Storage.Api/"]
COPY ["src/Services/Storage/Storage.Application/Storage.Application.csproj", "src/Services/Storage/Storage.Application/"]
COPY ["src/Services/Storage/Storage.Domain/Storage.Domain.csproj", "src/Services/Storage/Storage.Domain/"]
COPY ["src/Services/Storage/Storage.Infrastructure/Storage.Infrastructure.csproj", "src/Services/Storage/Storage.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Web/BuildingBlocks.Web.csproj", "src/BuildingBlocks/BuildingBlocks.Web/"]
COPY ["src/Contracts/StorageService.Contracts/StorageService.Contracts.csproj", "src/Contracts/StorageService.Contracts/"]
COPY ["src/Contracts/Common.Contracts/Common.Contracts.csproj", "src/Contracts/Common.Contracts/"]

RUN dotnet restore "src/Services/Storage/Storage.Api/Storage.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/Storage/Storage.Api"
RUN dotnet build "Storage.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Storage.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Storage.Api.dll"]
