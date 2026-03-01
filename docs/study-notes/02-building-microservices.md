# Building Microservices — Sam Newman

> The practitioner's guide to designing, building, and operating microservices architectures.

---

## Chapter 1: Microservices

### Key Ideas

Microservices are **independently deployable services** modeled around a **business domain**. They are a type of service-oriented architecture (SOA) with specific opinions about:

- Size: Small enough for one team to own. Not a line-count metric.
- Independence: Each service can be deployed, scaled, and evolved independently.
- Ownership: A team owns the full lifecycle — build, deploy, operate.
- Data isolation: Each service owns its data. No shared databases.

Microservices are NOT:
- Just "small services" — size is about scope, not lines of code
- A silver bullet — they trade one set of problems for another
- Required — monoliths are fine for many organizations

### Key Benefits

| Benefit | How |
|---------|-----|
| Technology heterogeneity | Each service picks its own stack |
| Resilience | Failure is isolated to one service |
| Scaling | Scale only the services that need it |
| Ease of deployment | Small, focused changes; lower deployment risk |
| Organizational alignment | Teams own services end-to-end (Conway's Law) |
| Composability | Services can be recombined for new use cases |

### Key Costs

| Cost | Reality |
|------|---------|
| Distributed system complexity | Network calls, partial failures, eventual consistency |
| Operational overhead | More services = more to deploy, monitor, debug |
| Data consistency | No cross-service transactions |
| Testing complexity | End-to-end testing is harder |
| Organizational maturity | Requires DevOps culture, CI/CD, observability |

### Modern Backend Application

The auction platform as microservices:
```
AuctionService    — auction lifecycle (create, update, close)
BidService        — bid validation, placement, history
IdentityService   — authentication, authorization, user profiles
PaymentService    — payment processing, escrow, refunds
NotificationService — email, SMS, push, real-time WebSocket
SearchService     — full-text search, filtering, ranking
StorageService    — file/image storage
JobService        — scheduled tasks (auction closing, cleanup)
```

Each service: own database, own deployment pipeline, own API, own team.

### Interview Questions

1. **When would you recommend a monolith over microservices? Give specific criteria.**
2. **How do you decide the right boundary for a microservice?**
3. **What is Conway's Law and how does it affect microservice architecture?**
4. **Your team of 5 manages 20 microservices. What problems will you face?**
5. **How do you handle a cross-cutting concern (logging, auth) across 15 microservices?**

### Common Mistakes

- Starting with microservices before understanding the domain (decompose wrong)
- Creating too many, too small services ("nanoservices") — overhead exceeds benefit
- Shared database between services — kills independence, creates coupling
- Not investing in observability, CI/CD, and infrastructure before adopting microservices

---

## Chapter 2: How to Model Services

### Key Ideas

Good microservice boundaries come from **Domain-Driven Design (DDD)**:

- **Bounded Context**: A boundary within which a particular domain model is defined and applicable. Each microservice should align with one bounded context.
- **Aggregate**: A cluster of domain objects treated as a unit for data changes. Aggregates are the natural unit for microservice data ownership.
- **Ubiquitous Language**: Within a bounded context, everyone (developers, domain experts) uses the same terms.

The key insight: **Loose coupling and high cohesion** at the service level.

- **High cohesion**: Related behavior lives together. Change one thing → change one service.
- **Loose coupling**: Services know as little as possible about each other. Change one service → no changes to other services.

### Decomposition Strategies

| Strategy | Description | Example |
|----------|-------------|---------|
| By business capability | Align with what the business does | AuctionService, PaymentService |
| By subdomain | Align with DDD subdomains | Core (Auction), Supporting (Notification), Generic (Identity) |
| By data ownership | Each service owns its data entity | BidService owns bids table |
| Strangler fig | Incrementally extract from monolith | Extract PaymentService first |

### Real-World Example: Auction Platform Bounded Contexts

```
┌─────────────────────────┐  ┌─────────────────────────┐
│  Auction Context         │  │  Bidding Context         │
│  - Auction (aggregate)   │  │  - Bid (aggregate)       │
│  - Category              │  │  - BidHistory            │
│  - AuctionRules          │  │  - AutoBid               │
│  Language: "listing",    │  │  Language: "bid",         │
│  "reserve price", "lot"  │  │  "increment", "outbid"   │
└─────────────────────────┘  └─────────────────────────┘

┌─────────────────────────┐  ┌─────────────────────────┐
│  Identity Context        │  │  Payment Context         │
│  - User (aggregate)      │  │  - Payment (aggregate)   │
│  - Role, Permission      │  │  - Escrow, Refund        │
│  Language: "account",    │  │  Language: "charge",      │
│  "profile", "credential" │  │  "payout", "dispute"     │
└─────────────────────────┘  └─────────────────────────┘
```

Each context has its own "User" concept but with different attributes:
- Identity: User has email, password, roles
- Bidding: Bidder has bid history, auto-bid settings
- Payment: Payer has payment methods, balance

### Interview Questions

1. **How do you identify bounded contexts in a new domain you're unfamiliar with?**
2. **Two services both need "user" data. How do you avoid a shared database?**
3. **What's the difference between a bounded context and a microservice?**
4. **Explain the difference between core, supporting, and generic subdomains.**
5. **You're splitting a monolith into microservices. How do you decide which service to extract first?**

### Common Mistakes

- Creating microservices around technical layers (UserAPI, UserDB, UserCache) instead of business capabilities
- Ignoring bounded contexts — ending up with a "distributed monolith"
- Sharing domain models between services via a common library (tight coupling)
- Not talking to domain experts — engineering team guesses wrong boundaries

---

## Chapter 3: Split the Monolith

### Key Ideas

Migrating from monolith to microservices should be **incremental**, not a big-bang rewrite.

**Strangler Fig Pattern**: Build new functionality as services alongside the monolith. Route traffic to the new service. Gradually move features until the monolith shrinks to nothing.

**Decomposition steps**:
1. Identify seams in the code (modules with minimal dependencies)
2. Extract a seam as a separate service
3. Break the database: separate the schema
4. Redirect clients to the new service

**Database decomposition** is the hardest part:
- Start by separating read and write paths (CQRS)
- Split tables that clearly belong to one bounded context
- Handle shared tables with data synchronization or denormalization
- Foreign keys across service boundaries become API calls or events

### Patterns for Migration

| Pattern | When to Use |
|---------|------------|
| Strangler Fig | Gradual migration, feature by feature |
| Branch by Abstraction | Need to swap implementation behind an interface |
| Parallel Run | Run old and new simultaneously, compare results |
| Decorating Collaborator | Add behavior without changing existing code |

### Interview Questions

1. **Describe the Strangler Fig pattern. How do you handle database dependencies during extraction?**
2. **Your monolith has a `users` table referenced by 15 modules. How do you split it?**
3. **What's a "distributed monolith" and how do you avoid creating one during migration?**
4. **How do you ensure data consistency when splitting a monolith's database?**
5. **What is the "parallel run" pattern and when would you use it during migration?**

### Common Mistakes

- Big-bang rewrite — high risk, takes forever, often fails
- Extracting services without breaking the shared database → distributed monolith
- Not having a rollback strategy during migration
- Ignoring data migration — the data split is harder than the code split

---

## Chapter 4: Communication Styles

### Key Ideas

How services talk to each other is one of the most important architectural decisions.

**Synchronous (request/response)**:
- REST/HTTP: Universal, simple, stateless. Good for external APIs.
- gRPC: Fast, typed, bi-directional streaming. Good for internal service-to-service.
- **Drawback**: Temporal coupling — caller blocks until responder replies.

**Asynchronous (event-driven)**:
- Message queues (RabbitMQ): Point-to-point, request/response async patterns.
- Event brokers (Kafka): Publish-subscribe, event log, replay.
- **Benefit**: Temporal decoupling — sender doesn't wait.

| Aspect | REST | gRPC | Message Queue | Event Broker |
|--------|------|------|--------------|-------------|
| Coupling | Temporal | Temporal | Loose | Loosest |
| Latency | Higher (HTTP/JSON) | Lower (HTTP/2+Protobuf) | Variable | Variable |
| Contract | OpenAPI/Swagger | .proto files | Message schema | Event schema |
| Discovery | Service registry / DNS | Same | Broker URL | Broker URL |
| Error handling | HTTP status codes | gRPC status codes | Dead letter queue | DLQ / retry topic |
| Debugging | Easy (browser, curl) | Harder | Queue inspection | Log offset inspection |

**Orchestration vs. Choreography**:

| | Orchestration | Choreography |
|--|---------------|-------------|
| Control | Central orchestrator | Distributed, event-driven |
| Visibility | Clear workflow, easy to trace | Hard to trace full flow |
| Coupling | Services coupled to orchestrator | Services coupled to events |
| Example | Saga with orchestrator | Each service reacts to events |
| When to use | Complex workflows with conditions | Simple, linear event flows |

### Modern Backend Application

In the auction platform:
- **Synchronous**: Web client → API Gateway → AuctionService (REST/JSON)
- **gRPC**: BidService → AuctionService (internal, high-throughput bid validation)
- **Async (RabbitMQ)**: BidPlaced event → NotificationService sends email
- **Orchestrated Saga**: PlaceOrder → PaymentService → AuctionService → NotificationService (orchestrator coordinates)

### Interview Questions

1. **Design the communication strategy for an auction platform. Which calls are sync vs. async? Why?**
2. **Compare REST vs. gRPC for internal microservice communication.**
3. **Your notification service is slow and causes timeouts in the bidding flow. How do you fix this?**
4. **When would you choose orchestration over choreography for a saga?**
5. **How do you handle eventual consistency in an async, event-driven architecture?**

### Practical Exercises

1. Implement the same service call in REST and gRPC. Benchmark latency and throughput for 10K requests.
2. Convert a synchronous call chain (BidService → AuctionService → NotificationService) to async using RabbitMQ. Measure the latency improvement for the client.
3. Implement a simple orchestrator saga: CreateAuction → ValidatePayment → ConfirmListing. Handle compensation (rollback) if any step fails.

### Common Mistakes

- Making all calls synchronous → cascading failures, high latency
- Making all calls asynchronous → hard to debug, complex error handling
- Not designing for idempotency in async communication
- Ignoring backpressure — fast producer overwhelms slow consumer

---

## Chapter 5: Implementing Communication

### Key Ideas

This chapter covers practical patterns for service discovery, API management, and handling failures in inter-service communication.

**Service Discovery**:
- **DNS-based**: Simple, works with Kubernetes Services
- **Service registry (Consul, Eureka)**: Dynamic, health-checked
- **Service mesh (Istio, Linkerd)**: Sidecar proxy handles discovery, load balancing, retries

**API Gateway patterns**:
- Single entry point for external clients
- Handles: routing, authentication, rate limiting, response aggregation, protocol translation
- Risk: becomes a monolithic bottleneck if too much logic is placed here

**Failure handling**:
- **Timeouts**: Always set timeouts on remote calls. No call should wait forever.
- **Retries**: Retry transient failures with exponential backoff + jitter
- **Circuit breaker**: After N failures, stop calling the failing service for a period
- **Bulkhead**: Isolate resources (thread pools, connections) per downstream service so one failing service can't exhaust all resources
- **Fallback**: Return degraded results (cached data, default values) when a service is unavailable

### Circuit Breaker States

```
[Closed] → failures exceed threshold → [Open]
   ↑                                      ↓
   └── success ← [Half-Open] ← timeout expires
   
Closed: Requests pass through. Failures counted.
Open: Requests fail immediately. No calls to downstream.
Half-Open: Allow one test request. If success → Closed. If failure → Open.
```

### Modern Backend Application

In .NET with Polly library:
```csharp
services.AddHttpClient<IAuctionServiceClient>()
    .AddPolicyHandler(Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt))))
    .AddPolicyHandler(Policy
        .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.ServiceUnavailable)
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
```

### Interview Questions

1. **Design a resilient inter-service communication layer for the auction platform.**
2. **How does a circuit breaker prevent cascading failures?**
3. **Compare service mesh (Istio) with application-level resilience (Polly). When would you use each?**
4. **Your BidService calls 3 downstream services. One is slow. How do you prevent it from affecting the other two?**
5. **How do you implement idempotency for a payment processing API?**

### Practical Exercises

1. Implement a circuit breaker using Polly in .NET. Simulate downstream failures and observe the state transitions (Closed → Open → Half-Open → Closed).
2. Implement the bulkhead pattern: Use separate HttpClient instances (with separate connection pools) for each downstream service.
3. Set up a service mesh (Istio on Kubernetes) and demonstrate automatic retry, timeout, and circuit breaking without application code changes.

### Common Mistakes

- No timeouts on HTTP calls → thread pool exhaustion during downstream failures
- Retry without jitter → thundering herd when a service recovers
- Circuit breaker threshold too sensitive → unnecessary failures during minor hiccups
- API Gateway doing too much → becomes a distributed monolith bottleneck

---

## Chapter 6: Deployment

### Key Ideas

Independent deployability is the core benefit of microservices. Your deployment strategy must support this.

**Deployment patterns**:

| Pattern | Description | Downtime | Risk | Rollback |
|---------|------------|----------|------|----------|
| Rolling update | Replace instances one at a time | None | Medium | Reverse rolling |
| Blue-green | Switch traffic from old (blue) to new (green) | None | Low | Switch back |
| Canary | Route small % of traffic to new version | None | Lowest | Route back |
| Feature flags | Deploy code but toggle features on/off | None | Low | Toggle off |

**Containerization**:
- Docker standardizes the deployment unit
- Kubernetes orchestrates containers: scheduling, scaling, self-healing, service discovery
- Each microservice is a container image with its own Dockerfile

**CI/CD pipeline** for microservices:
```
Code Commit → Build → Unit Tests → Integration Tests → Container Build 
→ Push to Registry → Deploy to Staging → Smoke Tests → Deploy to Production (canary)
→ Monitor → Full rollout or rollback
```

**One pipeline per service**: Each microservice should have its own independent CI/CD pipeline. A change to BidService should not require rebuilding or redeploying AuctionService.

### Modern Backend Application

In the auction platform:
```
GitHub repo (monorepo or multi-repo)
  → GitHub Actions per service
  → Docker build → Push to ACR/ECR
  → Helm chart per service
  → ArgoCD deploys to Kubernetes
  → Canary rollout (10% → 50% → 100%)
  → Prometheus + Grafana monitors error rate
  → Auto-rollback if error rate > threshold
```

### Interview Questions

1. **Design a CI/CD pipeline for a microservices platform with 10 services.**
2. **Compare blue-green, canary, and rolling deployments. When would you use each?**
3. **How do you handle database migrations in a rolling deployment where old and new code coexist?**
4. **Your canary deployment shows 2% error rate for the new version. What do you do?**
5. **Monorepo vs. multi-repo for microservices — what are the trade-offs?**

### Common Mistakes

- Manual deployments — human error is the #1 cause of outages
- Deploying all services together ("lockstep deployment") — defeats the purpose of microservices
- Not testing rollback procedures — you discover rollback is broken during an incident
- Ignoring database backward compatibility during deployments

---

## Chapter 7: Testing

### Key Ideas

Testing microservices requires a layered strategy. The **test pyramid** still applies but with new considerations:

```
         /  E2E Tests  \         ← Few, slow, brittle
        / Contract Tests \       ← Verify service interfaces
       / Integration Tests \     ← Test service + dependencies
      /    Unit Tests       \    ← Many, fast, isolated
```

**Key testing types for microservices**:

| Type | What It Tests | Speed | Confidence | Brittleness |
|------|--------------|-------|-----------|-------------|
| Unit | Single class/method | ms | Low-Medium | Low |
| Integration | Service + DB/queue | seconds | Medium | Medium |
| Contract | API compatibility between services | seconds | High (for integration) | Low |
| End-to-end | Full system flow | minutes | Highest | Highest |

**Consumer-driven contract tests** are the killer pattern for microservices:
- Each consumer defines what it needs from a provider
- Provider tests verify it still satisfies all consumer contracts
- Catches breaking API changes before deployment
- Tools: Pact, Spring Cloud Contract

**Testing in production** (carefully):
- Canary releases, feature flags
- Synthetic transactions (fake user flows in production)
- Chaos engineering (Netflix Chaos Monkey)

### Modern Backend Application

In the auction platform:
- **Unit**: Test `Bid.Place()` domain logic (max bid validation, increment rules)
- **Integration**: Test BidService + PostgreSQL (actual database in Docker)
- **Contract**: BidService produces `BidPlaced` event → AuctionService consumes it. Both test against the same contract.
- **E2E**: "User creates auction → another user bids → auction closes → payment processed" — run sparingly

### Interview Questions

1. **Design a testing strategy for a microservices auction platform.**
2. **Explain consumer-driven contract testing. How does it prevent breaking changes?**
3. **Your end-to-end tests take 45 minutes and fail 30% of the time. What do you do?**
4. **How do you test an event-driven system where services communicate asynchronously?**
5. **What is chaos engineering and when should you introduce it?**

### Common Mistakes

- Over-investing in E2E tests (slow, flaky, expensive to maintain)
- No contract tests → services break each other's APIs unknowingly
- Testing against mocks that don't match real behavior
- Not testing failure scenarios (network errors, timeouts, out-of-order events)

---

## Chapter 8: Monitoring and Observability

### Key Ideas

In a monolith, you watch one application. In microservices, you must observe **dozens of services interacting**. The three pillars of observability:

**1. Logs**: Structured events from each service
- Centralize with ELK (Elasticsearch, Logstash, Kibana) or Loki
- Always include correlation IDs for tracing requests across services
- Structure: JSON with timestamp, service, level, correlation ID, message

**2. Metrics**: Numeric measurements over time
- **RED method** (for services): Rate, Errors, Duration
- **USE method** (for resources): Utilization, Saturation, Errors
- Tools: Prometheus + Grafana
- Key metrics: request rate, error rate, latency (p50/p95/p99), saturation

**3. Distributed Tracing**: Following a request across multiple services
- Each request gets a trace ID; each service creates a span
- Tools: Jaeger, Zipkin, OpenTelemetry
- Critical for debugging latency in multi-service call chains

**Alerting philosophy**:
- Alert on symptoms (high error rate, high latency), not causes
- Every alert should be actionable
- Use error budgets (SRE) to decide when to alert

### Architecture Diagram (Text)

```
Service A ──→ Service B ──→ Service C
   │              │              │
   ├─ logs ──→ ┌──┴──────────────┴──┐
   ├─ metrics → │  Observability Stack │
   └─ traces ─→ │  - ELK (logs)       │
                │  - Prometheus (metrics)|
                │  - Jaeger (traces)    │
                │  - Grafana (dashboards)|
                └───────────────────────┘
                         ↓
                   Alert Manager → PagerDuty → On-call engineer
```

### Interview Questions

1. **Design the observability stack for a microservices auction platform.**
2. **A user reports "bidding is slow." How do you diagnose the problem using the three pillars?**
3. **What is a correlation ID and why is it essential in microservices?**
4. **How do you set up meaningful alerts that don't cause alert fatigue?**
5. **Compare the RED and USE methods for monitoring. When do you use each?**

### Common Mistakes

- Logging without correlation IDs → impossible to trace requests
- Alerting on causes instead of symptoms → alert fatigue
- No distributed tracing → "it's slow" becomes an unsolvable mystery
- Monitoring only infrastructure (CPU, memory) and not business metrics (bids/sec, conversion rate)

---

## Chapter 9: Security

### Key Ideas

Microservices expand the attack surface dramatically. Every service-to-service call is a potential vulnerability.

**Defense in depth**:

| Layer | Protection |
|-------|-----------|
| Edge / API Gateway | Authentication, rate limiting, WAF |
| Service-to-service | Mutual TLS (mTLS), service mesh |
| Application | Authorization (RBAC/ABAC), input validation |
| Data | Encryption at rest, field-level encryption |
| Infrastructure | Network policies, least privilege |

**Authentication & Authorization**:
- **OAuth 2.0 + OpenID Connect**: Industry standard for API authentication
- **JWT tokens**: Stateless auth tokens passed between services. Include claims (user ID, roles, permissions).
- **API Gateway validates JWT**: Downstream services trust the gateway
- **Service mesh mTLS**: Services authenticate each other using certificates

**Confused Deputy Problem**: Service A calls Service B on behalf of User X. Service B must verify that User X (not just Service A) is authorized for the operation. Solution: Pass the user's token/context through the call chain.

### Modern Backend Application

In the auction platform:
```
User → [API Gateway: JWT validation, rate limiting]
     → [AuctionService: RBAC (admin can create auction, user can bid)]
     → [PaymentService: mTLS + user token forwarding]
     → [Database: encrypted at rest, column-level encryption for payment data]
```

### Interview Questions

1. **Design the security architecture for an auction platform's microservices.**
2. **Explain the confused deputy problem with a microservices example.**
3. **How do you handle authentication in a microservices architecture? JWT vs. session-based?**
4. **How does a service mesh (Istio) improve security between services?**
5. **A vulnerability is discovered in one microservice. How do you limit the blast radius?**

### Common Mistakes

- Trusting inter-service calls without authentication ("it's internal, so it's safe")
- Storing secrets in code or config files instead of a vault (HashiCorp Vault, AWS Secrets Manager)
- Not rotating credentials and certificates
- Validating authentication but not authorization — "is the user logged in?" ≠ "can the user do this?"

---

## Chapter 10: Conway's Law and System Design

### Key Ideas

> "Organizations which design systems are constrained to produce designs which are copies of the communication structures of these organizations." — Melvin Conway

**The Inverse Conway Maneuver**: Structure your teams to match the architecture you want. If you want independent microservices, create independent teams aligned to those services.

**Team topologies for microservices**:
- **Stream-aligned teams**: Own a business capability end-to-end (Team Auction, Team Bidding)
- **Platform teams**: Provide shared infrastructure (Kubernetes, CI/CD, observability)
- **Enabling teams**: Help stream-aligned teams adopt new technologies
- **Complicated subsystem teams**: Own deeply specialized components (ML, payment processing)

### Team-Service Alignment

```
Team Auction ──→ AuctionService, SearchService
Team Bidding ──→ BidService, AutoBidService
Team Payments ─→ PaymentService, EscrowService
Team Platform ─→ Kubernetes, CI/CD, Monitoring, API Gateway
Team Identity ─→ IdentityService, AuthorizationService
```

### Interview Questions

1. **How does Conway's Law affect microservice boundaries?**
2. **Your org has a frontend team, backend team, and DBA team. What architecture will they produce?**
3. **Design team structure for a company building a 12-service auction platform.**
4. **What is the Inverse Conway Maneuver and how do you apply it?**
5. **How do you handle a service that two teams need to change frequently?**

### Common Mistakes

- Ignoring Conway's Law — your architecture will mirror your org chart whether you plan it or not
- Creating a "shared services team" → bottleneck for all changes
- Not giving teams full ownership (build, deploy, operate)
- Frequent re-orgs that don't match the architecture → constant confusion

---

## Chapter 11: Resiliency

### Key Ideas

Microservices must be designed to **handle failure gracefully**, not to prevent failure.

**The key principle**: Expect everything to fail and design for it.

**Resilience patterns**:

| Pattern | Purpose | How |
|---------|---------|-----|
| Timeout | Don't wait forever | Set deadline on every remote call |
| Retry | Handle transient failures | Exponential backoff + jitter |
| Circuit breaker | Stop cascading failures | Open circuit after N failures |
| Bulkhead | Isolate blast radius | Separate thread pools / resources |
| Fallback | Graceful degradation | Return cached/default data |
| Idempotency | Safe retries | Operations produce same result regardless of repetitions |
| Hedging | Reduce tail latency | Send same request to multiple replicas, take first response |

**Cascading failure anatomy**:
```
Service A → Service B (slow) → A's threads exhaust → A becomes slow 
→ Service C calls A → C's threads exhaust → ...entire system down
```

Prevention: Timeouts + circuit breakers + bulkheads at every service boundary.

**Chaos engineering**: Deliberately inject failures in production to find weaknesses before they find you.
- Kill random pods (Chaos Monkey)
- Inject network latency
- Fill disk space
- Corrupt messages

### Interview Questions

1. **Your auction platform has a cascading failure. Service A calls B, B calls C, C is down. Design the resilience strategy.**
2. **Explain the bulkhead pattern. How would you implement it for the bidding service?**
3. **What is chaos engineering? How would you introduce it safely?**
4. **Design a fallback strategy for the search service when Elasticsearch is down.**
5. **How do you make a payment retry safe (idempotent)?**

### Common Mistakes

- Assuming cloud infrastructure means your services won't fail
- Not testing failure scenarios → first outage is a surprise
- Setting the same timeout for all downstream calls (they have different latency profiles)
- Retrying without backoff → amplifying the problem during overload

---

## Chapter 12: Scaling

### Key Ideas

Scaling microservices involves multiple dimensions:

**Scaling dimensions**:
1. **Vertical**: Bigger machines (CPU, RAM) — simplest, has limits
2. **Horizontal**: More instances behind a load balancer — primary strategy for microservices
3. **Data partitioning**: Shard data across multiple databases
4. **Functional decomposition**: Already done by microservices — each service scales independently

**The four key scaling techniques**:

| Technique | Description | Use Case |
|-----------|-------------|----------|
| Caching | Store computed results | Read-heavy services (search, catalog) |
| Autoscaling | Add/remove instances based on load | Kubernetes HPA on CPU/request count |
| CQRS | Separate read and write models | High read:write ratio (auction listing views) |
| Async processing | Queue work for later | Spike absorption (bid processing during auction close) |

**Caching strategies**:
- **Client-side**: Browser, mobile app cache
- **CDN**: Edge caching for static assets and API responses
- **API Gateway**: Cache responses for identical requests
- **Application**: In-memory cache (Redis, Memcached)
- **Database**: Query result caching

**Cache invalidation** (the hardest problem):
- TTL (time-to-live): Simple, eventual consistency
- Write-through: Update cache on every write
- Write-behind: Queue cache updates asynchronously
- Event-driven: Invalidate cache when events arrive

### Modern Backend Application

```
Auction Platform Scaling Strategy:
                                    
AuctionService:  2 replicas (low write volume)
BidService:      10 replicas (high write volume during peak)
SearchService:   5 replicas + Elasticsearch cluster (high read volume)
NotificationService: 3 replicas + queue-based (async, burst-friendly)

HPA rules:
- BidService: scale up when requests/sec > 1000 per pod
- SearchService: scale up when p95 latency > 200ms

Redis cache:
- Auction details: TTL 30s (read-heavy, tolerance for slight staleness)
- Current highest bid: event-driven invalidation (must be fresh)
```

### Interview Questions

1. **Design the scaling strategy for an auction platform expecting 10x traffic during a celebrity auction.**
2. **How does CQRS help with scaling read-heavy workloads?**
3. **Compare TTL-based vs. event-driven cache invalidation for the "current highest bid."**
4. **Your system handles 100 bids/sec normally but 10,000 bids/sec during the last minute of popular auctions. Design for this.**
5. **What's the difference between scaling for throughput vs. scaling for latency?**

### Common Mistakes

- Caching everything without thinking about invalidation → stale data bugs
- Autoscaling too slowly → by the time new instances are ready, the spike is over
- Not load testing with realistic traffic patterns
- Scaling all services equally instead of identifying and scaling the bottleneck

---

## Summary: Key Takeaways from Building Microservices

| Chapter | One-Line Takeaway |
|---------|-------------------|
| 1 | Microservices are independently deployable services modeled around business domains |
| 2 | Use DDD bounded contexts to find service boundaries — high cohesion, loose coupling |
| 3 | Split the monolith incrementally using Strangler Fig — database split is the hardest part |
| 4 | Choose sync vs. async communication based on coupling requirements, not convenience |
| 5 | Every remote call needs timeouts, retries, circuit breakers — build resilience into the communication layer |
| 6 | Independent CI/CD pipelines per service; use canary deployments for safety |
| 7 | Test pyramid + contract tests; don't over-invest in end-to-end tests |
| 8 | Three pillars: logs, metrics, traces — correlation IDs are non-negotiable |
| 9 | Defense in depth — mTLS, JWT, API gateway, encryption, least privilege |
| 10 | Conway's Law is real — structure teams to match your desired architecture |
| 11 | Design for failure — timeouts, circuit breakers, bulkheads, chaos testing |
| 12 | Scale independently — cache aggressively, use CQRS, autoscale the bottleneck |
