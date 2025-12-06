FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and project files for restore
COPY ["Auction.sln", "./"]
COPY ["UtilityService/UtilityService.csproj", "UtilityService/"]
COPY ["Common/Common.Core/Common.Core.csproj", "Common/Common.Core/"]
COPY ["Common/Common.Messaging/Common.Messaging.csproj", "Common/Common.Messaging/"]
COPY ["Common/Common.Audit/Common.Audit.csproj", "Common/Common.Audit/"]

# Restore packages
RUN dotnet restore "UtilityService/UtilityService.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/UtilityService"
RUN dotnet build "UtilityService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UtilityService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UtilityService.dll"]
