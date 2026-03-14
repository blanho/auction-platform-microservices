# Getting Started Guide

This guide walks you through everything you need to go from a fresh clone to a running development environment. Follow it step by step.

---

## Table of Contents

- [Prerequisites](#prerequisites)
- [Clone and Setup](#clone-and-setup)
- [Understanding the Repository](#understanding-the-repository)
- [Running with Docker Compose](#running-with-docker-compose)
- [Running Individual Services](#running-individual-services)
- [Running the Frontend](#running-the-frontend)
- [Verifying Everything Works](#verifying-everything-works)
- [Development Workflow](#development-workflow)
- [Working on a Specific Service](#working-on-a-specific-service)
- [Running Tests](#running-tests)
- [Debugging](#debugging)
- [Common Issues](#common-issues)

---

## Prerequisites

Install the following tools before proceeding:

| Tool | Version | Installation |
|---|---|---|
| .NET SDK | 9.0+ | [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) |
| Node.js | 20 LTS | [nodejs.org](https://nodejs.org) |
| Docker Desktop | 24+ | [docker.com](https://www.docker.com/products/docker-desktop) |
| Docker Compose | v2 | Included with Docker Desktop |
| Git | Latest | [git-scm.com](https://git-scm.com) |

**Recommended IDE:** Visual Studio Code with the C# Dev Kit extension, or JetBrains Rider.

**VS Code extensions:**
- C# Dev Kit (Microsoft)
- ESLint
- Prettier
- Docker
- REST Client (for testing APIs)

---

## Clone and Setup

```bash
# Clone the repository
git clone https://github.com/blanho/auction-platform-microservices.git
cd auction-platform-microservices

# Verify .NET SDK
dotnet --version    # Should show 9.x.x

# Verify Node.js
node --version      # Should show 20.x.x or higher

# Verify Docker
docker --version
docker compose version
```

### Configure Environment Variables

```bash
cp .env.example .env
```

Open `.env` and fill in the required values. For local development, most defaults work out of the box. You will need to provide:

| Variable | Where to Get It |
|---|---|
| `STRIPE_SECRET_KEY` | [Stripe Dashboard](https://dashboard.stripe.com/test/apikeys) (use test key) |
| `STRIPE_WEBHOOK_SECRET` | Run `stripe listen --forward-to localhost:5004/webhooks/stripe` |
| `SENDGRID_API_KEY` | [SendGrid](https://app.sendgrid.com/settings/api_keys) (free tier works) |
| `GOOGLE_CLIENT_ID/SECRET` | [Google Cloud Console](https://console.cloud.google.com/apis/credentials) |
| `FACEBOOK_APP_ID/SECRET` | [Facebook Developers](https://developers.facebook.com/apps/) |

For **local development without external services**, you can leave Stripe/SendGrid/OAuth values as placeholder strings. The services will start but those specific features won't work.

---

## Understanding the Repository

Before writing any code, understand how the project is organized:

```
auction-platform-microservices/
├── src/
│   ├── BuildingBlocks/        # Shared libraries used by all services
│   ├── Contracts/             # Shared message contracts
│   ├── Services/              # The 9 microservices (each with 4-5 layers)
│   ├── Gateway/               # YARP API gateway
│   └── Orchestration/         # Saga state machines
├── web/                       # React frontend
├── deploy/                    # Docker and Kubernetes configs
└── docs/                      # You are here
```

### Service Layer Structure

Every service under `src/Services/{Name}/` follows the same pattern:

```
{Name}.Contracts/       # Message contracts (events, commands)
{Name}.Domain/          # Entities, value objects, domain events
{Name}.Application/     # CQRS handlers, DTOs, validators
{Name}.Infrastructure/  # EF Core, external service clients, MassTransit
{Name}.Api/             # Endpoints, DI setup, middleware
```

**Key concept:** Dependencies flow inward. Domain has zero external dependencies. Application depends only on Domain. Infrastructure implements interfaces defined in Application. API is the composition root that wires everything together.

### BuildingBlocks

Shared libraries that every service references:

| Library | What It Provides |
|---|---|
| `BuildingBlocks.Domain` | Base entity classes, domain event interfaces, value object base, audit fields |
| `BuildingBlocks.Application` | CQRS interfaces (`ICommand`, `IQuery`), pipeline behaviors, paging, filtering |
| `BuildingBlocks.Infrastructure` | EF Core extensions, Redis caching, MassTransit config, Polly resilience, audit publisher |
| `BuildingBlocks.Web` | Auth middleware, rate limiting, error handling middleware, health checks, security headers |

---

## Running with Docker Compose

The fastest way to get the entire platform running:

```bash
# Start everything (infrastructure + all services + frontend)
docker compose -f deploy/docker/docker-compose.yml up -d

# Watch the logs
docker compose -f deploy/docker/docker-compose.yml logs -f

# Check health status
docker compose -f deploy/docker/docker-compose.yml ps
```

Wait for all containers to show `healthy` status. The infrastructure services (PostgreSQL, Redis, RabbitMQ, Elasticsearch) start first, and application services wait for them via health check dependencies.

### What Gets Started

| Container | Port | Description |
|---|---|---|
| `auction-postgres` | 5432 | PostgreSQL with all databases |
| `auction-redis` | 6379 | Redis cache |
| `auction-rabbitmq` | 5672, 15672 | RabbitMQ (AMQP + Management UI) |
| `auction-elasticsearch` | 9200 | Elasticsearch |
| `auction-seq` | 5341 | Seq log viewer |
| `identity-api` | 5001 | Identity Service |
| `auction-api` | 5002 | Auction Service |
| `bidding-api` | 5003 | Bidding Service |
| `payment-api` | 5004 | Payment Service |
| `notification-api` | 5005 | Notification Service |
| `job-api` | 5006 | Job Service |
| `analytics-api` | 5007 | Analytics Service |
| `search-api` | 5008 | Search Service |
| `storage-api` | 5009 | Storage Service |
| `auction-gateway` | 6001 | YARP API Gateway |
| `auction-web-frontend` | 3000 | React SPA (Nginx) |

### Stopping Everything

```bash
docker compose -f deploy/docker/docker-compose.yml down

# To also remove volumes (reset all data):
docker compose -f deploy/docker/docker-compose.yml down -v
```

---

## Running Individual Services

For day-to-day development, you typically run infrastructure in Docker and the service you are working on locally:

### Step 1: Start Infrastructure Only

```bash
# Start only PostgreSQL, Redis, RabbitMQ, Elasticsearch, Seq
docker compose -f deploy/docker/docker-compose.yml up -d postgres redis rabbitmq elasticsearch seq
```

### Step 2: Run Your Service Locally

```bash
cd src/Services/Auction/Auction.Api
dotnet run
```

The service will start on its configured port and connect to the Docker infrastructure.

### Step 3: (Optional) Run the Gateway

If you need to test through the gateway:

```bash
cd src/Gateway/Gateway.Api
dotnet run
```

---

## Running the Frontend

```bash
cd web
npm install
npm run dev
```

The dev server starts at http://localhost:5173 with hot module replacement.

**Frontend environment:** The frontend is pre-configured to proxy API requests to the gateway at `http://localhost:6001`. If you change the gateway port, update `vite.config.ts`.

---

## Verifying Everything Works

### Check Service Health

Every service exposes health endpoints:

```bash
# Gateway health
curl http://localhost:6001/health

# Individual service health
curl http://localhost:5002/health    # Auction
curl http://localhost:5003/health    # Bidding
```

### Check RabbitMQ

Open http://localhost:15672 (guest/guest). You should see exchanges and queues created by MassTransit.

### Check Logs

Open http://localhost:5341 (Seq). All services send structured logs here. Use this for debugging.

### Quick API Test

```bash
# Register a user
curl -X POST http://localhost:6001/identity/register \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com", "password": "Test123!", "firstName": "Test", "lastName": "User"}'

# Login
curl -X POST http://localhost:6001/identity/login \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com", "password": "Test123!"}'
# Save the token from the response

# Get auctions (no auth needed)
curl http://localhost:6001/auctions
```

---

## Development Workflow

### Daily Workflow

1. Pull latest changes: `git pull origin main`
2. Start infrastructure: `docker compose -f deploy/docker/docker-compose.yml up -d postgres redis rabbitmq elasticsearch seq`
3. Run the service you are working on: `cd src/Services/{Name}/{Name}.Api && dotnet run`
4. Run the frontend if needed: `cd web && npm run dev`
5. Make changes — the .NET hot reload will pick up most changes
6. Run tests before committing: `dotnet test`
7. Commit with a clear message following conventional commits

### Branch Strategy

| Branch | Purpose |
|---|---|
| `main` | Production-ready code (protected) |
| `develop` | Integration branch |
| `feature/*` | New features |
| `staging` | Pre-production testing |
| `production` | Release branch |

### Commit Convention

Use conventional commits:

```
feat: add auto-bid functionality to bidding service
fix: prevent race condition in concurrent bid placement
refactor: extract bid validation into domain service
docs: update API reference for payment endpoints
```

---

## Working on a Specific Service

### Adding a New Feature (Example: Add a new query to Auction Service)

1. **Define the DTO** in `Auction.Application/DTOs/`
2. **Create the Query** in `Auction.Application/Queries/{FeatureName}/`
   - `{Name}Query.cs` — implements `IQuery<TResult>`
   - `{Name}QueryHandler.cs` — implements `IQueryHandler<TQuery, TResult>`
3. **Add the endpoint** in `Auction.Api/Endpoints/`
   - Carter module that maps the route and dispatches the query via MediatR
4. **Add tests** in `tests/Auction.Application.Tests/`

### Adding a New Command (Example: Add a new mutation)

1. **Define domain logic** in `{Service}.Domain/` (entity methods, domain events)
2. **Create the Command** in `{Service}.Application/Commands/{FeatureName}/`
   - Command class, Handler class, Validator class
3. **Add infrastructure** if needed (repository methods, external service calls)
4. **Add the endpoint** in `{Service}.Api/Endpoints/`
5. **Write tests** for the handler

### Publishing a Domain Event

1. Raise the event in the domain entity: `AddDomainEvent(new AuctionCreatedEvent(Id, Title))`
2. Define the contract in `{Service}.Contracts/Events/`
3. Add consumers in other services that need to react to this event
4. Register consumers in the service's MassTransit configuration

---

## Running Tests

```bash
# All tests
dotnet test

# Specific project
dotnet test src/Services/Auction/tests/Auction.Domain.Tests

# With verbose output
dotnet test --logger "console;verbosity=normal"

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Frontend tests
cd web && npm run validate
```

### Test Structure

Tests are co-located with each service under a `tests/` directory:

```
Services/Auction/
├── Auction.Domain/
├── Auction.Application/
├── Auction.Infrastructure/
├── Auction.Api/
└── tests/
    ├── Auction.Domain.Tests/         # Pure domain logic tests
    └── Auction.Application.Tests/    # Handler tests with mocked dependencies
```

**Testing conventions:**
- Domain tests are pure unit tests (no mocks, no infrastructure)
- Application tests use NSubstitute for mocking repositories and external services
- FluentAssertions for readable assertions
- One test class per handler/entity

---

## Debugging

### Backend (VS Code)

1. Open the service folder in VS Code
2. Set breakpoints in handler or endpoint code
3. Press F5 (uses `.vscode/launch.json` if configured)
4. Or use `dotnet run` and attach the debugger

### Backend (Rider/Visual Studio)

1. Open `auction.sln`
2. Set the startup project to the API you want to debug
3. Press F5

### Frontend

1. Open `web/` in VS Code
2. Run `npm run dev`
3. Use browser DevTools for debugging
4. React DevTools extension is recommended

### Logs

Always check Seq at http://localhost:5341 for structured logs with correlation IDs. You can filter by:
- Service name
- Correlation ID (trace a request across services)
- Log level
- Time range

---

## Common Issues

### Port Already in Use

```bash
# Find what is using the port
lsof -i :5002

# Kill it
kill -9 <PID>
```

### Docker Containers Not Starting

```bash
# Check logs for a specific container
docker logs auction-postgres

# Restart everything
docker compose -f deploy/docker/docker-compose.yml down
docker compose -f deploy/docker/docker-compose.yml up -d
```

### Database Migration Errors

```bash
# Apply migrations for a specific service
cd src/Services/Auction/Auction.Infrastructure
dotnet ef database update --startup-project ../Auction.Api
```

### RabbitMQ Connection Refused

Ensure RabbitMQ is fully started (check `docker compose ps`). MassTransit retries connections automatically, so if RabbitMQ started late, services will reconnect.

### NuGet Restore Failures

```bash
dotnet restore auction.sln
```

If you get version conflicts, check `Directory.Packages.props` for centralized version management.

### Frontend Proxy Issues

If API calls from the frontend fail, ensure:
1. The gateway is running on port 6001
2. CORS is configured to allow `http://localhost:5173`
3. Check `vite.config.ts` proxy configuration
