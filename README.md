# Auction Platform — Microservices

A portfolio-focused, event-driven auction platform built with **.NET 9 microservices** and a **React 19 frontend**.
The system supports real-time bidding, authoritative auction closing, buy-now flows, integrated payments, multi-channel notifications, full-text search, file storage, analytics, and scheduled jobs — all behind a unified API gateway.

[![CI](https://github.com/blanho/auction-platform-microservices/actions/workflows/ci.yml/badge.svg)](https://github.com/blanho/auction-platform-microservices/actions/workflows/ci.yml)
[![CD](https://github.com/blanho/auction-platform-microservices/actions/workflows/cd.yml/badge.svg)](https://github.com/blanho/auction-platform-microservices/actions/workflows/cd.yml)
[![Sonar Analysis](https://github.com/blanho/auction-platform-microservices/actions/workflows/sonar.yml/badge.svg)](https://github.com/blanho/auction-platform-microservices/actions/workflows/sonar.yml)

## Contents

- [Architecture Overview](#architecture-overview)
- [System Architecture Diagram](#system-architecture-diagram)
- [Service Map](#service-map)
- [Tech Stack](#tech-stack)
- [Key Workflows](#key-workflows)
- [Getting Started](#getting-started)
- [Cloud and Deployment](#cloud-and-deployment)
- [Testing](#testing)
- [Documentation](#documentation)

## Architecture Overview

The platform follows a **domain-driven microservices** architecture with strict bounded contexts. Each service owns its data, communicates asynchronously through RabbitMQ (via MassTransit), and exposes REST endpoints through a centralized YARP API gateway. Immediate authoritative queries use internal gRPC. The React frontend connects via REST and receives real-time updates through SignalR WebSockets.

**Core architectural patterns:**

| Pattern | Implementation |
|---|---|
| Domain-Driven Design | Bounded contexts per service, aggregates, value objects, domain events |
| CQRS | Separate command/query models via MediatR |
| Domain Events | Aggregate events published through transactional outboxes; current state remains in service databases |
| Event-driven workflows | Durable MassTransit sagas for Buy Now and auction completion; idempotent participants and transactional outboxes |
| Transactional Outbox | Guaranteed message delivery via EF Core outbox |
| API Gateway | YARP reverse proxy with JWT validation and rate limiting |

---

## System Architecture Diagram

The overview groups the services and shows the main request, data, and messaging paths.
Click the diagram to open the full-size SVG.

[![Auction platform overview: React web app to YARP gateway and ten domain services, with service-owned data, RabbitMQ messaging, and durable orchestration.](docs/diagrams/auction-system-overview.svg)](docs/diagrams/auction-system-overview.svg)

For service responsibilities, see the [Service Map](#service-map). For detailed
interactions, external integrations, and observability, see the
[Architecture Guide](docs/architecture.md). The [cloud deployment diagrams](docs/diagrams/README.md)
show Azure and Kubernetes details, with an
[editable draw.io file](docs/diagrams/auction-azure-architecture.drawio).

## Service Map

| Service | Port | gRPC | Database | Responsibilities |
|---|---|---|---|---|
| **Identity** | 5001 | — | `identity_db` | JWT issuance, OAuth (Google / Facebook), user management |
| **Auction** | 5002 | 5011 | `auction_db` | Auction lifecycle, bookmarks, reviews, media, authoritative close scheduling |
| **Bidding** | 5003 | 5012 | `bid_db` | Bid placement, auto-bids, bid history, authoritative winner selection |
| **Payment** | 5004 | — | `payment_db` | Stripe integration, wallets, order processing, refunds |
| **Notification** | 5005 | — | `notification_db` | Email (SendGrid), SMS (Twilio), Push (Firebase), SignalR hub |
| **Job** | 5006 | — | `job_db` | Tracked bulk operations, item dispatch, progress, and retries |
| **Analytics** | 5007 | — | `analytics_db` | Event ingestion, reporting, dashboards |
| **Catalog** | 5013 | 8081 internal | `catalog_db` | Categories, brands, catalog queries |
| **Search** | 5008 | — | Elasticsearch | Full-text auction search, filtering, facets |
| **Storage** | 5009 | — | `storage_db` | File upload, validation, Azure Blob / local storage |
| **Gateway** | 6001 | — | — | YARP reverse proxy, JWT validation, rate limiting |
| **Orchestration Service** | 5012 | — | PostgreSQL | Durable sale workflows, deadlines, and Admin recovery API. [Workflow and recovery notes](src/Orchestration/README.md) |

All 14 aggregate repositories across Auction, Bidding, Catalog, Job, Payment,
Notification, and Storage reuse the shared EF Core repository. Service-specific
queries, transactions, and deletion policies remain explicit. See the
[reuse guide and service audit](docs/common-repositories.md).

---

## Tech Stack

### Backend

| Category | Technology |
|---|---|
| Runtime | .NET 9, C# |
| Architecture | Clean Architecture (Domain / Application / Infrastructure / API) |
| API | ASP.NET Core Minimal API + Carter |
| CQRS / Mediator | MediatR |
| ORM | Entity Framework Core 9 |
| Messaging | MassTransit + RabbitMQ |
| Transactional Outbox | MassTransit Outbox (EF Core) |
| Workflow coordination | MassTransit state machines with PostgreSQL persistence and durable deadlines |
| gRPC | Grpc.AspNetCore / Grpc.Net.Client |
| API Gateway | YARP (Yet Another Reverse Proxy) |
| Auth | Custom JWT (HS256 / RS256), OAuth2 (Google, Facebook) |
| Caching | Redis (StackExchange.Redis) |
| Concurrency control | Redis distributed locks + PostgreSQL advisory locks for bid/finalization serialization |
| Search | Elasticsearch 8 (Elastic.Clients.Elasticsearch) |
| Logging | Serilog → Seq / Elasticsearch |
| Tracing | OpenTelemetry (OTLP-ready) |
| Validation | FluentValidation |
| Resilience | Polly (circuit breaker, retry, timeout, bulkhead) |
| Scheduling | Quartz / Hangfire (service-local background jobs) |
| Payment | Stripe .NET SDK |
| Email / SMS / Push | SendGrid, Twilio, Firebase Admin SDK |
| File Storage | Azure Blob Storage / Local |
| Testing | xUnit, NSubstitute, FluentAssertions |

### Frontend

| Category | Technology |
|---|---|
| Framework | React 19 |
| Language | TypeScript 5.9 |
| Build Tool | Vite 7 |
| UI Library | MUI (Material UI) v7 |
| Animations | Framer Motion 12 |
| Styling | Tailwind CSS 4 |
| State / Data Fetching | TanStack Query v5 |
| Routing | React Router v7 |
| Forms | React Hook Form + Zod |
| Real-time | @microsoft/signalr v10 |
| Charts | Recharts 3 |
| i18n | i18next + react-i18next |
| HTTP Client | Axios |
| Linting / Formatting | ESLint 9, Prettier 3, Husky + lint-staged |

## Key Workflows

- **Place a bid:** Bidding validates auction eligibility and persists the bid with an outbox message. Bid placement and winner selection share a per-auction PostgreSQL advisory lock.
- **Close an auction:** Auction's scheduled close asks Bidding for the authoritative winner, persists the final state, and starts durable order creation for a sold auction.
- **Buy now:** Auction reserves the listing and returns a purchase receipt with `202 Accepted`. Orchestration creates a held order, completes the auction, and confirms the order; the browser polls the receipt for the outcome.

See the [Architecture Guide](docs/architecture.md) for communication patterns and
consistency boundaries, and [Orchestration](src/Orchestration/README.md) for state
transitions, compensation, and recovery.

## Getting Started

### Prerequisites

| Tool | Minimum Version |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 9.0 |
| [Node.js](https://nodejs.org) | 20 LTS |
| [Docker Desktop](https://www.docker.com/products/docker-desktop) | 24+ |
| [Docker Compose](https://docs.docker.com/compose/) | v2 |

### Local Development (Docker Compose)

```bash
# 1. Clone
git clone https://github.com/blanho/auction-platform-microservices.git
cd auction-platform-microservices

# 2. Configure environment
cp .env.example .env          # Fill in required values

# 3. Start everything
docker compose -f deploy/docker/docker-compose.yml up -d

# 4. Verify health
docker compose -f deploy/docker/docker-compose.yml ps

# 5. Start the frontend dev server
cd web && npm install && npm run dev
```

The full Compose stack serves the frontend at http://localhost:3000; Vite serves
it at http://localhost:5173. The gateway is at http://localhost:6001.
See [Getting Started](docs/getting-started.md) for environment configuration,
individual services, debugging, and troubleshooting.

## Cloud and Deployment

Azure production uses Bicep, AKS, ACR, Key Vault, PostgreSQL Flexible Server,
Azure Managed Redis, and Blob Storage. The default overlay runs RabbitMQ and
Elasticsearch in AKS; the optional managed-provider overlay connects to HA
providers and enables Azure SignalR. Cloud capacity and failover still require
staging validation.

- [Deployment guide](docs/deployment.md): Docker, Kubernetes, CI/CD, secrets, monitoring, and rollback.
- [Azure deployment guide](deploy/azure/README.md): provisioning, GitHub configuration, DNS, and production rollout.
- [Architecture diagrams](docs/diagrams/README.md): Azure deployment, runtime flows, Kubernetes details, and the editable draw.io file.
- [Production readiness](docs/production-readiness.md): managed-provider migration, rollout checks, and remaining limits.

## Testing

Follow the [testing guide](docs/testing.md) to build and run all backend projects
with disposable PostgreSQL, RabbitMQ, and Elasticsearch, and to run frontend and
browser checks. The backend runner rejects failed, missing, or unexpectedly
skipped results. Use the [load-testing guide](tests/load/README.md) for staging
capacity and failure drills.

Backend builds run the pinned Sonar C# analyzer with warnings treated as errors.
Frontend validation uses ESLint and TypeScript. See [Code Quality](docs/code-quality.md)
for commands, maintenance boundaries, and localization requirements.

## Documentation

| Document | Description |
|---|---|
| [Architecture Guide](docs/architecture.md) | System design, bounded contexts, data flow, design decisions |
| [Cloud Architecture Diagrams](docs/diagrams/README.md) | Editable Azure deployment, runtime flow, and Kubernetes detail diagrams with SVG previews |
| [Getting Started Guide](docs/getting-started.md) | Step-by-step onboarding for new developers |
| [API Reference](docs/api-reference.md) | Gateway routes, authentication, request/response schemas |
| [Background Jobs](docs/background-jobs.md) | Supported job operations, payload examples, retries, and staged roadmap |
| [Paging Guide and Consistency](docs/paging-consistency.md) | Shared paging rules, typed immutable sort maps across services, compatibility differences, and verification |
| [Code Quality](docs/code-quality.md) | Local Sonar analysis, verification commands, security configuration, and reviewed exceptions |
| [Common Repositories](docs/common-repositories.md) | Research, shared EF Core CRUD and queries, audit policies, transaction boundaries, and adoption |
| [Deployment Guide](docs/deployment.md) | Docker, Kubernetes, CI/CD pipeline details |
| [Azure Deployment Guide](deploy/azure/README.md) | Provision and deploy the Azure AKS environment |
| [Testing](docs/testing.md) | Backend test runner, disposable providers, frontend checks, and browser journeys |
| [Orchestration](src/Orchestration/README.md) | Durable sale workflows, compensation, deployment, and operator recovery |
| [Production Readiness](docs/production-readiness.md) | Managed providers, rollout, capacity validation, and remaining HA limits |
| [Load Testing](tests/load/README.md) | Staging capacity tests and failure drills |

## License

This project is for educational and portfolio purposes.
