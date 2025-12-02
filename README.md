# Auction Microservices Platform

A modern, scalable auction platform built with microservices architecture using .NET 9, Next.js, and Docker.

## Architecture Overview

This application consists of multiple microservices:

- **Auction Service** - Manages auction listings and bids
- **Search Service** - Provides search functionality with MongoDB
- **Identity Service** - Handles authentication and authorization (Duende IdentityServer)
- **Gateway Service** - API Gateway using YARP reverse proxy
- **Web** - Next.js frontend application

### Supporting Infrastructure

- **PostgreSQL** - Database for Auction and Identity services
- **MongoDB** - Database for Search service
- **Redis** - Caching layer
- **RabbitMQ** - Message broker for inter-service communication

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (for local development)
- [Node.js 18+](https://nodejs.org/) (for local frontend development)

## Getting Started

### Running with Docker Compose (Recommended)

1. **Clone the repository**

   ```bash
   git clone https://github.com/blanho/auction-microservices.git
   cd auction-microservices
   ```

2. **Start all services**

   ```powershell
   docker compose up -d
   ```

3. **Wait for services to be healthy** (takes ~30-60 seconds)

   ```powershell
   docker compose ps
   ```

4. **Access the application**
   - **Web UI**: http://localhost:3000
   - **API Gateway**: http://localhost:6001
   - **Auction API**: http://localhost:6001/auctions
   - **Search API**: http://localhost:6001/search
   - **Identity Server**: http://localhost:5002
   - **RabbitMQ Management**: http://localhost:15672 (guest/guest)

### Service Ports

| Service             | Host Port | Container Port | Description              |
| ------------------- | --------- | -------------- | ------------------------ |
| Web                 | 3000      | 3000           | Next.js frontend         |
| Gateway             | 6001      | 8080           | API Gateway              |
| Auction API         | 5000      | 8080           | Auction service (direct) |
| Search API          | 5001      | 8080           | Search service (direct)  |
| Identity            | 5002      | 5001           | Identity server          |
| PostgreSQL          | 5433      | 5432           | Database                 |
| MongoDB             | 27018     | 27017          | Search database          |
| Redis               | 6379      | 6379           | Cache                    |
| RabbitMQ            | 5672      | 5672           | Message broker           |
| RabbitMQ Management | 15672     | 15672          | Management UI            |

## Docker Commands

### View running containers

```powershell
docker compose ps
```

### View logs

```powershell
# All services
docker compose logs -f

# Specific service
docker compose logs -f gateway
docker compose logs -f auction-api
docker compose logs -f web

# Last 50 lines
docker compose logs --tail 50 gateway
```

### Stop all services

```powershell
docker compose down
```

### Stop and remove volumes (clean slate)

```powershell
docker compose down -v
```

### Rebuild and restart a specific service

```powershell
# Rebuild gateway after config changes
docker compose up -d --build gateway

# Rebuild all services
docker compose up -d --build
```

### Restart a service

```powershell
docker compose restart gateway
```

## Testing the API

### Test Auction Service

```powershell
# Get all auctions
Invoke-WebRequest -Uri "http://localhost:6001/auctions?pageNumber=1&pageSize=12" -UseBasicParsing

# Direct access (bypass gateway)
Invoke-WebRequest -Uri "http://localhost:5000/api/v1/auctions" -UseBasicParsing
```

### Test Search Service

```powershell
# Search auctions
Invoke-WebRequest -Uri "http://localhost:6001/search?searchTerm=car" -UseBasicParsing
```

### Health Checks

```powershell
# Gateway health
Invoke-WebRequest -Uri "http://localhost:6001/health" -UseBasicParsing

# Auction API health
Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing

# Search API health
Invoke-WebRequest -Uri "http://localhost:5001/health" -UseBasicParsing
```

## Local Development (Without Docker)

### Run Auction Service Locally

1. **Start infrastructure services**

   ```powershell
   docker compose up -d postgres redis rabbitmq
   ```

2. **Update connection strings** in `AuctionService/API/appsettings.Development.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5433;Database=auction_db;Username=postgres;Password=postgres123",
       "Redis": "localhost:6379"
     },
     "RabbitMQ": {
       "Host": "localhost",
       "Username": "guest",
       "Password": "guest"
     }
   }
   ```

3. **Run the service**
   ```powershell
   cd AuctionService/API
   dotnet run
   ```

### Run Web Frontend Locally

1. **Install dependencies**

   ```bash
   cd Web
   npm install
   ```

2. **Create `.env.local` file**:

   ```env
   NEXT_PUBLIC_GATEWAY_URL=http://localhost:6001
   NEXT_PUBLIC_IDENTITY_URL=http://localhost:5002
   NEXT_PUBLIC_AUCTION_API_URL=http://localhost:6001/auctions
   NEXT_PUBLIC_SEARCH_API_URL=http://localhost:6001/search
   ```

3. **Run development server**
   ```bash
   npm run dev
   ```

## Troubleshooting

### 502 Bad Gateway Error

If you encounter 502 errors when accessing the gateway:

1. Check if all services are running and healthy:

   ```powershell
   docker compose ps
   ```

2. Check gateway logs:

   ```powershell
   docker compose logs gateway --tail 50
   ```

3. Verify the gateway can reach backend services:
   ```powershell
   docker compose exec gateway curl http://auction-api:8080/health
   ```

### Database Connection Issues

1. Check if PostgreSQL is healthy:

   ```powershell
   docker compose ps postgres
   ```

2. View PostgreSQL logs:

   ```powershell
   docker compose logs postgres
   ```

3. Connect to PostgreSQL:
   ```powershell
   docker compose exec postgres psql -U postgres
   ```

### Container Won't Start

1. View container logs:

   ```powershell
   docker compose logs <service-name>
   ```

2. Rebuild the container:

   ```powershell
   docker compose up -d --build <service-name>
   ```

3. Remove and recreate:
   ```powershell
   docker compose down
   docker compose up -d
   ```

### Clean Everything and Start Fresh

```powershell
# Stop all containers
docker compose down

# Remove all volumes (WARNING: deletes all data)
docker compose down -v

# Remove images
docker compose down --rmi all

# Start fresh
docker compose up -d --build
```

## Project Structure

```
auction-microservices/
├── AuctionService/          # Auction microservice
│   ├── API/                 # API layer
│   ├── Application/         # Business logic
│   ├── Domain/              # Domain models
│   └── Infrastructure/      # Data access
├── SearchService/           # Search microservice
│   ├── API/
│   ├── Application/
│   ├── Domain/
│   └── Infrastructure/
├── IdentityService/         # Identity microservice
├── GatewayService/          # API Gateway
├── Web/                     # Next.js frontend
├── Common/                  # Shared libraries
│   ├── Common.Core/
│   ├── Common.Caching/
│   ├── Common.Logging/
│   ├── Common.Messaging/
│   └── ...
└── docker-compose.yml       # Docker composition
```

## Environment Variables

### Gateway Service

- `ASPNETCORE_ENVIRONMENT` - Environment (Development/Production)
- `Identity__Authority` - Identity server URL

### Auction Service

- `ConnectionStrings__DefaultConnection` - PostgreSQL connection
- `ConnectionStrings__Redis` - Redis connection
- `RabbitMQ__Host` - RabbitMQ host

### Search Service

- `MongoDb__ConnectionString` - MongoDB connection
- `MongoDb__DatabaseName` - Database name
- `MongoDb__CollectionName` - Collection name

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

[Add your license here]
