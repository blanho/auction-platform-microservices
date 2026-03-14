# Deployment Guide

This document covers how the auction platform is deployed across local development, staging, and production environments.

---

## Table of Contents

- [Deployment Environments](#deployment-environments)
- [Docker Compose (Local Development)](#docker-compose-local-development)
- [Container Images](#container-images)
- [Kubernetes Architecture](#kubernetes-architecture)
- [Kustomize Structure](#kustomize-structure)
- [Deploying to Kubernetes](#deploying-to-kubernetes)
- [Production Configuration](#production-configuration)
- [CI/CD Pipeline](#cicd-pipeline)
- [Secrets Management](#secrets-management)
- [Scaling Strategy](#scaling-strategy)
- [Database Management](#database-management)
- [Monitoring and Alerting](#monitoring-and-alerting)
- [Rollback Procedures](#rollback-procedures)

---

## Deployment Environments

| Environment | Infrastructure | Purpose |
|---|---|---|
| **Local** | Docker Compose | Developer workstation, full stack |
| **Dev** | Kubernetes (dev overlay) | Shared development, integration testing |
| **Staging** | Kubernetes (staging overlay) | Pre-production validation |
| **Production** | Kubernetes (production overlay) | Live traffic |

---

## Docker Compose (Local Development)

The `deploy/docker/docker-compose.yml` file defines the complete local stack.

### Infrastructure Services

| Service | Image | Ports | Volumes |
|---|---|---|---|
| PostgreSQL 16 | `postgres:16-alpine` | 5432 | `postgres_data` |
| Redis 7 | `redis:7-alpine` | 6379 | `redis_data` |
| RabbitMQ 3.13 | `rabbitmq:3.13-management-alpine` | 5672, 15672 | `rabbitmq_data` |
| Elasticsearch 8 | `elasticsearch:8.11.3` | 9200, 9300 | `elasticsearch_data` |
| Seq | `datalust/seq:latest` | 5341 | `seq_data` |

### Database Initialization

The `deploy/docker/scripts/init-databases.sh` script runs on first PostgreSQL startup. It creates per-service databases:
- `auction_db`, `bid_db`, `payment_db`, `notification_db`
- `identity_db`, `analytics_db`, `storage_db`, `job_db`

### Commands

```bash
# Full stack
docker compose -f deploy/docker/docker-compose.yml up -d

# Infrastructure only
docker compose -f deploy/docker/docker-compose.yml up -d postgres redis rabbitmq elasticsearch seq

# Rebuild a specific service
docker compose -f deploy/docker/docker-compose.yml build auction-api
docker compose -f deploy/docker/docker-compose.yml up -d auction-api

# View logs
docker compose -f deploy/docker/docker-compose.yml logs -f auction-api

# Tear down (preserve data)
docker compose -f deploy/docker/docker-compose.yml down

# Tear down (reset everything)
docker compose -f deploy/docker/docker-compose.yml down -v
```

---

## Container Images

Each service has a Dockerfile in its API project directory. All Dockerfiles use a multi-stage build:

```
Stage 1: SDK image → restore, build, publish
Stage 2: ASP.NET runtime image → copy published output, run
```

### Image Registry

Production images are published to **GitHub Container Registry (GHCR)**:

```
ghcr.io/blanho/auction-platform/auction-api:v1.0.0
ghcr.io/blanho/auction-platform/bidding-api:v1.0.0
ghcr.io/blanho/auction-platform/payment-api:v1.0.0
ghcr.io/blanho/auction-platform/notification-api:v1.0.0
ghcr.io/blanho/auction-platform/identity-api:v1.0.0
ghcr.io/blanho/auction-platform/analytics-api:v1.0.0
ghcr.io/blanho/auction-platform/search-api:v1.0.0
ghcr.io/blanho/auction-platform/storage-api:v1.0.0
ghcr.io/blanho/auction-platform/job-api:v1.0.0
ghcr.io/blanho/auction-platform/gateway-api:v1.0.0
```

### Building Images Locally

```bash
# Build a single service
docker build -t auction-api:local -f src/Services/Auction/Auction.Api/Dockerfile .

# Build all services
docker compose -f deploy/docker/docker-compose.yml build
```

---

## Kubernetes Architecture

The Kubernetes deployment uses Kustomize for environment-specific configuration.

```mermaid
graph TB
    subgraph Cluster["Kubernetes Cluster"]
        subgraph NS["Namespace: auction-platform"]
            ING["Ingress<br/>TLS Termination"]

            subgraph Deployments["Deployments"]
                GW["gateway-api<br/>Replicas: 3 (prod)"]
                AUC["auction-api"]
                BID["bidding-api<br/>Replicas: 3 (prod)"]
                PAY["payment-api"]
                NOT["notification-api"]
                IDN["identity-api"]
                SRC["search-api"]
                STR["storage-api"]
                ANA["analytics-api"]
                JOB["job-api"]
            end

            subgraph StatefulSets["StatefulSets"]
                PG["PostgreSQL<br/>100Gi PVC (prod)"]
                RMQ["RabbitMQ"]
                ES["Elasticsearch"]
            end

            subgraph Config["Configuration"]
                CM["ConfigMap"]
                SEC["Secrets / ExternalSecrets"]
                PDB["PodDisruptionBudgets"]
                PC["PriorityClasses"]
            end

            subgraph Mon["Monitoring"]
                SM["ServiceMonitors<br/>(Prometheus)"]
            end
        end
    end

    ING --> GW
    GW --> Deployments
    Deployments --> StatefulSets
    SM --> Deployments
```

---

## Kustomize Structure

```
deploy/kubernetes/
├── base/                              # Shared base manifests
│   ├── kustomization.yaml             # Resource list, labels
│   ├── namespace.yaml                 # auction-platform namespace
│   ├── configmap.yaml                 # Shared config (connection strings, etc.)
│   ├── secrets.yaml                   # Base secrets (overridden per env)
│   ├── ingress.yaml                   # Ingress with TLS
│   ├── priority-classes.yaml          # Pod priority classes
│   ├── rbac.yaml                      # ServiceAccounts, Roles, RoleBindings
│   ├── services/
│   │   ├── auction-api.yaml           # Deployment + Service + HPA
│   │   ├── bidding-api.yaml
│   │   ├── payment-api.yaml
│   │   ├── notification-api.yaml
│   │   ├── identity-api.yaml
│   │   ├── analytics-api.yaml
│   │   ├── search-api.yaml
│   │   ├── storage-api.yaml
│   │   ├── job-api.yaml
│   │   ├── gateway-api.yaml
│   │   └── pdb.yaml                   # PodDisruptionBudgets for all services
│   ├── infrastructure/
│   │   ├── postgres.yaml              # StatefulSet + PVC
│   │   ├── redis.yaml                 # Deployment
│   │   ├── rabbitmq.yaml              # StatefulSet
│   │   ├── elasticsearch.yaml         # StatefulSet
│   │   ├── jaeger.yaml                # Deployment (tracing)
│   │   ├── seq.yaml                   # Deployment (logging)
│   │   └── rate-limiting.yaml         # Rate limiting config
│   └── monitoring/
│       └── service-monitors.yaml      # Prometheus ServiceMonitor resources
│
└── overlays/
    ├── dev/
    │   └── kustomization.yaml         # Minimal resources, debug logging
    ├── staging/
    │   └── kustomization.yaml         # Moderate resources, staging config
    └── production/
        ├── kustomization.yaml         # Full resources, GHCR images, replicas
        └── external-secrets.yaml      # ExternalSecrets for credential management
```

---

## Deploying to Kubernetes

### Prerequisites

- `kubectl` configured with cluster access
- Kustomize (built into kubectl v1.14+)
- Container images pushed to GHCR

### Deploy to Development

```bash
kubectl apply -k deploy/kubernetes/overlays/dev
```

### Deploy to Staging

```bash
kubectl apply -k deploy/kubernetes/overlays/staging
```

### Deploy to Production

```bash
kubectl apply -k deploy/kubernetes/overlays/production
```

### Verify Deployment

```bash
# Check all pods
kubectl get pods -n auction-platform

# Check services
kubectl get svc -n auction-platform

# Check pod logs
kubectl logs -n auction-platform deployment/auction-api -f

# Check pod health
kubectl describe pod -n auction-platform <pod-name>
```

---

## Production Configuration

Production-specific settings are in `deploy/config/appsettings.Production.template.json`:

### Logging
- Minimum level: **Warning** (not Debug/Information)
- Sinks: **Console** + **Elasticsearch** (Seq disabled)
- JSON format with service name enrichment

### Network
- HTTP on port **8080** (internal)
- gRPC on port **8081** (internal, for Auction ↔ Bidding)
- TLS terminated at Ingress level

### Observability
- OpenTelemetry traces exported to configurable OTLP endpoint
- Console exporter disabled

### Resources (Production Overlay)

| Resource | Requests | Limits |
|---|---|---|
| Backend services | 200m CPU, 512Mi memory | 1000m CPU, 1Gi memory |
| PostgreSQL | — | 2Gi–4Gi memory |
| Gateway, Bidding | 3 replicas | — |
| PostgreSQL storage | — | 100Gi PVC |

---

## CI/CD Pipeline

### Pipeline Flow

```mermaid
graph TD
    PR["Pull Request"] --> PRC["pr-checks.yml"]
    PRC --> |Pass| Merge["Merge to main"]
    Merge --> CI["ci.yml"]
    CI --> Build["Build all services"]
    CI --> Test["Run tests + coverage"]
    CI --> SONAR["sonarcloud.yml<br/>Quality gate"]
    Test --> |Pass| CD["cd.yml"]
    SONAR --> |Pass| CD
    CD --> Docker["Build Docker images"]
    Docker --> Push["Push to GHCR"]
    Push --> Deploy["Deploy to K8s"]
```

### Workflow Details

**pr-checks.yml** (Pull Request)
- Triggered on: Pull request to main
- Steps: Restore, build, run tests, lint frontend
- Must pass before merge is allowed

**ci.yml** (Continuous Integration)
- Triggered on: Push to main
- Steps: Build all projects, run all tests, collect code coverage
- Uploads coverage reports for SonarCloud

**sonarcloud.yml** (Code Quality)
- Triggered by: CI pipeline
- Steps: Run SonarCloud analysis, enforce quality gate
- Configuration: `sonar-project.properties`
- Quality gate timeout: 300 seconds

**cd.yml** (Continuous Deployment)
- Triggered by: Successful CI
- Steps: Build Docker images for all services, push to GHCR, deploy to Kubernetes
- Tags images with commit SHA and version

**scheduled.yml** (Security)
- Triggered on: Cron schedule
- Steps: Dependency vulnerability scanning

---

## Secrets Management

### Local Development
- Environment variables in `.env` file
- Docker Compose injects them into containers

### Kubernetes Dev/Staging
- Kubernetes Secrets in `base/secrets.yaml`
- Overridden per environment in overlay

### Kubernetes Production
- **ExternalSecrets** (`production/external-secrets.yaml`)
- Credentials stored in external secret manager (e.g., Azure Key Vault, AWS Secrets Manager)
- ExternalSecrets operator syncs them into Kubernetes Secrets

### Required Secrets

| Secret | Used By |
|---|---|
| PostgreSQL credentials | All database-backed services |
| Redis password | Auction, Bidding, Notification |
| RabbitMQ credentials | All services |
| JWT signing key | Identity, Gateway |
| Stripe keys | Payment |
| SendGrid API key | Notification |
| Twilio credentials | Notification |
| Firebase service account | Notification |
| OAuth client credentials | Identity |
| Elasticsearch credentials | Search, Logging |

---

## Scaling Strategy

### Horizontal Scaling

| Service | Default Replicas | Scaling Trigger |
|---|---|---|
| Gateway | 3 | CPU > 70%, request rate |
| Bidding | 3 | CPU > 70%, bid volume |
| Auction | 1 | CPU > 70% |
| Payment | 1 | CPU > 70% |
| Notification | 1 | Queue depth |
| Others | 1 | CPU > 70% |

Gateway and Bidding get 3 replicas by default because:
- Gateway handles all inbound traffic
- Bidding is the most latency-sensitive path (real-time auctions)

### Vertical Scaling

Increase resource limits in the Kustomize overlay:

```yaml
patches:
  - target:
      kind: Deployment
      name: bidding-api
    patch: |
      - op: replace
        path: /spec/template/spec/containers/0/resources/limits/memory
        value: "2Gi"
```

### Database Scaling

PostgreSQL is a StatefulSet with persistent storage. For production:
- Consider managed PostgreSQL (Azure Database for PostgreSQL, AWS RDS)
- Read replicas for query-heavy services
- Connection pooling with PgBouncer

---

## Database Management

### Migrations

EF Core migrations are applied automatically on service startup in development. For production, run migrations manually or as an init container:

```bash
# Apply migrations locally
cd src/Services/Auction/Auction.Infrastructure
dotnet ef database update --startup-project ../Auction.Api

# Create a new migration
dotnet ef migrations add MigrationName --startup-project ../Auction.Api
```

### Backups

For production PostgreSQL:
- Automated daily backups via pg_dump or managed service backups
- Point-in-time recovery enabled
- Test restore procedure regularly

---

## Monitoring and Alerting

### Health Checks

Every service exposes:
- `/health` — overall health
- `/health/ready` — readiness (Kubernetes readiness probe)
- `/health/live` — liveness (Kubernetes liveness probe)

### Prometheus Metrics

ServiceMonitors in `base/monitoring/service-monitors.yaml` configure Prometheus scraping for all services.

### Logging

| Environment | Log Sink | Access |
|---|---|---|
| Local | Seq | http://localhost:5341 |
| Production | Elasticsearch | Kibana dashboard |

### Tracing

OpenTelemetry traces are exported via OTLP to Jaeger (dev) or a managed tracing service (production).

### Recommended Alerts

| Alert | Condition | Severity |
|---|---|---|
| Service down | Pod restarts > 3 in 5 min | Critical |
| High latency | p99 > 500ms for 5 min | Warning |
| Error rate | 5xx rate > 1% for 5 min | Critical |
| Queue depth | RabbitMQ queue > 10k messages | Warning |
| Disk usage | PostgreSQL PVC > 80% | Warning |
| Memory | Pod memory > 90% limit | Warning |

---

## Rollback Procedures

### Rolling Back a Deployment

```bash
# Check rollout history
kubectl rollout history deployment/auction-api -n auction-platform

# Rollback to previous revision
kubectl rollout undo deployment/auction-api -n auction-platform

# Rollback to a specific revision
kubectl rollout undo deployment/auction-api -n auction-platform --to-revision=3
```

### Rolling Back a Database Migration

EF Core does not have automatic rollback in production. Options:
1. Apply a new migration that reverses the changes
2. Restore from backup (if destructive migration)
3. Use `dotnet ef database update PreviousMigrationName` to revert

### Emergency Procedures

1. **Scale to zero:** `kubectl scale deployment/auction-api --replicas=0 -n auction-platform`
2. **Check logs:** `kubectl logs -n auction-platform deployment/auction-api --previous`
3. **Rollback:** `kubectl rollout undo deployment/auction-api -n auction-platform`
4. **Verify:** `kubectl get pods -n auction-platform -w`
