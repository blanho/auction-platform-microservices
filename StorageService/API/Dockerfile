FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["StorageService/API/StorageService.API.csproj", "StorageService/API/"]
COPY ["StorageService/Application/StorageService.Application.csproj", "StorageService/Application/"]
COPY ["StorageService/Domain/StorageService.Domain.csproj", "StorageService/Domain/"]
COPY ["StorageService/Infrastructure/StorageService.Infrastructure.csproj", "StorageService/Infrastructure/"]
COPY ["Common/Common.Core/Common.Core.csproj", "Common/Common.Core/"]
COPY ["Common/Common.Domain/Common.Domain.csproj", "Common/Common.Domain/"]
COPY ["Common/Common.Logging/Common.Logging.csproj", "Common/Common.Logging/"]
COPY ["Common/Common.OpenApi/Common.OpenApi.csproj", "Common/Common.OpenApi/"]
COPY ["Common/Common.Repository/Common.Repository.csproj", "Common/Common.Repository/"]
RUN dotnet restore "StorageService/API/StorageService.API.csproj"
COPY . .
WORKDIR "/src/StorageService/API"
RUN dotnet build "StorageService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StorageService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StorageService.API.dll"]
