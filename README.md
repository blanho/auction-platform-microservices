<p align="center">
  <h1 align="center">🏛️ Auction Platform Microservices</h1>
  <p align="center">
    A production-grade, full-stack online auction platform built with .NET 10 microservices and React 19.
    <br />
    Powered by Domain-Driven Design, CQRS, Event-Driven Architecture, and Saga Orchestration.
  </p>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/React-19-61DAFB?style=for-the-badge&logo=react&logoColor=black" />
  <img src="https://img.shields.io/badge/TypeScript-5.9-3178C6?style=for-the-badge&logo=typescript&logoColor=white" />
  <img src="https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" />
  <img src="https://img.shields.io/badge/RabbitMQ-3.13-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white" />
  <img src="https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white" />
  <img src="https://img.shields.io/badge/Kubernetes-Ready-326CE5?style=for-the-badge&logo=kubernetes&logoColor=white" />
</p>

---

## 📑 Table of Contents

- [Architecture Overview](#-architecture-overview)
- [System Architecture Diagram](#-system-architecture-diagram)
- [Microservices](#-microservices)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [Service Ports](#-service-ports)
- [Infrastructure Services](#-infrastructure-services)
- [Frontend Application](#-frontend-application)
- [Architectural Patterns](#-architectural-patterns)
- [Inter-Service Communication](#-inter-service-communication)
- [Saga Orchestration](#-saga-orchestration)
- [Testing](#-testing)
- [Deployment](#-deployment)
- [Observability](#-observability)
- [API Documentation](#-api-documentation)

---

## 🏗 Architecture Overview

The platform follows a **microservices architecture** with **Domain-Driven Design (DDD)** principles. Each service is a self-contained bounded context with its own database (**Database-per-Service** pattern), communicating through asynchronous events via **RabbitMQ** and synchronous queries via **gRPC**.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                            Client Applications                             │
│                     React 19 SPA + SignalR WebSocket                        │
└──────────────────────────────────┬──────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          API Gateway (YARP)                                 │
│               Rate Limiting · JWT Auth · CORS · Routing                    │
│                            Port: 6001                                      │
└──────────────────────────────────┬──────────────────────────────────────────┘
                                   │
           ┌───────────────────────┼───────────────────────┐
           ▼                       ▼                       ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│  Auction Service │  │ Bidding Service  │  │ Identity Service│
│    Port: 5002    │  │   Port: 5003     │  │   Port: 5001    │
│   auction_db     │  │    bid_db        │  │  identity_db    │
└────────┬─────────┘  └────────┬─────────┘  └────────┬────────┘
         │                     │                      │
         ▼                     ▼                      ▼
┌─────────────────┐  ┌─────────────────┐  ┌──────────────────┐
│ Payment Service  │  │  Search Service  │  │Notification Svc  │
│   Port: 5004     │  │   Port: 5008     │  │   Port: 5005     │
│  payment_db      │  │  Elasticsearch   │  │ notification_db  │
└────────┬─────────┘  └────────┬─────────┘  └────────┬─────────┘
         │                     │                      │
         ▼                     ▼                      ▼
┌─────────────────┐  ┌─────────────────┐  ┌──────────────────┐
│Analytics Service │  │ Storage Service  │  │   Job Service    │
│   Port: 5007     │  │   Port: 5009     │  │                  │
│  analytics_db    │  │   storage_db     │  │                  │
└──────────────────┘  └─────────────────┘  └──────────────────┘
```

---

## 🔭 System Architecture Diagram

```
                                    ┌──────────────┐
                                    │  React 19    │
                                    │  SPA Client  │
                                    │  (Vite)      │
                                    └──────┬───────┘
                                           │ HTTPS
                                           ▼
                                  ┌─────────────────┐
                                  │   YARP Gateway   │
                                  │   (Port 6001)    │
                                  │ ┌─────────────┐  │
                                  │ │Rate Limiting │  │
                                  │ │JWT Auth      │  │
                                  │ │CORS          │  │
                                  │ │Path Transform│  │
                                  │ └─────────────┘  │
                                  └────────┬─────────┘
                    ┌──────────┬───────────┼───────────┬──────────┐
                    │          │           │           │          │
              ┌─────▼────┐┌───▼─────┐┌────▼───┐┌─────▼────┐┌────▼─────┐
              │ Auction  ││ Bidding ││Identity││ Payment  ││  Search  │
              │ Service  ││ Service ││Service ││ Service  ││ Service  │
              │          ││         ││        ││ (Stripe) ││          │
              └──┬───┬───┘└──┬──┬───┘└──┬──┬──┘└──┬───┬──┘└──┬──┬───┘
                 │   │       │  │       │  │      │   │      │  │
    ┌────────────┘   │       │  │       │  │      │   │      │  └──────────┐
    │                │       │  │       │  │      │   │      │             │
    ▼                ▼       ▼  ▼       ▼  ▼      ▼   ▼      ▼             ▼
┌────────┐    ┌─────────┐ ┌──────┐ ┌────────┐ ┌──────────┐  ┌──────────────┐
│Postgres│    │RabbitMQ │ │Redis │ │  Seq   │ │Orchestr. │  │Elasticsearch │
│  (7 DBs│    │(Events) │ │Cache │ │(Logs)  │ │  Sagas   │  │  (Search)    │
│  )     │    │         │ │Lock  │ │        │ │(MassTran)│  │              │
└────────┘    └────┬────┘ └──────┘ └────────┘ └──────────┘  └──────────────┘
                   │
          ┌────────┴────────┐
          │  Notification   │  ◄── SignalR WebSocket
          │  Analytics      │
          │  Storage        │
          │  Job            │
          └─────────────────┘
```

---

## 🧩 Microservices

| Service | Responsibility | Database | Key Features |
|---------|---------------|----------|--------------|
| **Gateway** | API Gateway & Reverse Proxy | — | YARP routing, rate limiting, JWT validation, CORS, WebSocket proxy |
| **Auction** | Auction lifecycle management | `auction_db` | Create/update/delete auctions, categories, brands, bookmarks, reviews |
| **Bidding** | Bid management | `bid_db` | Place bids, auto-bidding, bid history |
| **Identity** | Authentication & Authorization | `identity_db` | JWT auth, refresh tokens, Google/Facebook OAuth, roles & permissions |
| **Payment** | Payment processing | `payment_db` | Stripe integration, wallets, orders, checkout |
| **Notification** | Real-time notifications | `notification_db` | SignalR hubs, email/push notifications, templates |
| **Search** | Full-text search | Elasticsearch | Auction search, indexing via event consumers |
| **Analytics** | Platform analytics | `analytics_db` | Dashboards, reports, audit logs, platform settings |
| **Storage** | File management | `storage_db` | Azure Blob Storage, file uploads, image management |
| **Job** | Background job management | — | Scheduled tasks, recurring jobs |

---

## 🛠 Tech Stack

### Backend

| Category | Technology | Version |
|----------|-----------|---------|
| **Runtime** | .NET | 10.0 |
| **API Framework** | ASP.NET Core Minimal APIs + Carter | 8.2.1 |
| **API Gateway** | YARP (Yet Another Reverse Proxy) | 2.2.0 |
| **ORM** | Entity Framework Core | 10.0.2 |
| **Database** | PostgreSQL (Npgsql) | 16 |
| **Caching** | Redis (StackExchange.Redis) | 7 |
| **Message Broker** | RabbitMQ (MassTransit) | 3.13 / 8.4.0 |
| **Search Engine** | Elasticsearch | 8.11.3 |
| **Mediator / CQRS** | MediatR | 12.4.1 |
| **Validation** | FluentValidation | 11.11.0 |
| **Mapping** | AutoMapper | 13.0.1 |
| **gRPC** | Grpc.AspNetCore | 2.70.0 |
| **Auth** | JWT Bearer + ASP.NET Identity | 10.0.2 |
| **OAuth** | Google, Facebook | — |
| **Payments** | Stripe | — |
| **Storage** | Azure Blob Storage | 12.24.0 |
| **Job Scheduling** | Quartz.NET | 3.14.0 |
| **Distributed Locking** | RedLock.net | 2.3.2 |
| **Logging** | Serilog + Seq | 10.0.0 |
| **Tracing** | OpenTelemetry (OTLP) | 1.10.0 |
| **Resilience** | Microsoft.Extensions.Http.Resilience | 10.1.0 |
| **PDF Generation** | QuestPDF | 2024.12.2 |
| **Excel Export** | ClosedXML | 0.104.2 |
| **HTML Sanitizer** | HtmlSanitizer | 9.0.889 |
| **Code Quality** | SonarCloud | — |

### Frontend

| Category | Technology | Version |
|----------|-----------|---------|
| **Framework** | React | 19.2.0 |
| **Language** | TypeScript | ~5.9.3 |
| **Build Tool** | Vite | 7.2.4 |
| **UI Library** | MUI (Material UI) | 7.3.7 |
| **Styling** | Tailwind CSS + Emotion | 4.1.18 |
| **Animations** | Framer Motion | 12.29.0 |
| **State Management** | TanStack React Query | 5.90.20 |
| **Routing** | React Router DOM | 7.13.0 |
| **Forms** | React Hook Form + Zod | 7.71.1 / 4.3.6 |
| **HTTP Client** | Axios | 1.13.2 |
| **Real-time** | SignalR (@microsoft/signalr) | 10.0.0 |
| **Internationalization** | i18next + react-i18next | 25.8.0 |
| **Charts** | Recharts | 3.7.0 |
| **File Upload** | react-dropzone | 14.4.0 |
| **Linting** | ESLint + typescript-eslint | 9.39.2 |
| **Formatting** | Prettier | 3.8.1 |
| **Git Hooks** | Husky + lint-staged | — |

### Infrastructure

| Category | Technology |
|----------|-----------|
| **Containerization** | Docker + Docker Compose |
| **Orchestration** | Kubernetes (Kustomize overlays) |
| **Environments** | Dev / Staging / Production |
| **CI/CD** | GitHub Actions |
| **Code Analysis** | SonarCloud |

---

## 📂 Project Structure

```
auction-platform-microservices/
│
├── src/
│   ├── BuildingBlocks/                    # Shared libraries across all services
│   │   ├── BuildingBlocks.Domain/         #   Base entities, value objects, domain events
│   │   ├── BuildingBlocks.Application/    #   CQRS abstractions, behaviors, validation pipeline
│   │   ├── BuildingBlocks.Infrastructure/ #   EF Core, Redis, MassTransit, Quartz setup
│   │   └── BuildingBlocks.Web/            #   Shared API concerns, middleware
│   │
│   ├── Contracts/                         # Inter-service communication contracts
│   │   ├── AuctionService.Contracts/      #   Auction events + gRPC protos
│   │   ├── BidService.Contracts/          #   Bid events
│   │   ├── IdentityService.Contracts/     #   Identity events + gRPC protos
│   │   ├── PaymentService.Contracts/      #   Payment events
│   │   ├── NotificationService.Contracts/ #   Notification events
│   │   ├── OrchestrationService.Contracts/#   Saga commands & events (24 contracts)
│   │   ├── Common.Contracts/              #   Shared event contracts
│   │   ├── JobService.Contracts/          #   Job events
│   │   ├── SearchService.Contracts/       #   Search events
│   │   └── StorageService.Contracts/      #   Storage events
│   │
│   ├── Gateway/
│   │   └── Gateway.Api/                   # YARP API Gateway
│   │
│   ├── Orchestration/
│   │   └── Orchestration.Sagas/           # MassTransit Saga State Machines
│   │
│   └── Services/                          # Bounded Context Microservices
│       ├── Auction/                        #   (Api / Application / Domain / Infrastructure)
│       ├── Bidding/                        #   (Api / Application / Domain / Infrastructure)
│       ├── Identity/                       #   (Api)
│       ├── Payment/                        #   (Api / Application / Domain / Infrastructure)
│       ├── Notification/                   #   (Api / Application / Domain / Infrastructure)
│       ├── Search/                         #   (Api)
│       ├── Analytics/                      #   (Api)
│       ├── Storage/                        #   (Api / Application / Domain / Infrastructure)
│       └── Job/                            #   (Api / Application / Domain / Infrastructure)
│
├── tests/
│   ├── Auction.Domain.Tests/              # Auction domain unit tests
│   ├── Auction.Application.Tests/         # Auction application layer tests
│   ├── Bidding.Domain.Tests/              # Bidding domain unit tests
│   └── Bidding.Application.Tests/         # Bidding application layer tests
│
├── web/                                   # React 19 SPA Frontend
│   └── src/
│       ├── app/                           #   Providers, context, routing
│       ├── config/                        #   Environment config
│       ├── i18n/                          #   Internationalization (EN + JA)
│       ├── modules/                       #   Feature modules (10 modules)
│       │   ├── auctions/                  #     Auction management
│       │   ├── auth/                      #     Authentication flows
│       │   ├── bidding/                   #     Bid management
│       │   ├── users/                     #     User profiles & settings
│       │   ├── payments/                  #     Wallet, orders, checkout
│       │   ├── notifications/             #     Real-time notifications
│       │   ├── analytics/                 #     Dashboards & reports
│       │   ├── search/                    #     Full-text search
│       │   ├── home/                      #     Landing pages
│       │   └── jobs/                      #     Job management
│       ├── services/                      #   Axios HTTP client, SignalR hub
│       └── shared/                        #   Reusable components, hooks, theme
│
├── deploy/
│   ├── docker/
│   │   ├── docker-compose.yml             # Full dev environment
│   │   └── Dockerfile.base                # Multi-stage build template
│   └── kubernetes/
│       ├── base/                          # K8s base manifests
│       └── overlays/                      # Dev / Staging / Production
│           ├── dev/
│           ├── staging/
│           └── production/
│
├── docs/                                  # Documentation & runbooks
├── auction.sln                            # .NET Solution file
└── sonar-project.properties               # SonarCloud configuration
```

---

## 🚀 Getting Started

### Prerequisites

| Tool | Version | Required |
|------|---------|----------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ | ✅ |
| [Node.js](https://nodejs.org/) | 20+ | ✅ |
| [Docker Desktop](https://www.docker.com/products/docker-desktop) | Latest | ✅ |
| [Docker Compose](https://docs.docker.com/compose/) | v2+ | ✅ |

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/auction-platform-microservices.git
cd auction-platform-microservices
```

### 2. Start Infrastructure Services

```bash
docker-compose -f deploy/docker/docker-compose.yml up -d postgres redis rabbitmq elasticsearch seq
```

Wait for all services to be healthy:

```bash
docker-compose -f deploy/docker/docker-compose.yml ps
```

### 3. Run Backend Services

**Option A: Run all services with Docker Compose**

```bash
docker-compose -f deploy/docker/docker-compose.yml up -d
```

**Option B: Run individual services locally**

```bash
# Restore and build the entire solution
dotnet restore auction.sln
dotnet build auction.sln

# Run a specific service
dotnet run --project src/Services/Auction/Auction.Api/Auction.Api.csproj
dotnet run --project src/Services/Bidding/Bidding.Api/Bidding.Api.csproj
dotnet run --project src/Services/Identity/Identity.Api/Identity.Api.csproj
dotnet run --project src/Gateway/Gateway.Api/Gateway.Api.csproj
```

### 4. Run Frontend Application

```bash
cd web
npm install
npm run dev
```

The frontend will be available at `http://localhost:5173` and proxies API requests to the Gateway at `http://localhost:6001`.

### 5. Verify Setup

| Service | URL | Description |
|---------|-----|-------------|
| Frontend | http://localhost:5173 | React SPA |
| Gateway | http://localhost:6001 | API Gateway |
| RabbitMQ Management | http://localhost:15672 | Username: `guest` / Password: `guest` |
| Seq (Logs) | http://localhost:5341 | Centralized logging dashboard |
| Elasticsearch | http://localhost:9200 | Search engine |

---

## 🔌 Service Ports

| Service | Host Port | Container Port |
|---------|-----------|----------------|
| **Gateway** | 6001 | 8080 |
| **Identity API** | 5001 | 8080 |
| **Auction API** | 5002 | 8080 |
| **Bidding API** | 5003 | 8080 |
| **Payment API** | 5004 | 8080 |
| **Notification API** | 5005 | 8080 |
| **Analytics API** | 5007 | 8080 |
| **Search API** | 5008 | 8080 |
| **Storage API** | 5009 | 8080 |

---

## 🏢 Infrastructure Services

| Service | Image | Port(s) | Purpose |
|---------|-------|---------|---------|
| **PostgreSQL** | `postgres:16-alpine` | 5432 | Primary database (7 databases) |
| **Redis** | `redis:7-alpine` | 6379 | Caching + distributed locking |
| **RabbitMQ** | `rabbitmq:3.13-management-alpine` | 5672 / 15672 | Message broker + management UI |
| **Elasticsearch** | `elasticsearch:8.11.3` | 9200 / 9300 | Full-text search engine |
| **Seq** | `datalust/seq:latest` | 5341 | Centralized log aggregation |

---

## 🖥 Frontend Application

### Feature Modules

| Module | Pages |
|--------|-------|
| **Auctions** | Listing, Detail, Create/Edit, Watchlist, Categories, Brands |
| **Auth** | Login, Register, Forgot/Reset Password, Email Confirmation, OAuth |
| **Bidding** | My Bids, Winning Bids, Bid History, Auto-Bid Management |
| **Users** | Profile, Settings, My Auctions, Seller Application, Reviews |
| **Payments** | Wallet, Orders, Checkout, Payment Methods, Transactions |
| **Notifications** | Notification Center, Templates, Broadcasting, Stats |
| **Analytics** | User Dashboard, Admin Dashboard, Reports, Audit Logs |
| **Search** | Full-text Search with Filters |
| **Home** | Landing Page, How It Works |
| **Jobs** | Job Listing, Job Details |

### Frontend Scripts

```bash
npm run dev          # Start development server
npm run build        # Production build (TypeScript check + Vite build)
npm run lint         # ESLint check
npm run lint:fix     # ESLint auto-fix
npm run format       # Prettier format
npm run typecheck    # TypeScript type checking
npm run validate     # Full validation (format + lint + typecheck)
```

---

## 🧱 Architectural Patterns

```
┌─────────────────────────────────────────────────────────────────┐
│                    Per-Service Architecture                      │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  API Layer (Carter Endpoints)                            │   │
│  │  HTTP Controllers · gRPC Services · Health Checks        │   │
│  └────────────────────────┬─────────────────────────────────┘   │
│                           │                                     │
│  ┌────────────────────────▼─────────────────────────────────┐   │
│  │  Application Layer (MediatR Handlers)                    │   │
│  │  Commands · Queries · DTOs · Validation (FluentValidation│   │
│  │  Behaviors: Logging · Validation · Transaction Pipeline  │   │
│  └────────────────────────┬─────────────────────────────────┘   │
│                           │                                     │
│  ┌────────────────────────▼─────────────────────────────────┐   │
│  │  Domain Layer (Pure Business Logic)                      │   │
│  │  Entities · Value Objects · Domain Events · Aggregates   │   │
│  │  No external dependencies                                │   │
│  └────────────────────────┬─────────────────────────────────┘   │
│                           │                                     │
│  ┌────────────────────────▼─────────────────────────────────┐   │
│  │  Infrastructure Layer                                    │   │
│  │  EF Core Repositories · MassTransit Consumers · Redis    │   │
│  │  Outbox Pattern · External Service Integrations          │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

| Pattern | Implementation |
|---------|---------------|
| **Clean Architecture** | Domain → Application → Infrastructure → API layers |
| **Domain-Driven Design** | Bounded contexts, aggregates, domain events, value objects |
| **CQRS** | MediatR commands/queries with separate handlers |
| **Event-Driven Architecture** | RabbitMQ pub/sub via MassTransit for loose coupling |
| **Saga Orchestration** | MassTransit state machines for cross-service workflows |
| **Outbox Pattern** | MassTransit.EntityFrameworkCore for reliable messaging |
| **Database-per-Service** | Each service owns its PostgreSQL database |
| **API Gateway** | YARP reverse proxy with rate limiting, auth, path transforms |
| **Repository + Unit of Work** | EF Core abstracted behind BuildingBlocks interfaces |
| **Distributed Locking** | RedLock.net for concurrency control |
| **Validation Pipeline** | FluentValidation via MediatR behavior pipeline |
| **Minimal APIs** | Carter for clean endpoint routing |
| **Background Jobs** | Quartz.NET for scheduled tasks |

---

## 🔗 Inter-Service Communication

```
┌─────────────────────────────────────────────────────────────────┐
│                  Communication Patterns                         │
│                                                                 │
│  ┌──────────────────────┐     ┌──────────────────────┐         │
│  │   Synchronous (gRPC) │     │ Asynchronous (Events)│         │
│  │                      │     │                      │         │
│  │  • Auction Validation│     │  • AuctionCreated    │         │
│  │  • User Statistics   │     │  • BidPlaced         │         │
│  │  • Bid Data Queries  │     │  • PaymentProcessed  │         │
│  │  • Auction Stats     │     │  • OrderCreated      │         │
│  │                      │     │  • Saga Commands     │         │
│  └──────────────────────┘     └──────────────────────┘         │
│           │                            │                       │
│           ▼                            ▼                       │
│    ┌─────────────┐            ┌─────────────────┐              │
│    │   gRPC +    │            │   MassTransit + │              │
│    │   Protobuf  │            │   RabbitMQ      │              │
│    └─────────────┘            └─────────────────┘              │
└─────────────────────────────────────────────────────────────────┘
```

**10 Contracts projects** define the shared communication interface between services, containing event definitions and gRPC `.proto` files.

---

## 🔄 Saga Orchestration

The platform uses **MassTransit State Machines** for complex cross-service workflows:

### BuyNow Saga

```
StartBuyNow ──► ReserveAuction ──► CreateOrder ──► CompleteAuction
                     │                   │
                     ▼                   ▼
              ReleaseReservation    (Compensation)
              (on failure)
```

### Auction Completion Saga

```
AuctionEnded ──► CreateWinnerOrder ──► SendNotifications ──► Complete
                      │
                      ▼
               RevertAuction
               (on failure)
```

---

## 🧪 Testing

```bash
# Run all tests
dotnet test auction.sln

# Run specific test projects
dotnet test tests/Auction.Domain.Tests/Auction.Domain.Tests.csproj
dotnet test tests/Auction.Application.Tests/Auction.Application.Tests.csproj
dotnet test tests/Bidding.Domain.Tests/Bidding.Domain.Tests.csproj
dotnet test tests/Bidding.Application.Tests/Bidding.Application.Tests.csproj

# Run with coverage
dotnet test auction.sln --collect:"XPlat Code Coverage"
```

```
Test Pyramid
        ╱╲
       ╱  ╲        E2E Tests
      ╱────╲
     ╱      ╲      Integration Tests
    ╱────────╲
   ╱          ╲    Unit Tests (Domain + Application)
  ╱────────────╲
```

---

## 🚢 Deployment

### Docker Compose (Development)

```bash
# Start everything
docker-compose -f deploy/docker/docker-compose.yml up -d

# View logs
docker-compose -f deploy/docker/docker-compose.yml logs -f

# Stop everything
docker-compose -f deploy/docker/docker-compose.yml down

# Stop and remove volumes
docker-compose -f deploy/docker/docker-compose.yml down -v
```

### Kubernetes (Staging / Production)

The project uses **Kustomize** with environment overlays:

```bash
# Dev environment
kubectl apply -k deploy/kubernetes/overlays/dev

# Staging environment
kubectl apply -k deploy/kubernetes/overlays/staging

# Production environment
kubectl apply -k deploy/kubernetes/overlays/production
```

**Kubernetes features:**
- Pod Security Standards (restricted)
- Resource quotas (50 CPU / 100Gi memory)
- Pod Disruption Budgets
- Prometheus ServiceMonitors
- Priority classes
- RBAC configuration
- Ingress with TLS

---

## 📊 Observability

| Tool | Purpose | Access |
|------|---------|--------|
| **Seq** | Centralized log aggregation & search | http://localhost:5341 |
| **Serilog** | Structured logging with correlation IDs | — |
| **OpenTelemetry** | Distributed tracing (OTLP export) | — |
| **Prometheus** | Metrics collection (K8s) | — |
| **SonarCloud** | Static code analysis & coverage | — |

### Health Checks

Every service exposes health check endpoints:

```
GET /health        # Overall health
GET /health/ready  # Readiness probe
GET /health/live   # Liveness probe
```

---

## 📖 API Documentation

Each service exposes OpenAPI documentation:

```
GET /swagger       # Swagger UI (Development only)
```

All API traffic is routed through the Gateway at `http://localhost:6001/api/v1/{service}/...`

### API Versioning

The platform supports API versioning. See [docs/API_VERSIONING.md](docs/API_VERSIONING.md) for details.

---

## ⚙️ Environment Variables

### Backend Services

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | See docker-compose |
| `ConnectionStrings__Redis` | Redis connection string | `redis:6379` |
| `RabbitMQ__Host` | RabbitMQ hostname | `rabbitmq` |
| `RabbitMQ__Username` | RabbitMQ username | `guest` |
| `RabbitMQ__Password` | RabbitMQ password | `guest` |
| `Elasticsearch__Uri` | Elasticsearch URI | `http://elasticsearch:9200` |
| `STRIPE_SECRET_KEY` | Stripe API secret key | — |
| `STRIPE_WEBHOOK_SECRET` | Stripe webhook secret | — |

### Frontend

| Variable | Description | Default |
|----------|-------------|---------|
| `VITE_API_BASE_URL` | API Gateway base URL | `/api` → `localhost:6001` |

---

## 📄 License

This project is proprietary. All rights reserved.
