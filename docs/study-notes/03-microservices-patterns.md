# Microservices Patterns — Chris Richardson

> The definitive patterns catalog for designing and implementing microservices architectures.

---

## Chapter 1: Escaping Monolithic Hell

### Key Ideas

Monoliths start great but become "monolithic hell" as they grow:
- Builds take minutes to hours
- Every change risks breaking something unrelated
- Scaling requires scaling the entire application
- Technology stack is locked in
- Onboarding new developers takes weeks

**Microservices are NOT the goal**. The goal is rapid, frequent, reliable delivery of complex applications. Microservices are one means to that goal.

**The Scale Cube** (three dimensions of scaling):

```
                 Y-axis: Functional decomposition
                 (split by service/function)
                    ↑
                    |
                    |
X-axis: Horizontal ←────→ Z-axis: Data partitioning
duplication               (split by data shard)
(clone & load balance)
```

- **X-axis**: Run multiple identical copies behind a load balancer
- **Y-axis**: Decompose into microservices (each handles a different function)
- **Z-axis**: Partition data (e.g., shard users A-M to server 1, N-Z to server 2)

### Core Trade-offs

| Monolith Advantage | Microservices Advantage |
|-------------------|------------------------|
| Simple development | Independent deployment |
| Simple testing | Technology diversity |
| Simple deployment | Independent scaling |
| Easy to reason about | Fault isolation |
| ACID transactions | Team autonomy |

### Interview Questions

1. **At what point should a growing startup consider moving from a monolith to microservices?**
2. **Explain the Scale Cube with a concrete example for an auction platform.**
3. **What is "monolithic hell" and what are its symptoms?**
4. **Can you have microservices benefits without a full microservices architecture?**
5. **How do you evaluate whether the complexity of microservices is worth it for a given project?**

---

## Chapter 2: Decomposition Strategies

### Key Ideas

Two main strategies for decomposing a system into services:

**1. Decompose by business capability**:
- What does the business *do*? → Each capability becomes a service
- Example: Auction Management, Bidding, Payment Processing, User Management
- Stable over time (business capabilities change slowly)

**2. Decompose by subdomain (DDD)**:
- Model the domain → identify subdomains → one service per subdomain
- **Core subdomains**: Competitive advantage (Auction, Bidding)
- **Supporting subdomains**: Needed but not differentiating (Notification)
- **Generic subdomains**: Commodity (Identity, Storage)

**The key guidelines**:
- **Single Responsibility Principle (SRP)**: Each service has one reason to change
- **Common Closure Principle (CCP)**: Things that change together belong together
- **No "God services"**: If a service is involved in every operation, the boundary is wrong

### Decomposition Anti-patterns

| Anti-pattern | Problem | Fix |
|-------------|---------|-----|
| Distributed monolith | Services coupled, must deploy together | Redefine boundaries, reduce sync calls |
| Chatty services | Too many inter-service calls per operation | Merge services or use async communication |
| Anemic services | Service is just a CRUD wrapper | Embed domain logic, use DDD aggregates |
| Data-driven decomposition | One service per DB table | Decompose by business capability instead |

### Modern Backend Application: Auction Platform Decomposition

```
Core Subdomains (competitive advantage):
├── AuctionService    → auction lifecycle, rules, categories
├── BidService        → bid placement, validation, auto-bid
└── SearchService     → advanced search, recommendations

Supporting Subdomains:
├── NotificationService → alerts, emails, real-time updates
├── JobService          → scheduled tasks, auction closing
└── AnalyticsService    → reporting, dashboards

Generic Subdomains:
├── IdentityService     → auth, user management
├── PaymentService      → payment processing, escrow
└── StorageService      → file/image storage
```

### Interview Questions

1. **How do you decide service boundaries for a new system you know nothing about?**
2. **What's the difference between decomposing by business capability vs. by subdomain?**
3. **You have a "UserService" that every other service depends on. Is this a good design?**
4. **How do you handle a feature that spans multiple services?**
5. **Explain the Common Closure Principle with a microservices example.**

### Common Mistakes

- Starting with microservices before understanding the domain → wrong boundaries
- Creating services around data entities instead of business capabilities
- Not recognizing a distributed monolith when you've created one
- Ignoring the "two-pizza team" rule — if a team can't own a service end-to-end, it's too big or the team structure is wrong

---

## Chapter 3: Interprocess Communication

### Key Ideas

Microservices must communicate. The communication style fundamentally affects your architecture.

**Communication styles matrix**:

| | One-to-one | One-to-many |
|--|-----------|-------------|
| **Synchronous** | Request/response (REST, gRPC) | — |
| **Asynchronous** | Async request/response, one-way notification | Publish/subscribe, publish/async responses |

**API-first design**: Define the API contract before implementing. Use:
- **OpenAPI/Swagger** for REST
- **Protocol Buffers** for gRPC
- **AsyncAPI** for event-driven interfaces

**Handling partial failures**:
Every remote call can fail. You MUST implement:
1. Network timeouts (connect + read timeout)
2. Limit outstanding requests (circuit breaker)
3. Retry with exponential backoff
4. Use fallbacks (cached data, default response)

**Service discovery patterns**:
- **Client-side discovery**: Client queries service registry, picks an instance (Eureka + Ribbon)
- **Server-side discovery**: Client calls a router/load balancer that queries the registry (Kubernetes Services, AWS ALB)
- **Service mesh**: Sidecar proxy handles discovery transparently (Istio, Linkerd)

### Message Formats

| Format | Pros | Cons | Use When |
|--------|------|------|----------|
| JSON | Human-readable, universal | Large, no schema enforcement | Public APIs |
| Protobuf | Compact, typed, backward compatible | Not human-readable | Internal gRPC |
| Avro | Best schema evolution, compact | Needs schema registry | Event streaming |

### Interview Questions

1. **Design the API communication strategy for an auction platform with 10 services.**
2. **Your BidService needs to notify 5 other services when a bid is placed. What communication pattern do you use?**
3. **Compare client-side vs. server-side service discovery. Which fits better with Kubernetes?**
4. **How do you version APIs in a microservices architecture without breaking consumers?**
5. **What is the difference between an event, a command, and a query in microservices communication?**

### Practical Exercises

1. Define the `BidPlaced` event schema in Protobuf and Avro. Evolve it (add field, remove field). Test backward compatibility.
2. Implement client-side service discovery: Build a .NET client that queries Consul for service instances and load-balances between them.
3. Implement an async request/response pattern using RabbitMQ: BidService sends a validation request to AuctionService and processes the response asynchronously.

---

## Chapter 4: Managing Transactions with Sagas

### Key Ideas

In a monolith, you wrap a business operation in a database transaction (ACID). In microservices, each service has its own database → **no distributed transactions** (they don't scale and kill availability).

**Solution: Sagas** — a sequence of local transactions, each updating a service's database and publishing an event/message to trigger the next step. If a step fails, **compensating transactions** undo the previous steps.

**Two saga coordination modes**:

**Choreography** (event-driven):
```
BidService           AuctionService        PaymentService
    |                     |                      |
    |── BidPlaced ──→     |                      |
    |                     |── BidValidated ──→   |
    |                     |                      |── PaymentReserved ──→
    |                     |── AuctionUpdated     |
```
- Each service listens for events and acts
- Simple for 2-3 step sagas
- Hard to understand with many steps; cyclic dependencies possible

**Orchestration** (central coordinator):
```
Orchestrator:
  1. Tell PaymentService: Reserve funds     → success
  2. Tell AuctionService: Place bid         → success
  3. Tell NotificationService: Send alert   → success
  
If step 2 fails:
  1. Tell PaymentService: Release funds (compensating transaction)
```
- Central orchestrator directs the workflow
- Easier to understand for complex flows
- Orchestrator can become a bottleneck/single point of failure

### Saga Design Considerations

**Compensating transactions**:
| Step | Action | Compensation |
|------|--------|-------------|
| 1 | Reserve payment | Release payment |
| 2 | Place bid | Cancel bid |
| 3 | Update auction highest bid | Revert to previous bid |
| 4 | Send notification | Send correction notification |

**Semantic lock** (countermeasure for isolation issues):
- While a saga is in progress, mark affected entities as "pending"
- Other operations check for the "pending" flag and either wait or reject
- Example: Auction has `status: "BidProcessing"` while a saga runs

**Anomalies in sagas** (no ACID isolation):
- **Lost updates**: Two sagas overwrite each other's changes
- **Dirty reads**: One saga reads data that another saga hasn't committed yet
- **Non-repeatable reads**: Different steps of a saga see different values

**Countermeasures**:
- Semantic lock (set a flag)
- Commutative updates (operations that work in any order)
- Pessimistic view (reorder saga steps to minimize risk)
- Reread value (verify data hasn't changed before committing)
- Version file (record operations for conflict resolution)

### Modern Backend Application: Auction Bid Saga

```csharp
public class PlaceBidSaga : MassTransitStateMachine<PlaceBidSagaState>
{
    public State ValidatingBid { get; private set; }
    public State ReservingPayment { get; private set; }
    public State UpdatingAuction { get; private set; }
    public State Completed { get; private set; }
    public State Failed { get; private set; }

    public PlaceBidSaga()
    {
        InstanceState(x => x.CurrentState);

        Initially(
            When(BidSubmitted)
                .Then(context => context.Saga.BidAmount = context.Message.Amount)
                .TransitionTo(ValidatingBid)
                .Send(new ValidateBid(...)));

        During(ValidatingBid,
            When(BidValidated)
                .TransitionTo(ReservingPayment)
                .Send(new ReservePayment(...)),
            When(BidValidationFailed)
                .TransitionTo(Failed));

        During(ReservingPayment,
            When(PaymentReserved)
                .TransitionTo(UpdatingAuction)
                .Send(new UpdateAuctionBid(...)),
            When(PaymentFailed)
                .TransitionTo(Failed));

        During(UpdatingAuction,
            When(AuctionUpdated)
                .TransitionTo(Completed)
                .Publish(new BidPlacedSuccessfully(...)),
            When(AuctionUpdateFailed)
                .Send(new ReleasePayment(...))
                .TransitionTo(Failed));
    }
}
```

### Interview Questions

1. **Design a saga for the "Buy It Now" flow in an auction platform: verify user, reserve payment, close auction, transfer ownership, notify participants.**
2. **Compare choreography vs. orchestration sagas. When would you use each?**
3. **How do you handle a scenario where the compensating transaction itself fails?**
4. **Explain the semantic lock pattern and how it prevents dirty reads in sagas.**
5. **Your saga has 7 steps and the 6th step fails. How do you roll back? What if step 3's compensation also fails?**

### Practical Exercises

1. Implement a choreography saga using MassTransit (RabbitMQ) in .NET: BidPlaced → PaymentReserved → AuctionUpdated → NotificationSent.
2. Implement the same saga as an orchestration using MassTransit StateMachine. Compare complexity, testability, and failure handling.
3. Simulate a failure at step 3 of a 5-step saga. Implement and test all compensating transactions.

### Common Mistakes

- Using distributed transactions (2PC) instead of sagas → kills availability and performance
- Forgetting to implement compensating transactions for every step
- Not handling the case where a compensating transaction fails (manual intervention queue)
- Choreography sagas with 7+ steps → impossible to understand; use orchestration

---

## Chapter 5: Designing Business Logic

### Key Ideas

Two patterns for organizing business logic in microservices:

**1. Transaction Script Pattern**:
- Procedural code: a method for each business operation
- Simple, good for CRUD-heavy services
- Gets messy as business logic grows complex

**2. Domain Model Pattern (DDD)**:
- Rich objects with behavior and state
- **Aggregates**: Cluster of domain objects with a root entity. The unit of consistency.
- **Aggregate rules**:
  - Reference other aggregates by ID, not by object reference
  - One transaction = one aggregate update
  - Eventual consistency between aggregates (use domain events)

### Aggregate Design Rules

| Rule | Why | Example |
|------|-----|---------|
| One aggregate per transaction | Avoid distributed locks | PlaceBid updates only the Bid aggregate |
| Reference by ID | Loose coupling between aggregates | Bid has `AuctionId` (not an Auction reference) |
| Eventual consistency between aggregates | No cross-aggregate transactions | BidPlaced event → AuctionService updates Auction aggregate |
| Small aggregates | Minimize contention, improve scalability | Bid is separate from Auction |

### Domain Events

Domain events are the mechanism for inter-aggregate and inter-service communication:

```
Bid aggregate (BidService):
  bid.Place(userId, amount)
    → validates amount > currentHighest
    → sets state to Placed
    → raises BidPlacedDomainEvent

BidPlacedDomainEvent published to message broker
  → AuctionService receives, updates Auction aggregate
  → NotificationService receives, sends "outbid" alert
```

### Interview Questions

1. **When would you use Transaction Script vs. Domain Model? Give examples from an auction system.**
2. **Explain the aggregate pattern. Why should one transaction only update one aggregate?**
3. **How do you maintain consistency between the Bid and Auction aggregates in separate services?**
4. **What is the Outbox Pattern and why is it essential for publishing domain events reliably?**
5. **Design the aggregate structure for an auction platform. Define the aggregates, their boundaries, and the events between them.**

### Practical Exercises

1. Implement the `Auction` aggregate with domain logic: `CreateAuction`, `UpdateReservePrice`, `CloseAuction`. Raise domain events for each state change.
2. Implement the Outbox Pattern: When `Bid.Place()` is called, save the bid AND the `BidPlaced` event in the same database transaction. A separate publisher reads from the outbox table.
3. Demonstrate eventual consistency: BidService places a bid, publishes event. AuctionService consumes event and updates the highest bid. Show that there's a small window where the two are inconsistent.

### Common Mistakes

- Anemic domain model: entities with only getters/setters, all logic in services → not DDD
- Large aggregates with many entities → contention, slow transactions
- Updating multiple aggregates in one transaction → breaks aggregate boundaries
- Publishing events outside the transaction → if the transaction rolls back, the event is already sent (use Outbox)

---

## Chapter 6: Developing Business Logic with Event Sourcing

### Key Ideas

**Event Sourcing**: Instead of storing the current state, store the sequence of events that led to the current state. The current state is derived by replaying events.

Traditional: `UPDATE auctions SET highest_bid = 500 WHERE id = 1`
Event Sourced: Store `BidPlaced(auctionId=1, amount=500, userId=42, timestamp=...)`

**Benefits**:
- Complete audit trail (every change is recorded)
- Temporal queries ("what was the state at 3:00 PM?")
- Event publishing is built-in (the events ARE the data)
- Debugging: replay events to reproduce bugs

**Challenges**:
- Querying is hard (can't `SELECT * FROM auctions WHERE price > 100`)
- Event schema evolution (events are stored forever)
- Eventual consistency (read models are derived, may lag behind)
- Learning curve for the team

### Event Sourcing + CQRS

Event sourcing is almost always paired with **CQRS (Command Query Responsibility Segregation)**:

```
Command Side (Write):              Query Side (Read):

User Action                        User Query
    ↓                                  ↓
Command Handler                    Read Model (Materialized View)
    ↓                                  ↑
Aggregate                         Event Handler
    ↓                                  ↑
Event Store ──── Events ──────→ Projection
  (append only)                  (builds read model)
```

- **Write side**: Processes commands, produces events, stores in event store
- **Read side**: Consumes events, builds optimized read models (different DB, different schema)
- Read models can be multiple: one for search (Elasticsearch), one for analytics (ClickHouse), one for API (PostgreSQL denormalized)

### Event Store Implementation

```
Event Store Table:
┌────────────┬───────────┬────────────┬─────────┬──────────────────┐
│ event_id   │ aggregate │ aggregate  │ version │ event_data       │
│            │ _type     │ _id        │         │ (JSON/Protobuf)  │
├────────────┼───────────┼────────────┼─────────┼──────────────────┤
│ evt-001    │ Auction   │ auction-1  │ 1       │ AuctionCreated{} │
│ evt-002    │ Auction   │ auction-1  │ 2       │ BidPlaced{}      │
│ evt-003    │ Auction   │ auction-1  │ 3       │ BidPlaced{}      │
│ evt-004    │ Auction   │ auction-1  │ 4       │ AuctionClosed{}  │
└────────────┴───────────┴────────────┴─────────┴──────────────────┘

To get current state: replay events 1-4 for auction-1
Optimistic concurrency: expected_version check on write
```

### Snapshots

Replaying thousands of events for every read is expensive. Solution: **Snapshots**.

```
Events: [1] [2] [3] ... [998] [999] [SNAPSHOT @ 999] [1000] [1001]

To rebuild state:
  Load snapshot at event 999 → Apply events 1000, 1001
  Instead of replaying all 1001 events
```

### Interview Questions

1. **Explain event sourcing with a real auction example. Walk through creating an auction, placing bids, and closing it.**
2. **Why is CQRS almost always used with event sourcing?**
3. **How do you handle event schema evolution when events are stored forever?**
4. **What are snapshots in event sourcing and when do you need them?**
5. **Design the event-sourced model for the BidService: define the commands, events, and read models.**

### Practical Exercises

1. Implement an event-sourced `Auction` aggregate in C#:
   - Commands: `CreateAuction`, `PlaceBid`, `CloseAuction`
   - Events: `AuctionCreated`, `BidPlaced`, `AuctionClosed`
   - State rebuilt by replaying events
2. Build a CQRS read model: Project `BidPlaced` events into a "current auction status" read model (auction ID, current highest bid, bid count, time remaining).
3. Implement event schema evolution: V1 `BidPlaced` has `{amount}`, V2 adds `{amount, currency}`. Write an upcaster that converts V1 events to V2 format during replay.

### Common Mistakes

- Not using CQRS with event sourcing → read queries are impossibly complex
- Storing too much data in events (full entity snapshots instead of deltas)
- Not planning for event schema evolution from day one
- Treating event store like a traditional database (querying it directly)
- Forgetting about snapshots → rebuilding state from millions of events is slow

### Connections

- **DDIA Ch 11**: Event sourcing is stream processing applied to business logic
- **DDIA Ch 12**: The event log is the source of truth; read models are derived views
- **Consistency**: Write side is strongly consistent (within one aggregate); read side is eventually consistent

---

## Chapter 7: Implementing Queries in a Microservice Architecture

### Key Ideas

In microservices, data is spread across services. Queries that join data from multiple services are challenging.

**Two patterns for cross-service queries**:

**1. API Composition**:
```
Client → API Composer → AuctionService (get auction details)
                      → BidService (get bids)
                      → UserService (get bidder names)
         ← Combine results → Return to client
```
- Simple, synchronous
- Works well for simple joins
- Problems: latency (sequential calls), availability (any service down = query fails), data consistency (no transaction across services)

**2. CQRS (Command Query Responsibility Segregation)**:
- Maintain a pre-built read model that joins data from multiple services
- Read model updated by consuming events from each service
- Queries hit the read model directly — fast, no cross-service calls

```
AuctionService ──(AuctionCreated)──→ ┌──────────────┐
BidService ──────(BidPlaced)───────→ │  Read Model   │ ← Client queries
UserService ─────(UserUpdated)─────→ │  (Denormalized │
                                     │   view in DB)  │
                                     └──────────────┘
```

### When to Use Which

| Aspect | API Composition | CQRS |
|--------|----------------|------|
| Complexity | Low | High |
| Latency | Higher (fan-out) | Low (pre-built) |
| Consistency | Real-time | Eventually consistent |
| Data volume | Low-to-medium | Any |
| Development effort | Low | High (event consumers, projections) |
| Operational effort | Low | High (event store, read model rebuild) |

### Interview Questions

1. **Design a "Search Auctions" feature that needs data from AuctionService, BidService, and UserService. Compare API Composition vs. CQRS.**
2. **How do you handle inconsistency in a CQRS read model? What if the projection is behind?**
3. **Your auction detail page needs 7 API calls to different services. How do you optimize this?**
4. **How do you rebuild a CQRS read model from scratch without downtime?**
5. **When is API Composition sufficient and CQRS is overkill?**

### Practical Exercises

1. Implement API Composition in the API Gateway: Fetch auction details (AuctionService) + current bids (BidService) + seller info (UserService) and combine into a single response.
2. Implement a CQRS projection: Consume `AuctionCreated`, `BidPlaced`, and `AuctionClosed` events to maintain a denormalized "auction detail" view in a read-optimized database.
3. Compare performance: Load test the API Composition approach vs. the CQRS approach for 1000 concurrent auction detail requests.

---

## Chapter 8: External API Patterns

### Key Ideas

External clients (web, mobile, third-party) need a unified, stable API to interact with internal microservices.

**API Gateway pattern**:
- Single entry point for all external requests
- Responsibilities: routing, authentication, rate limiting, response aggregation, protocol translation
- Implements the **Backend for Frontend (BFF)** pattern: different gateways for different clients

```
Mobile Client ──→ [Mobile BFF] ──→ Internal Services
Web Client ────→ [Web BFF]    ──→ Internal Services  
3rd Party ─────→ [Public API] ──→ Internal Services
```

**Why different BFFs?**
- Mobile needs smaller payloads, fewer requests (battery, bandwidth)
- Web can handle larger payloads, more interactions
- Public API needs versioning, strict rate limiting, documentation

**API Gateway responsibilities**:

| Responsibility | Example |
|---------------|---------|
| Request routing | `/api/auctions/*` → AuctionService |
| Authentication | Validate JWT token |
| Rate limiting | 100 requests/min per API key |
| Response aggregation | Combine auction + bids + seller into one response |
| Protocol translation | REST → gRPC for internal services |
| Caching | Cache auction list for 30s |
| Request/response transformation | Add/remove fields for different client versions |

### Interview Questions

1. **Design the API Gateway architecture for an auction platform with web, mobile, and third-party API clients.**
2. **What is the Backend for Frontend pattern and when should you use it?**
3. **How do you prevent the API Gateway from becoming a monolithic bottleneck?**
4. **Compare building a custom API Gateway vs. using Kong/YARP/Ocelot.**
5. **How do you handle API versioning for public APIs?**

### Common Mistakes

- Putting business logic in the API Gateway → becomes a monolith
- Single API Gateway for all client types → bloated, slow releases
- Not rate limiting → one client can take down the entire system
- Exposing internal service APIs directly → tight coupling to internal structure

---

## Chapter 9: Testing Microservices

### Key Ideas

Testing a distributed system is fundamentally harder than testing a monolith. The test strategy must balance **confidence** with **speed and maintainability**.

**Test Pyramid for Microservices**:
```
            /   End-to-End   \       ← Very few, slow, costly
           / Component Tests  \      ← Test one service in isolation  
          /  Integration Tests \     ← Service + real dependencies
         / Contract Tests       \    ← API compatibility
        /   Unit Tests           \   ← Many, fast, no dependencies
```

**Consumer-Driven Contract Testing**:

The most important testing innovation for microservices:

```
BidService (Consumer)                    AuctionService (Provider)
  |                                        |
  |── Defines contract:                    |
  |   "I need GET /auctions/{id}           |
  |    to return {id, title, status}"      |
  |                                        |
  |── Contract stored in Pact Broker ──→   |
  |                                        |── Runs provider tests
  |                                        |── Verifies it still satisfies
  |                                        |   all consumer contracts
```

- Consumers define what they need (not everything the provider offers)
- Provider tests verify compliance with ALL consumer contracts
- Catches breaking changes BEFORE deployment
- Decouples consumer and provider test suites

**Component testing**: Test one service in isolation by replacing external dependencies with stubs/mocks.

```
[Test Environment]
┌─────────────────────────────────────────┐
│  BidService                             │
│  ├── Real code                          │
│  ├── Real database (in-memory/Docker)   │
│  ├── Stubbed AuctionService (WireMock)  │
│  └── Stubbed PaymentService (WireMock)  │
└─────────────────────────────────────────┘
```

### Interview Questions

1. **Design the testing strategy for the BidService. What tests do you write at each level?**
2. **Explain consumer-driven contract testing. How does it prevent breaking changes?**
3. **Your E2E test suite takes 1 hour and fails 40% of the time. How do you fix this?**
4. **How do you test a saga with 5 steps across 5 services?**
5. **What is the difference between component testing and integration testing in microservices?**

### Practical Exercises

1. Write consumer-driven contract tests for BidService → AuctionService using Pact.
2. Set up component tests for BidService: real database in Docker, stubbed external services with WireMock.
3. Write integration tests for the PlaceBid saga: test the happy path and two different failure scenarios.

---

## Chapter 10: Developing Production-Ready Services

### Key Ideas

A microservice isn't production-ready just because it works. It needs:

**Security**:
- Authentication at the edge (API Gateway)
- Authorization within each service (role-based)
- Encrypt data in transit (TLS) and at rest

**Configurability**:
- Externalize configuration (environment variables, config server)
- Feature flags for gradual rollout
- Different configs for dev/staging/production

**Observability** (three pillars):
1. **Health check API**: `/health` endpoint for load balancers and Kubernetes
2. **Log aggregation**: Structured JSON logs, centralized collection
3. **Distributed tracing**: OpenTelemetry for cross-service request tracing
4. **Application metrics**: Prometheus metrics for RED method
5. **Exception tracking**: Sentry, Application Insights

**Health Check Pattern**:
```
GET /health/live   → Am I running? (Kubernetes liveness)
GET /health/ready  → Can I serve traffic? (Kubernetes readiness)
GET /health/startup → Am I initialized? (Kubernetes startup probe)

Readiness checks:
- Database connection: OK
- Message broker connection: OK
- Disk space: OK
- Dependent service (optional): OK/Degraded
```

### Interview Questions

1. **What does "production-ready" mean for a microservice? List the requirements.**
2. **Design the health check strategy for the BidService.**
3. **How do you implement distributed tracing across 10 microservices?**
4. **What security measures should every microservice have?**
5. **How do you manage configuration across 15 microservices in 3 environments?**

---

## Chapter 11: Deploying Microservices

### Key Ideas

Deployment strategy determines how fast and safely you can ship changes.

**Deployment patterns**:

| Pattern | How It Works | Risk | Speed |
|---------|-------------|------|-------|
| Single machine, multiple services | All services on one box | High (blast radius) | Fast |
| Service per VM | One service per VM | Medium | Slow (VM boot) |
| Service per container | One service per Docker container | Low | Fast |
| Serverless | Cloud runs your function | Lowest | Fastest |

**Kubernetes deployment strategies**:
- **Rolling update**: Default. Replace pods one by one.
- **Blue/green**: Run old and new side by side, switch traffic
- **Canary**: Route small % to new version, expand if healthy

**Service mesh** (Istio/Linkerd) provides:
- Traffic management (canary, A/B testing)
- Mutual TLS between services
- Observability (metrics, tracing)
- Resilience (retries, circuit breaking)

### The Deployment Pipeline

```
Developer commits → Build → Unit Tests → Container Build → Push Image
  → Deploy to Dev → Integration Tests → Deploy to Staging
  → Contract Tests → Performance Tests 
  → Deploy to Production (Canary 5% → 25% → 50% → 100%)
  → Monitor error rate → Auto-rollback if error rate > 1%
```

### Interview Questions

1. **Design the deployment architecture for a 12-service auction platform on Kubernetes.**
2. **Compare rolling update vs. canary deployment for a critical payment service.**
3. **How does a service mesh like Istio simplify microservice deployment?**
4. **Your deployment caused a 5% error rate increase. How do you detect and respond?**
5. **What is GitOps and how does it apply to microservices deployment?**

---

## Chapter 12: Refactoring to Microservices

### Key Ideas

This chapter focuses on the practical journey from monolith to microservices.

**The Strangler Fig strategy (detailed)**:

```
Phase 1: New features as services
  Monolith ←→ New AuctionSearchService (new feature, separate service)

Phase 2: Extract existing features
  Monolith (shrinking) ←→ AuctionService (extracted)
                       ←→ BidService (extracted)
                       ←→ SearchService (running)

Phase 3: Monolith eliminated
  AuctionService ←→ BidService ←→ SearchService ←→ ...
```

**Extraction priorities**:
1. Services with high change frequency (extract to deploy independently)
2. Services with different scaling requirements (extract to scale independently)
3. Services with technology constraints (need different stack)
4. Services with clear domain boundaries (lowest extraction risk)

**Anti-Corruption Layer (ACL)**: When extracting a service, add an adapter between the new service and the monolith. The ACL translates between the old model and the new model, preventing the monolith's model from "corrupting" the new service's clean design.

### Interview Questions

1. **You have a 500K-line monolith. Plan the migration to microservices over 12 months.**
2. **Which service would you extract first from the auction monolith? Why?**
3. **Explain the Anti-Corruption Layer pattern. When is it necessary?**
4. **How do you handle the shared database during migration?**
5. **What metrics indicate the migration is succeeding vs. creating more problems?**

### Practical Exercises

1. Identify extraction candidates in a monolith: Look at change frequency (git log), module dependencies (static analysis), and scaling requirements.
2. Extract a BidService from a monolith: Set up the Strangler Fig proxy, migrate the database tables, redirect traffic, and decommission the old code.
3. Implement an Anti-Corruption Layer: The new BidService speaks a clean API, while the ACL translates to/from the monolith's legacy data format.

---

## Chapter 13: Microservices Refactoring Patterns

### Key Ideas

This chapter provides additional patterns for the extraction process.

**Key patterns**:

| Pattern | Purpose |
|---------|---------|
| Strangler Fig | Incrementally replace monolith functionality |
| Anti-Corruption Layer | Protect new service from old model |
| Database refactoring | Separate shared tables into per-service databases |
| Event publishing | Use CDC or outbox to publish domain events from the monolith |
| Feature toggle | Deploy new service code, toggle traffic between old and new |

**Database refactoring strategies**:
1. **Shared database (temporary)**: Both monolith and new service access the same DB. Bridge pattern during migration.
2. **Database view**: Create a view in the monolith's DB for the new service. Decouples schema.
3. **Change Data Capture (CDC)**: Capture changes from monolith's DB, stream to new service's DB.
4. **Synchronize via events**: Monolith publishes events, new service builds its own data store.

### Interview Questions

1. **How do you handle data synchronization during the transition period when both monolith and microservice need the same data?**
2. **Explain CDC (Change Data Capture) and how it helps during monolith decomposition.**
3. **When would you use a feature toggle during migration vs. a Strangler Fig?**
4. **How do you test during the migration period when some features are in the monolith and some are in microservices?**
5. **What is the "parallel run" pattern and when would you use it for high-risk migrations?**

---

## Summary: Key Takeaways from Microservices Patterns

| Chapter | One-Line Takeaway |
|---------|-------------------|
| 1 | Microservices solve organizational and deployment problems, not technical ones |
| 2 | Decompose by business capability or DDD subdomain — not by data entity or technical layer |
| 3 | Choose communication style (sync/async) based on coupling requirements |
| 4 | Sagas replace distributed transactions; choose orchestration for complex flows, choreography for simple ones |
| 5 | Use DDD aggregates for business logic; one transaction per aggregate |
| 6 | Event sourcing gives perfect audit trails but requires CQRS for queries |
| 7 | CQRS for complex queries across services; API composition for simple ones |
| 8 | API Gateway + BFF pattern keeps clients insulated from internal service structure |
| 9 | Consumer-driven contract tests are the most valuable test type for microservices |
| 10 | Production-ready = secure + configurable + observable |
| 11 | Containers + Kubernetes + canary deployments = safe, fast releases |
| 12-13 | Strangler Fig + ACL + database refactoring = safe monolith-to-microservice migration |
