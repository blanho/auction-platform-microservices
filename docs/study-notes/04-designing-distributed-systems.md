# Designing Distributed Systems — Brendan Burns

> A patterns-based approach to designing distributed systems using containers and orchestrators (Kubernetes).

---

## Part I: Single-Node Patterns

These patterns run multiple containers on a single node (pod in Kubernetes), working together as a unit.

---

## Chapter 1: Introduction

### Key Ideas

Distributed system design can be formalized into **reusable patterns**, just like the Gang of Four did for object-oriented design. Containers are the building blocks:

- **Containers** are the "objects" of distributed systems
- **Container images** are the "classes" — reusable, versioned, immutable
- **Pods** (Kubernetes) are groups of containers that share resources (network, storage)
- Patterns are composed from these primitives

**Why patterns matter**:
- Codify best practices so teams don't reinvent the wheel
- Create a shared vocabulary for distributed system design
- Enable reuse: build once, deploy everywhere
- Reduce errors: proven patterns have known failure modes

---

## Chapter 2: The Sidecar Pattern

### Key Ideas

A **sidecar** is an additional container deployed alongside the main application container in the same pod. It extends or enhances the main container's functionality without modifying its code.

```
┌──────────────────────── Pod ────────────────────────┐
│                                                      │
│  ┌──────────────┐      ┌──────────────────────┐    │
│  │  Application  │ ←──→ │    Sidecar Container   │   │
│  │  Container    │      │  (log shipper, proxy,  │   │
│  │  (main logic) │      │   config reloader...)  │   │
│  └──────────────┘      └──────────────────────┘    │
│                                                      │
│  Shared: network (localhost), volumes, lifecycle     │
└──────────────────────────────────────────────────────┘
```

**Key principle**: The sidecar and main container share the same lifecycle, network namespace, and storage volumes — but are independently deployable and upgradeable.

### Common Sidecar Uses

| Use Case | Sidecar Does | Example |
|----------|-------------|---------|
| Log collection | Ships logs to centralized system | Fluentd, Filebeat |
| Configuration | Watches config changes, reloads app | Consul Template |
| Proxy / mTLS | Handles TLS termination, service mesh | Envoy (Istio), Linkerd |
| Monitoring | Exposes metrics endpoint | Prometheus exporter |
| Data sync | Syncs files from remote source | Git-sync |
| Security | Handles auth, certificate rotation | Vault agent |

### Modern Backend Application

In the auction platform on Kubernetes:
```yaml
apiVersion: v1
kind: Pod
spec:
  containers:
  - name: bid-service
    image: auction/bid-service:latest
    ports:
    - containerPort: 8080
  - name: envoy-proxy          # Sidecar: service mesh proxy
    image: envoyproxy/envoy:latest
  - name: fluentd               # Sidecar: log shipping
    image: fluentd:latest
    volumeMounts:
    - name: logs
      mountPath: /var/log
  volumes:
  - name: logs
    emptyDir: {}
```

### Interview Questions

1. **Explain the sidecar pattern. When would you use it instead of a library?**
2. **How does Istio's Envoy sidecar provide mTLS without changing application code?**
3. **What are the resource implications of running sidecars alongside every service pod?**
4. **Compare sidecar vs. library approach for cross-cutting concerns (logging, tracing, auth).**
5. **Design a sidecar for dynamic configuration reloading in a microservices platform.**

### Practical Exercises

1. Deploy a sidecar container that ships application logs to Elasticsearch. The main container writes logs to a shared volume; the sidecar reads and forwards them.
2. Implement a sidecar that watches a ConfigMap and reloads the main application's configuration without restarting the pod.
3. Set up Istio on a Kubernetes cluster and observe the Envoy sidecar handling mTLS between two services.

### Common Mistakes

- Sidecar becomes too heavy → pod resource usage doubles
- Sidecar has different lifecycle requirements than main container → deployment complications
- Too many sidecars per pod → resource contention, complex debugging
- Using sidecar for logic that should be in the application (business logic in the proxy)

### Connections

- **Microservices**: Service mesh (Istio) uses sidecars to provide resilience, security, and observability without application changes
- **SRE**: Sidecar-based monitoring/logging is how SRE teams instrument services they don't own

---

## Chapter 3: Ambassadors

### Key Ideas

An **ambassador** is a sidecar that proxies communication from the main container to the outside world. It handles the complexity of connecting to external services.

```
┌────────────── Pod ──────────────┐
│                                  │
│  Application ──→ Ambassador ──→ External Service
│  (connects to   (handles sharding,  (database cluster,
│   localhost)     retries, auth)       API, cache)
│                                  │
└──────────────────────────────────┘
```

**Key principle**: The application connects to `localhost` and the ambassador handles all the complexity of reaching external services (sharding logic, retries, circuit breaking, authentication).

### Ambassador Use Cases

| Use Case | Ambassador Handles | Example |
|----------|-------------------|---------|
| Database sharding | Route queries to correct shard | twemproxy (Redis), pgBouncer |
| Service discovery | Resolve service names to IPs | Consul Connect |
| Circuit breaking | Retry, timeout, fail-fast | Envoy proxy |
| Protocol bridging | Translate between protocols | gRPC → REST adapter |
| Credential injection | Add auth headers/tokens | Vault agent |
| Rate limiting | Client-side throttling | Custom rate limiter |

### Modern Backend Application

In the auction platform:
```
BidService connects to Redis on localhost:6379
  → Ambassador container (twemproxy) on localhost:6379
    → Routes to Redis Cluster (shard 1, shard 2, shard 3)
    
BidService doesn't know about Redis sharding.
Ambassador handles consistent hashing, connection pooling, failover.
```

### Interview Questions

1. **How does the ambassador pattern differ from the sidecar pattern?**
2. **Design an ambassador for connecting to a sharded database cluster.**
3. **When would you use an ambassador instead of a client library?**
4. **How does an ambassador simplify testing? (Hint: swap the ambassador for a mock.)**
5. **Design an ambassador that adds authentication headers to all outgoing requests to a third-party API.**

### Common Mistakes

- Ambassador adds latency to every request → must be lightweight
- Ambassador failure takes down the main application → needs health checks
- Using ambassador for simple single-instance connections (overhead not justified)

---

## Chapter 4: Adapters

### Key Ideas

An **adapter** is a sidecar that presents a uniform interface to the outside world, regardless of the main container's interface.

```
External System ──→ Adapter ──→ Application
(expects standard    (translates)   (has custom format)
 format)
```

**Key principle**: Adapters normalize heterogeneous applications to present a consistent external interface.

### Adapter Use Cases

| Use Case | Adapter Converts | Example |
|----------|-----------------|---------|
| Monitoring | App-specific metrics → Prometheus format | Redis exporter, MySQL exporter |
| Logging | App-specific logs → structured JSON | Log format adapter |
| Health check | App-specific health → standard HTTP | Health check adapter |
| API versioning | Old API → new API contract | API adapter |

### The Most Common Adapter: Prometheus Exporter

```
┌────────────── Pod ──────────────────┐
│                                      │
│  Redis Server                        │
│  (has its own metrics format)        │
│       ↓                              │
│  Redis Exporter (Adapter)            │
│  (converts Redis INFO → Prometheus   │
│   /metrics endpoint)                 │
│       ↓                              │
│  Prometheus scrapes /metrics         │
└──────────────────────────────────────┘
```

### Interview Questions

1. **Explain the adapter pattern. How does it differ from ambassador and sidecar?**
2. **You have 10 different services with 10 different metrics formats. How do you standardize monitoring?**
3. **Design an adapter that converts legacy SOAP service responses to REST/JSON.**
4. **When would you use an adapter container vs. an in-application adapter library?**
5. **How does the adapter pattern support heterogeneous technology in a microservices platform?**

### Summary: Single-Node Pattern Comparison

| Pattern | Direction | Purpose | Example |
|---------|-----------|---------|---------|
| Sidecar | Augments app | Extend functionality | Log shipper, config sync |
| Ambassador | App → External | Proxy outgoing connections | Database proxy, auth injector |
| Adapter | External → App | Normalize incoming interface | Prometheus exporter, API translator |

---

## Part II: Serving Patterns

Multi-node patterns for serving real-time traffic.

---

## Chapter 5: Replicated Load-Balanced Services

### Key Ideas

The simplest distributed pattern: run multiple identical copies of a service behind a load balancer.

```
           ┌──→ [Instance 1]
Client ──→ [Load Balancer] ──→ [Instance 2]
           └──→ [Instance 3]
```

**Stateless services** (ideal): Any instance can handle any request. Scale by adding instances.

**Session affinity** (sticky sessions): Route same user to same instance. Required when instances hold session state. Avoid if possible — use external state store (Redis) instead.

**Readiness probes**: Don't send traffic to an instance until it's ready (warmed up, connected to DB, loaded caches).

### Scaling Strategies

| Strategy | Trigger | Tool |
|----------|---------|------|
| Manual | Operator decision | `kubectl scale` |
| Reactive (HPA) | CPU, memory, or custom metric threshold | Kubernetes HPA |
| Predictive | Time-of-day patterns, ML prediction | KEDA, custom controller |
| Event-driven | Queue depth, message lag | KEDA + Kafka/RabbitMQ |

### Modern Backend Application

```
BidService (stateless, replicated):
- Kubernetes Deployment: 3 replicas (min), 20 replicas (max)
- HPA: Scale on requests/second (target: 500 req/s per pod)
- Pod Disruption Budget: min 2 pods available during rolling updates
- Readiness probe: /health/ready (checks DB + message broker connection)

Load balancing: Kubernetes Service (round-robin) 
  → Ingress Controller (path-based routing)
    → /api/bids → BidService
    → /api/auctions → AuctionService
```

### Interview Questions

1. **Design the scaling strategy for BidService during a high-profile auction with 100x normal traffic.**
2. **Why are stateless services easier to scale? How do you make a stateful service stateless?**
3. **What is the cold-start problem and how does it affect auto-scaling?**
4. **Compare reactive (HPA) vs. event-driven (KEDA) autoscaling. When would you use each?**
5. **How do you handle graceful shutdown when scaling down? (Hint: preStop hooks, connection draining.)**

---

## Chapter 6: Sharded Services

### Key Ideas

When a service needs to process requests that relate to specific data, **sharding** routes each request to the correct instance based on a shard key.

```
           ┌── shard(key) ──→ [Instance A-F]
Client ──→ [Shard Router] ──→ [Instance G-M]
           └── shard(key) ──→ [Instance N-Z]
```

Unlike replicated services where any instance handles any request, sharded services route specific requests to specific instances.

### Sharding Strategies

| Strategy | How | Pro | Con |
|----------|-----|-----|-----|
| Hash-based | `hash(key) % N` | Uniform distribution | Rebalancing moves ~all data |
| Consistent hashing | Hash ring with virtual nodes | Minimal rebalancing | More complex |
| Range-based | Key ranges assigned to shards | Efficient range queries | Risk of hot spots |
| Lookup table | External map of key → shard | Flexible | Lookup overhead |

### Sharded Cache Example

```
┌─────────────────────────────────────┐
│         Shard-Aware Proxy            │
│  hash("auction-123") → shard 2      │
└───────┬───────────┬─────────────────┘
        ↓           ↓           ↓
  [Redis Shard 0] [Redis Shard 1] [Redis Shard 2]
  keys: A-F        keys: G-M       keys: N-Z
```

### Hot Shard Mitigation

- **Replica reads**: Hot shard gets read replicas
- **Key splitting**: Add random suffix to hot keys (`auction-123-0`, `auction-123-1`)
- **Dedicated shard**: Move hot keys to a dedicated, more powerful shard
- **Caching layer**: Cache hot keys in front of the shard

### Interview Questions

1. **Design a sharded caching layer for the auction platform. How do you handle hot auctions?**
2. **Compare hash-based vs. consistent hashing for sharding. Why is consistent hashing better for dynamic environments?**
3. **How do you rebalance shards when adding or removing nodes?**
4. **A celebrity auction creates a hot shard. How do you mitigate this without affecting other auctions?**
5. **Design a sharded message processing system where all events for one auction go to the same processor.**

---

## Chapter 7: Scatter/Gather

### Key Ideas

**Scatter/Gather**: Send a request to all nodes (scatter), wait for all responses, combine results (gather).

```
Client Request
     ↓
  [Root Node]
   /    |    \
  ↓     ↓     ↓
[N1]  [N2]  [N3]    ← Scatter (parallel requests)
  \     |    /
   ↓    ↓   ↓
  [Root Node]         ← Gather (merge results)
     ↓
Combined Response
```

**Use cases**:
- Distributed search (query all shards, merge results)
- Parallel computation (split work, combine answers)
- Fan-out queries (ask all services for their data)

### Trade-offs

| Aspect | Implication |
|--------|------------|
| Latency | Bounded by the slowest node (tail latency problem) |
| Availability | Fails if any node fails (unless partial results acceptable) |
| Throughput | Limited by the root's ability to merge |
| Straggler mitigation | Send redundant requests, take first response |

**Straggler mitigation**: One slow node slows the entire request. Solutions:
- Set a timeout; return partial results if some nodes are slow
- Send redundant requests to multiple nodes; use the first response
- Hedge requests: wait for most responses, then send backup requests for the stragglers

### Modern Backend Application

Search in the auction platform (Elasticsearch):
```
Search Query: "vintage watch"
     ↓
  [Coordinator Node]
   /      |      \
  ↓       ↓       ↓
[Shard 0] [Shard 1] [Shard 2]   ← Each shard searches its data
  \       |       /
   ↓      ↓      ↓
  [Coordinator Node]              ← Merges results, sorts by relevance
     ↓
  Top 10 results
```

### Interview Questions

1. **Design a distributed search system for the auction platform using scatter/gather.**
2. **How do you handle the straggler problem in scatter/gather?**
3. **Your scatter/gather search has p99 latency of 5 seconds due to one slow shard. How do you fix it?**
4. **Compare scatter/gather vs. a single index for search. When is each appropriate?**
5. **How do you return partial results gracefully when some shards are unavailable?**

### Practical Exercises

1. Implement scatter/gather in C#: Query 3 "shards" (simulated HTTP endpoints) in parallel, merge sorted results, return top 10. Handle one shard timing out.
2. Implement straggler mitigation: Send redundant requests after 100ms if the first request hasn't returned. Show how p99 latency improves.
3. Build a scatter/gather API composition endpoint: Fan out to AuctionService + BidService + UserService in parallel, combine into a single auction detail response.

### Coding Example: Scatter/Gather

```csharp
public class ScatterGather<TResult>
{
    private readonly IReadOnlyList<Func<CancellationToken, Task<TResult>>> _workers;
    private readonly TimeSpan _timeout;

    public ScatterGather(
        IEnumerable<Func<CancellationToken, Task<TResult>>> workers,
        TimeSpan timeout)
    {
        _workers = workers.ToList();
        _timeout = timeout;
    }

    public async Task<IReadOnlyList<TResult>> ExecuteAsync()
    {
        using var cts = new CancellationTokenSource(_timeout);
        var tasks = _workers.Select(w => ExecuteWithFallback(w, cts.Token));
        var results = await Task.WhenAll(tasks);
        return results.Where(r => r is not null).ToList()!;
    }

    private async Task<TResult?> ExecuteWithFallback(
        Func<CancellationToken, Task<TResult>> worker,
        CancellationToken cancellationToken)
    {
        try
        {
            return await worker(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return default;
        }
        catch (Exception)
        {
            return default;
        }
    }
}
```

---

## Part III: Batch Computational Patterns

Patterns for processing large volumes of data.

---

## Chapter 8: Work Queue Systems

### Key Ideas

**Work Queue**: A system that distributes units of work across multiple workers.

```
┌──────────┐     ┌───────────┐     ┌──────────────┐
│  Source   │ ──→ │   Queue   │ ──→ │  Worker Pool  │
│ (items)  │     │ (buffer)  │     │  (parallel)   │
└──────────┘     └───────────┘     └──────────────┘
                                    ├── Worker 1 → process
                                    ├── Worker 2 → process
                                    └── Worker 3 → process
```

**Key characteristics**:
- Work items are independent (no ordering dependency)
- Workers are identical (any worker can process any item)
- Queue provides buffering (absorbs bursts)
- Workers can be scaled independently based on queue depth

**Patterns for work items**:
- **Container per work item**: Spin up a new container for each item. Maximum isolation. High overhead.
- **Multi-worker container**: A pool of workers in a single container. Shared resources. Lower overhead.

### Modern Backend Application

In the auction platform:
```
Image Processing Work Queue:
  Seller uploads auction images
    → Image Upload Event → Queue (RabbitMQ)
      → Worker 1: Generate thumbnail
      → Worker 2: Generate medium size
      → Worker 3: Run content moderation (AI)
      → Worker 4: Optimize for web
    → All results stored in StorageService
    → Event: ImagesProcessed → AuctionService updates listing

Scaling: KEDA scales workers based on queue depth
  Queue depth > 100 → scale up to 10 workers
  Queue depth < 10 → scale down to 2 workers
```

### Interview Questions

1. **Design a work queue system for processing auction images (resize, optimize, moderate).**
2. **How do you handle a poison message (a work item that always fails)?**
3. **Compare work queue vs. scatter/gather for parallel processing.**
4. **How do you ensure exactly-once processing for work queue items?**
5. **Design auto-scaling for a work queue based on queue depth and processing latency.**

---

## Chapter 9: Event-Driven Batch Processing

### Key Ideas

**Event-driven batch processing** connects multiple stages into a pipeline, where each stage processes events and produces output for the next stage.

```
Source → [Stage 1] → [Stage 2] → [Stage 3] → Sink
         (filter)    (transform)   (aggregate)
```

**Copier pattern**: One event triggers multiple parallel pipelines
```
              ┌──→ [Pipeline A]
Event → [Copier] ──→ [Pipeline B]
              └──→ [Pipeline C]
```

**Filter pattern**: Filter events based on criteria
```
Events → [Filter: price > 1000] → Expensive Bid Pipeline
```

**Splitter pattern**: Route events to different pipelines based on content
```
         ┌── category=art ──→ [Art Pipeline]
Events → [Splitter]
         └── category=electronics ──→ [Electronics Pipeline]
```

### Modern Backend Application

Auction analytics pipeline:
```
BidPlaced events
  → [Filter: amount > $1000]
    → [Enricher: add auction details, user details]
      → [Splitter: by category]
        → [Art aggregator: compute art market trends]
        → [Electronics aggregator: compute electronics trends]
      → [Sink: Write to ClickHouse for dashboards]
```

### Interview Questions

1. **Design an event-driven pipeline for real-time auction analytics.**
2. **How do you handle backpressure when a downstream stage is slower than upstream?**
3. **Compare event-driven batch processing with traditional MapReduce.**
4. **How do you ensure ordering guarantees in a multi-stage event pipeline?**
5. **Design a pipeline that detects fraudulent bidding patterns in real-time.**

---

## Chapter 10: Coordinated Batch Processing

### Key Ideas

Some batch processing requires **coordination** between workers — they can't work completely independently.

**Join pattern**: Wait for multiple inputs before producing output
```
Stream A ──→ ┌──────┐
             │ Join  │ ──→ Combined output
Stream B ──→ └──────┘

Wait for matching records from both streams before proceeding.
```

**Reduce pattern (Merge)**: Combine outputs from multiple workers into a single result
```
Worker 1 ──→ ┌──────────┐
Worker 2 ──→ │  Reduce   │ ──→ Final result
Worker 3 ──→ └──────────┘
```

**MapReduce as composition of patterns**:
```
Source → [Map Workers] → [Shuffle/Sort] → [Reduce Workers] → Sink
         (scatter)      (coordinated     (gather/merge)
                         redistribute)
```

### Modern Backend Application

End-of-day auction settlement:
```
Step 1 (Map): For each closed auction, compute winner and final price
  → [Worker pool] processes auctions in parallel

Step 2 (Join): Match auction results with payment info
  → Join auction_results stream with payment_methods stream

Step 3 (Reduce): Aggregate by seller
  → Compute total payout per seller
  → Generate settlement records

Step 4 (Execute): Process payments
  → Work queue: one payment per work item
```

### Interview Questions

1. **Design the end-of-day settlement batch job for an auction platform.**
2. **How do you implement a join between two data streams in a distributed system?**
3. **Compare coordinated batch processing with stream processing for daily aggregations.**
4. **How do you handle failures mid-batch in a coordinated pipeline?**
5. **Design a batch job that computes "similar items" recommendations using co-occurrence analysis.**

---

## Part IV: Advanced Patterns

---

## Chapter 11: Functions as a Service (FaaS)

### Key Ideas

**FaaS (Serverless)**: Execute individual functions in response to events. No server management.

```
Event ──→ [Cloud Function] ──→ Output
          (auto-scaled,
           billed per execution,
           stateless)
```

**When to use FaaS**:
- Event-driven processing (webhook handlers, file processing)
- Low/variable traffic (pay only for what you use)
- Simple, stateless transformations
- Glue code between services

**When NOT to use FaaS**:
- Long-running processes (timeout limits: 5-15 minutes)
- Latency-sensitive (cold start: 100ms - 10s)
- Stateful processing
- High, consistent traffic (dedicated containers are cheaper)

### Modern Backend Application

In the auction platform:
```
FaaS Use Cases:
- Image thumbnail generation (triggered by S3 upload)
- Bid notification email (triggered by BidPlaced event)
- Auction closing webhook (triggered by timer/schedule)
- PDF invoice generation (triggered by PaymentCompleted event)

NOT FaaS:
- BidService (high throughput, latency-sensitive)
- SearchService (needs warm caches, long-lived connections)
- Real-time WebSocket connections (long-lived)
```

### Interview Questions

1. **When would you use FaaS vs. a containerized microservice for the auction platform?**
2. **What is the cold start problem? How do you mitigate it?**
3. **Design an event-driven image processing pipeline using FaaS.**
4. **How does FaaS pricing compare to always-on containers for different traffic patterns?**
5. **What are the testing challenges with FaaS and how do you address them?**

---

## Chapter 12: Ownership Election

### Key Ideas

In distributed systems, sometimes exactly one node must be the "owner" or "leader" for a piece of work. This requires a distributed consensus mechanism.

**Use cases**:
- Leader election for a database (exactly one writer)
- Ownership of a work shard (exactly one processor per shard)
- Singleton services (exactly one instance performing a task)
- Distributed cron (exactly one node runs the scheduled job)

**Implementation approaches**:
1. **External lock service** (ZooKeeper, etcd, Consul): Services compete for a lock; winner is the leader
2. **Kubernetes lease**: Built-in leader election primitive
3. **Database-based**: `SELECT FOR UPDATE` with a lock row
4. **Raft/Paxos**: Built-in to some services

**Fencing tokens**: When a leader's lease expires while it's still processing (GC pause, network delay), a new leader is elected. The old leader might still think it's the leader → **split brain**. Fencing tokens prevent this:

```
Leader A gets lock with fence token #33
Leader A pauses (GC)
Leader B gets lock with fence token #34
Leader A resumes, tries to write with token #33
Storage rejects token #33 (< current #34) → stale leader detected
```

### Modern Backend Application

In the auction platform:
```
Auction Closing Job:
  - Must run EXACTLY once when auction time expires
  - Multiple instances of JobService running
  - One instance elected as leader via Redis distributed lock
  - Leader runs the closing logic
  - If leader dies, another instance acquires the lock within seconds

Implementation:
  - Redis SETNX with TTL (lock with expiry)
  - Leader renews lock periodically
  - On leader failure, lock expires → new leader elected
  - Fencing token ensures stale leader can't close an already-closed auction
```

### Interview Questions

1. **Design a distributed cron system where exactly one node runs each scheduled job.**
2. **What is the split-brain problem and how do fencing tokens solve it?**
3. **Compare ZooKeeper, etcd, and Redis for leader election. Trade-offs?**
4. **Design a system where each auction shard is owned by exactly one processing node.**
5. **How do you handle the transition period when a leader dies and a new one is elected?**

### Common Mistakes

- Using Redis `SETNX` without TTL → lock never released if owner crashes
- Not implementing fencing → split brain causes data corruption
- Setting lock TTL too short → leader keeps losing lock under normal operation
- Setting lock TTL too long → recovery takes too long when leader actually dies

---

## Summary: Key Takeaways from Designing Distributed Systems

| Pattern | Category | Use When |
|---------|----------|----------|
| Sidecar | Single-node | Extend app with cross-cutting concerns without code changes |
| Ambassador | Single-node | Proxy outgoing connections (sharding, auth, protocol translation) |
| Adapter | Single-node | Normalize app's interface for external consumers (monitoring, logging) |
| Replicated Service | Serving | Scale stateless services for throughput and availability |
| Sharded Service | Serving | Route requests to specific instances based on data ownership |
| Scatter/Gather | Serving | Fan-out request to all nodes, merge results |
| Work Queue | Batch | Distribute independent work items across worker pools |
| Event-Driven Batch | Batch | Build multi-stage processing pipelines from events |
| Coordinated Batch | Batch | MapReduce-style processing requiring coordination between stages |
| FaaS | Advanced | Event-driven, stateless, low/variable traffic workloads |
| Ownership Election | Advanced | Exactly-one semantics for leaders, singletons, cron jobs |

### Cross-Pattern Connections

```
Building a complete auction platform:

External Traffic:
  → API Gateway (Replicated Service)
    → BidService (Replicated + Sharded by auction region)
      → Redis Cache (Sharded Service via Ambassador)
    → SearchService (Scatter/Gather across Elasticsearch shards)

Background Processing:
  → Image Processing (Work Queue via RabbitMQ + KEDA)
  → Analytics Pipeline (Event-Driven Batch via Kafka)
  → Daily Settlement (Coordinated Batch)

Infrastructure:
  → Envoy sidecar (mTLS, observability)
  → Prometheus exporter adapter (metrics normalization)
  → Auction Closing Job (Ownership Election via Redis lock)
  → Thumbnail Generation (FaaS for bursty, stateless work)
```
