FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Auction.sln", "./"]
COPY ["PaymentService/API/PaymentService.API.csproj", "PaymentService/API/"]
COPY ["PaymentService/Application/PaymentService.Application.csproj", "PaymentService/Application/"]
COPY ["PaymentService/Domain/PaymentService.Domain.csproj", "PaymentService/Domain/"]
COPY ["PaymentService/Infrastructure/PaymentService.Infrastructure.csproj", "PaymentService/Infrastructure/"]
COPY ["Common/Common.Core/Common.Core.csproj", "Common/Common.Core/"]
COPY ["Common/Common.Domain/Common.Domain.csproj", "Common/Common.Domain/"]
COPY ["Common/Common.Caching/Common.Caching.csproj", "Common/Common.Caching/"]
COPY ["Common/Common.Logging/Common.Logging.csproj", "Common/Common.Logging/"]
COPY ["Common/Common.Messaging/Common.Messaging.csproj", "Common/Common.Messaging/"]
COPY ["Common/Common.Repository/Common.Repository.csproj", "Common/Common.Repository/"]
COPY ["Common/Common.OpenApi/Common.OpenApi.csproj", "Common/Common.OpenApi/"]
COPY ["Common/Common.Security/Common.Security.csproj", "Common/Common.Security/"]
COPY ["Common/Common.CQRS/Common.CQRS.csproj", "Common/Common.CQRS/"]
COPY ["Common/Common.Observability/Common.Observability.csproj", "Common/Common.Observability/"]
COPY ["Common/Common.Audit/Common.Audit.csproj", "Common/Common.Audit/"]
COPY ["Common/Common.Utilities/Common.Utilities.csproj", "Common/Common.Utilities/"]

RUN dotnet restore "PaymentService/API/PaymentService.API.csproj"

COPY . .

WORKDIR "/src/PaymentService/API"
RUN dotnet publish "PaymentService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "PaymentService.API.dll"]
