# Designing Data-Intensive Applications — Martin Kleppmann

> The definitive guide to understanding the internals and trade-offs of modern data systems.

---

## Part I: Foundations of Data Systems

---

## Chapter 1: Reliable, Scalable, and Maintainable Applications

### Key Ideas

Modern applications are **data-intensive**, not compute-intensive. The bottleneck is usually data — volume, complexity, and rate of change. Three non-negotiable pillars define a good data system:

- **Reliability**: The system continues working correctly even when things go wrong (hardware faults, software bugs, human errors).
- **Scalability**: The system can handle growth — in data volume, traffic, or complexity — without degrading.
- **Maintainability**: Future engineers can work productively on the system — operability, simplicity, and evolvability.

### Core Concepts & Trade-offs

| Concept | What It Means | Trade-off |
|---------|--------------|-----------|
| Fault vs. Failure | A fault is one component deviating; a failure is the whole system stopping | Design for fault-tolerance, not fault-prevention |
| Hardware faults | Disk dies, RAM corrupts, power goes out | Redundancy (RAID, dual power, multi-AZ) |
| Software faults | Systematic bugs, cascading failures, resource leaks | Monitoring, circuit breakers, chaos testing |
| Human errors | Misconfigurations, bad deploys | Sandboxes, rollback, gradual rollouts, observability |
| Latency percentiles | p50, p95, p99 — not averages | Tail latency costs more to optimize; SLAs usually target p99 |
| Scaling up vs. out | Vertical (bigger machine) vs. horizontal (more machines) | Vertical is simpler; horizontal is necessary at scale |

### Modern Backend Application (.NET / Java / Node)

In a .NET microservices system (like the auction platform):
- **Reliability** → Health checks, retry policies (Polly), circuit breakers, outbox pattern for message delivery
- **Scalability** → Kubernetes HPA, partitioned databases, CQRS read/write separation
- **Maintainability** → Clean Architecture, DDD, automated tests, CI/CD pipelines

### Real-World Example: Auction Platform

```
User places bid → API Gateway → BidService → validates → writes to DB
                                            → publishes BidPlaced event
                                            → AuctionService updates highest bid
```

- **Reliability**: If AuctionService is down, the event sits in RabbitMQ/Kafka. No data loss.
- **Scalability**: BidService scales horizontally; read queries go to separate read replicas.
- **Maintainability**: Each service owns its schema, can be deployed independently.

### Interview Questions

1. **How do you distinguish between faults and failures? Give an example where a fault doesn't become a failure.**
2. **Why are percentile latencies (p99) more important than averages for SLAs?**
3. **Describe a scenario where scaling vertically is actually the right choice over horizontal scaling.**
4. **How would you design a system that tolerates human operational errors?**
5. **In an auction system, how do you ensure reliability when the payment service goes down mid-transaction?**

### Practical Exercises

1. **Fault injection**: Add a middleware to your .NET service that randomly returns 500 errors with 5% probability. Observe how upstream services react. Implement retry + circuit breaker with Polly.
2. **Percentile tracking**: Instrument your API with Prometheus histograms. Compare p50 vs p99 latencies under load. Identify which endpoints have the worst tail latency.
3. **Operability audit**: Review your deployment pipeline. Can you roll back in under 2 minutes? Can a new team member deploy to staging on day one?

### Common Mistakes

- Treating averages as representative of user experience (p99 can be 10x the average)
- Building for scale before validating product-market fit
- Ignoring human error — it causes more outages than hardware failure
- Conflating reliability with "no bugs" — reliability means graceful degradation

### When NOT to Apply

- Don't over-engineer reliability for a prototype or MVP — YAGNI applies
- Don't scale horizontally if a single PostgreSQL instance handles your load fine
- Don't add complexity (event sourcing, CQRS) purely for "maintainability" on a small system

### Connections

- **Microservices**: Each pillar maps directly — reliability (fault isolation), scalability (independent scaling), maintainability (team autonomy)
- **CAP Theorem**: Reliability under partition requires choosing between consistency and availability
- **SRE**: Error budgets formalize the reliability vs. velocity trade-off

---

## Chapter 2: Data Models and Query Languages

### Key Ideas

The data model is the most fundamental decision in system design. It shapes how you think about the problem. Major models:

- **Relational (SQL)**: Tables, rows, joins. Best for structured data with many-to-many relationships.
- **Document (NoSQL)**: JSON-like nested structures. Best for self-contained entities with few joins.
- **Graph**: Nodes and edges. Best for highly interconnected data (social networks, fraud detection).

Each model makes some operations easy and others awkward. **No model is universally superior.**

### Core Concepts & Trade-offs

| Concept | Relational | Document | Graph |
|---------|-----------|----------|-------|
| Schema | Explicit (schema-on-write) | Implicit (schema-on-read) | Flexible |
| Joins | Native, optimized | Weak/manual | Traversals |
| Data locality | Scattered across tables | Entire document together | Depends on storage |
| Many-to-many | Natural with join tables | Awkward, denormalization | Natural with edges |
| Evolution | ALTER TABLE (migrations) | Just add fields | Add node/edge types |
| Impedance mismatch | ORM bridges the gap | Closer to application objects | Domain-specific |

**Schema-on-write** (relational): DB enforces structure at write time. Catches errors early. Migrations required.
**Schema-on-read** (document): Structure interpreted at read time. Flexible but pushes validation to app code.

### Modern Backend Application

- **Relational (.NET + EF Core)**: Auction entities, user accounts, payment records — structured, transactional
- **Document (MongoDB)**: Auction item descriptions with variable attributes (art vs. cars vs. electronics)
- **Graph (Neo4j)**: "Users who bid on similar items" recommendation engine, fraud ring detection

### Real-World Example: E-Commerce Product Catalog

A product catalog is a classic document model use case:
```
Product {
  id: "prod-123",
  name: "Vintage Watch",
  category: "Watches",
  attributes: {
    brand: "Rolex",
    year: 1965,
    condition: "Good",
    certificate: true
  },
  images: [...],
  seller: { id: "user-456", rating: 4.8 }
}
```

Each product category has different attributes. A relational schema would need EAV (Entity-Attribute-Value) pattern — messy. Document model handles this naturally.

But for "find all bids by user X across all auctions" — that's a many-to-many relationship. Relational model wins.

### Interview Questions

1. **When would you choose a document database over relational for a microservice? What are the risks?**
2. **Explain impedance mismatch and how ORMs like Entity Framework try to solve it.**
3. **Design a data model for an auction platform — would you use relational, document, or both? Why?**
4. **How does schema-on-read affect data quality over time in a long-lived system?**
5. **When would a graph database outperform both relational and document models?**

### Practical Exercises

1. Model the same auction entity in PostgreSQL (normalized), MongoDB (document), and compare query complexity for: "Get all bids for auction X" vs. "Get all auctions where user Y bid."
2. Implement a polyglot persistence approach: use PostgreSQL for transactions, Redis for caching, and Elasticsearch for search in a single .NET service.
3. Build a simple recommendation query: "Users who bid on this item also bid on..." — compare SQL (3+ joins) vs. a graph traversal.

### Common Mistakes

- Choosing MongoDB because "it's faster" without understanding you'll need joins (which it doesn't do well)
- Over-normalizing relational schemas (7 joins for a simple read)
- Using relational for data with highly variable schemas (EAV anti-pattern)
- Ignoring the impedance mismatch — your domain model and DB model will diverge

### Connections

- **Microservices**: Each service chooses its own data model (polyglot persistence)
- **CQRS**: Read model can be a different data model than the write model
- **Scalability**: Document and graph models often shard more naturally than relational

---

## Chapter 3: Storage and Retrieval

### Key Ideas

Two major families of storage engines power almost every database:

1. **Log-structured engines** (LSM trees, SSTables): Write-optimized. Append-only. Compact in background.
   - Used by: Cassandra, LevelDB, RocksDB, HBase
2. **Page-oriented engines** (B-trees): Read-optimized. Update in-place. The default for most relational DBs.
   - Used by: PostgreSQL, MySQL, SQL Server, Oracle

Understanding the difference is critical for choosing the right database and tuning performance.

### Core Concepts & Trade-offs

| Aspect | LSM Trees | B-Trees |
|--------|----------|---------|
| Write performance | Excellent (sequential writes) | Good (random I/O for updates) |
| Read performance | Slower (check multiple levels) | Fast (one tree traversal) |
| Write amplification | Higher (compaction rewrites) | Lower per-write, but fragmentation |
| Space amplification | Temporary during compaction | Fragmentation over time |
| Predictability | Compaction spikes can affect latency | More predictable latency |
| Concurrency | Simpler (immutable segments) | Complex (latches/locks on pages) |

**Write-Ahead Log (WAL)**: Both engines use WAL for crash recovery — every write goes to an append-only log before updating the main data structure.

**Indexes**: Speed up reads at the cost of writes. Every index adds write overhead.
- **B-tree index**: The workhorse. O(log n) lookups.
- **Hash index**: O(1) for exact lookups, useless for range queries.
- **LSM-based index**: Bloom filters avoid unnecessary disk reads.

**OLTP vs. OLAP**:
| | OLTP | OLAP |
|--|------|------|
| Pattern | Many small reads/writes | Few large analytical scans |
| Users | End users via application | Analysts, data scientists |
| Data size | GB to low TB | TB to PB |
| Bottleneck | Disk seek time / latency | Disk bandwidth / throughput |
| Engine | Row-oriented (B-tree) | Column-oriented (compressed) |

**Column-oriented storage**: Instead of storing entire rows together, store all values of each column together. Enables massive compression and fast analytical queries that touch only a few columns out of hundreds.

### Modern Backend Application

In a .NET auction platform:
- **B-tree (PostgreSQL)**: Primary store for auctions, users, bids. Excellent read performance for transactional queries.
- **LSM (Cassandra/ScyllaDB)**: Time-series bid history, event logs. Write-heavy, append-mostly.
- **Column store (ClickHouse/BigQuery)**: Analytics — "Average selling price by category over the last 12 months."
- **Redis**: In-memory hash indexes for session data, current highest bid caching.

### Architecture Diagram (Text)

```
Writes:                                  Reads:
                                         
User Bid → WAL (append) → Memtable      Query → Check Memtable
                ↓                                    ↓ miss
           Flush to SSTable              Check Bloom Filter → Read SSTable
                ↓                                    ↓
           Background Compaction          Merge results from multiple levels
```

### Interview Questions

1. **Explain the difference between LSM trees and B-trees. When would you choose each?**
2. **What is write amplification and why does it matter for SSD-backed databases?**
3. **How does column-oriented storage achieve 10x compression ratios?**
4. **Your auction platform writes 50K bids/second. Which storage engine family would you use and why?**
5. **Explain the role of Bloom filters in LSM-tree-based databases.**

### Practical Exercises

1. Benchmark PostgreSQL (B-tree) vs. Cassandra (LSM) for a write-heavy workload: Insert 1M bid records and compare throughput and p99 latency.
2. Implement a simple LSM storage engine in C#: append-only log, memtable (SortedDictionary), flush to sorted file, merge compaction.
3. Take an OLTP query ("Get auction by ID") and an OLAP query ("Average bid amount per category per month") — show why row-oriented excels at the first and column-oriented at the second.

### Coding Example: Simple Append-Only Log Store

```csharp
public class SimpleLogStore
{
    private readonly string _filePath;
    private readonly Dictionary<string, long> _index = new();

    public SimpleLogStore(string filePath)
    {
        _filePath = filePath;
    }

    public void Put(string key, string value)
    {
        using var stream = new FileStream(_filePath, FileMode.Append);
        var offset = stream.Position;
        var entry = $"{key},{value}\n";
        var bytes = Encoding.UTF8.GetBytes(entry);
        stream.Write(bytes);
        _index[key] = offset;
    }

    public string? Get(string key)
    {
        if (!_index.TryGetValue(key, out var offset))
            return null;

        using var stream = new FileStream(_filePath, FileMode.Open);
        stream.Seek(offset, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        var line = reader.ReadLine();
        return line?.Split(',', 2)[1];
    }
}
```

### Common Mistakes

- Adding indexes on every column "just in case" — each index slows down writes
- Using OLTP databases for analytical queries (scanning millions of rows in PostgreSQL)
- Ignoring compaction tuning in LSM databases — can cause write stalls under heavy load
- Not understanding that "in-memory database" doesn't mean data is lost on crash (Redis AOF, VoltDB WAL)

### Connections

- **Scalability**: Storage engine choice determines write throughput ceiling
- **Microservices**: Each service picks the storage engine that matches its access pattern
- **Reliability**: WAL ensures durability; compaction affects availability

---

## Chapter 4: Encoding and Evolution

### Key Ideas

Data outlives code. Systems must evolve, and old and new code will coexist during deployments. This chapter is about **how data is encoded** (serialized) and how to **maintain backward and forward compatibility**.

Encoding formats:
- **Language-specific** (Java Serializable, Python pickle): Tied to one language, security risks, no compatibility guarantees. **Avoid.**
- **Text-based** (JSON, XML, CSV): Human-readable, widely supported. Schema is implicit or external.
- **Binary** (Protocol Buffers, Avro, Thrift): Compact, fast, schema-enforced, excellent compatibility story.

### Core Concepts & Trade-offs

| Format | Size | Speed | Schema | Compatibility | Language Support |
|--------|------|-------|--------|--------------|-----------------|
| JSON | Large | Slow | Optional (JSON Schema) | Manual | Universal |
| Protocol Buffers | Small | Fast | Required (.proto) | Excellent | Wide |
| Avro | Smallest | Fast | Required | Best (schema evolution) | Wide |
| MessagePack | Medium | Fast | None | Manual | Wide |

**Backward compatibility**: New code can read old data. (Add new fields with defaults.)
**Forward compatibility**: Old code can read new data. (Old code ignores unknown fields.)

Rules for Protobuf/Avro compatibility:
- You CAN add a new optional field (backward + forward compatible)
- You CAN remove an optional field (if you never reuse the field number)
- You CANNOT change a field's data type (usually)
- You CANNOT make an optional field required

### Modern Backend Application

In your auction platform with microservices communicating via messages:
- **REST APIs**: JSON — human-readable, debuggable, universal
- **gRPC (inter-service)**: Protocol Buffers — compact, typed, contract-first
- **Event bus (Kafka/RabbitMQ)**: Avro with a schema registry — best evolution story for long-lived events
- **Database storage**: The database's own encoding (PostgreSQL binary format)

### Schema Evolution in Practice

```
Version 1 (BidPlaced event):
{ auctionId, bidderId, amount, timestamp }

Version 2 (new field added):
{ auctionId, bidderId, amount, timestamp, currency }  ← default "USD"

Version 3 (field deprecated):
{ auctionId, bidderId, amount, timestamp, currency }
  ↑ bidderId renamed to userId in code, but field number/name unchanged in schema
```

Old consumers still work with v2 events (they ignore `currency`). New consumers handle v1 events (default `currency` to "USD").

### Interview Questions

1. **Why should you never use language-specific serialization (Java Serializable) for inter-service communication?**
2. **Explain backward vs. forward compatibility with a concrete example from an event-driven system.**
3. **Your auction events need to evolve over 5 years. Which encoding format would you choose and why?**
4. **How does a schema registry (Confluent) prevent breaking changes in a Kafka-based system?**
5. **What happens if two microservices are deployed at different versions and exchange messages? How do you prevent data loss?**

### Practical Exercises

1. Define a `BidPlaced` event in Protocol Buffers. Evolve it through 3 versions (add field, deprecate field, rename field). Verify backward/forward compatibility.
2. Benchmark JSON vs. Protobuf serialization in .NET for 100K auction events — compare size and speed.
3. Set up a Confluent Schema Registry with Kafka and demonstrate that a breaking schema change (removing a required field) is rejected.

### Common Mistakes

- Breaking changes deployed without versioning — consumers crash on unknown fields
- Using JSON for high-throughput inter-service communication (10x larger than Protobuf)
- Not planning for schema evolution from day one in event-driven architectures
- Encoding dates as strings without timezone information
- Reusing field numbers in Protobuf after deleting a field

### Connections

- **Microservices**: Schema evolution is critical when services deploy independently
- **Reliability**: Incompatible schemas cause silent data corruption
- **Event Sourcing**: Events are stored forever — their schemas MUST evolve gracefully

---

## Part II: Distributed Data

---

## Chapter 5: Replication

### Key Ideas

Replication means keeping a copy of the same data on multiple machines. Three reasons:
1. **High availability**: System works even if some nodes fail
2. **Latency**: Serve reads from a geographically close replica
3. **Throughput**: Scale out read capacity

Three replication approaches:

**Single-leader (master-slave)**:
- One leader accepts writes; followers replicate asynchronously
- Simple, well-understood. Used by PostgreSQL, MySQL, SQL Server
- Risk: leader failure requires failover

**Multi-leader**:
- Multiple nodes accept writes; they replicate to each other
- Useful for multi-datacenter setups
- Complex: write conflicts must be resolved

**Leaderless (Dynamo-style)**:
- Any node accepts reads and writes; quorums determine consistency
- Used by Cassandra, DynamoDB, Riak
- Most available; weakest consistency guarantees

### Core Concepts & Trade-offs

| Aspect | Single-Leader | Multi-Leader | Leaderless |
|--------|--------------|-------------|------------|
| Write throughput | Limited by leader | Higher | Highest |
| Consistency | Strong (sync) or eventual (async) | Eventual (conflict resolution) | Eventual (quorums) |
| Availability | Leader is SPOF | High | Highest |
| Complexity | Low | High (conflicts) | Medium (quorums) |
| Read scaling | Excellent (read replicas) | Good | Good |
| Use case | Most OLTP apps | Multi-DC, offline-first | High availability, partition tolerance |

**Synchronous vs. asynchronous replication**:
- Synchronous: Leader waits for follower ACK. Strong consistency. Higher latency. Risk of blocking.
- Asynchronous: Leader doesn't wait. Fast. Risk of data loss if leader crashes.
- Semi-synchronous: One follower is sync, rest are async. Common compromise.

**Replication lag problems**:
- **Read-after-write consistency**: User writes data, then reads from a replica that hasn't caught up yet → sees stale data
- **Monotonic reads**: User reads from replica A (up-to-date), then replica B (behind) → data seems to "go backward"
- **Consistent prefix reads**: Causally related writes appear out of order on replicas

**Conflict resolution strategies** (multi-leader/leaderless):
- Last-write-wins (LWW): Simple but data loss
- Merge values: Application-specific logic
- CRDT: Conflict-free data structures
- Custom resolution: Push conflict to application layer

### Modern Backend Application

In the auction platform:
- **PostgreSQL single-leader** with streaming replication: Primary handles writes, 2 read replicas handle queries
- **Read-after-write**: After placing a bid, the user's next read is routed to the primary (not a replica) to guarantee they see their own bid
- **Multi-leader consideration**: If the platform operates in US + EU datacenters, each DC has a leader for local auctions

### Architecture Diagram (Text)

```
Single-Leader Replication:

  Client Writes → [Leader / Primary]
                       |
            ┌──────────┼──────────┐
            ↓          ↓          ↓
      [Follower 1] [Follower 2] [Follower 3]
            ↑          ↑          ↑
       Client Reads (load-balanced)

Replication Log: Leader → WAL stream → Followers apply changes
```

### Interview Questions

1. **Your auction platform shows stale bid data after a user places a bid. Diagnose the problem and propose solutions.**
2. **Compare single-leader vs. leaderless replication for a global bidding system with users on 5 continents.**
3. **How would you handle a split-brain scenario in single-leader replication?**
4. **Explain why last-write-wins conflict resolution can lose data. When is it acceptable?**
5. **Design a replication strategy for an auction system that must survive an entire datacenter going offline.**

### Practical Exercises

1. Set up PostgreSQL streaming replication (1 primary, 2 replicas). Insert rows on primary and measure replication lag. Simulate a failover by killing the primary.
2. Implement read-after-write consistency in your .NET service: after a write, route that user's reads to the primary for 5 seconds.
3. Simulate a multi-leader write conflict: two users update the same auction description in two different datacenters. Implement LWW and custom merge resolution.

### Common Mistakes

- Assuming replicas are always consistent (they're not — replication lag is real)
- Not handling failover properly — split-brain can cause data corruption
- Using multi-leader replication without a clear conflict resolution strategy
- Ignoring replication lag in read-heavy microservices behind a load balancer

### Connections

- **CAP Theorem**: Replication strategy directly determines your C vs. A trade-off under partitions
- **Consistency**: Single-leader can offer linearizability; leaderless typically cannot
- **Scalability**: Read replicas are the simplest way to scale read throughput
- **Microservices**: Each service's DB can have its own replication strategy

---

## Chapter 6: Partitioning (Sharding)

### Key Ideas

When data or query throughput exceeds what a single machine can handle, you **partition** (shard) the data across multiple nodes. Each partition is a mini-database.

Two main approaches:

1. **Key-range partitioning**: Partition by ranges of keys (A-F, G-M, N-Z). Good for range queries. Risk of hot spots.
2. **Hash partitioning**: Hash the key and assign to partitions. Even distribution. Destroys key ordering (no efficient range queries).

### Core Concepts & Trade-offs

| Aspect | Key-Range | Hash |
|--------|----------|------|
| Range queries | Efficient | Inefficient (scatter-gather) |
| Hot spots | Likely (e.g., time-based keys) | Unlikely (uniform distribution) |
| Rebalancing | Move ranges | Consistent hashing |
| Complexity | Lower | Higher |

**Secondary indexes with partitions**:
- **Local index (document-partitioned)**: Each partition maintains its own secondary index. Writes are fast; reads require scatter-gather across all partitions.
- **Global index (term-partitioned)**: Secondary index is itself partitioned differently. Reads are fast; writes must update multiple partitions.

**Rebalancing strategies**:
- Fixed number of partitions (Cassandra, Riak): Create many more partitions than nodes; move whole partitions between nodes
- Dynamic partitioning: Split when too large, merge when too small (HBase, MongoDB)
- Proportional to nodes: Fixed partitions per node (Cassandra)

**Request routing**: How does a client know which partition to query?
1. Client contacts any node → node forwards if needed (gossip protocol — Cassandra)
2. Routing tier / partition-aware proxy (MongoDB mongos)
3. Client is partition-aware (client library with partition map)

### Modern Backend Application

In the auction platform:
- **Bids partitioned by auction_id (hash)**: Even distribution, all bids for one auction on the same partition
- **Auctions partitioned by category + time range**: Range queries like "all active electronics auctions" are efficient
- **User data partitioned by user_id (hash)**: Uniform distribution
- **Search index**: Global secondary index in Elasticsearch (separate from the primary store)

### Interview Questions

1. **Your auction system has a "celebrity auction" getting 1000x normal bid traffic. How do you prevent a hot partition?**
2. **Design a partitioning strategy for a bid history table that supports both "bids by user" and "bids by auction" queries efficiently.**
3. **Explain the difference between local and global secondary indexes. Which would you use for a search feature?**
4. **How does consistent hashing work and why is it better than simple modulo hashing for rebalancing?**
5. **Your system needs to rebalance partitions after adding 3 new nodes. How do you do this without downtime?**

### Practical Exercises

1. Implement consistent hashing in C# with virtual nodes. Demonstrate that adding a node only moves ~1/n of the data.
2. Partition a bids table by auction_id hash in PostgreSQL (using declarative partitioning). Benchmark queries with and without partitioning.
3. Design and implement a scatter-gather query: query all partitions in parallel for "top 10 most expensive bids today," then merge results.

### Coding Example: Consistent Hashing

```csharp
public class ConsistentHashRing<T>
{
    private readonly SortedDictionary<int, T> _ring = new();
    private readonly int _virtualNodes;

    public ConsistentHashRing(int virtualNodes = 150)
    {
        _virtualNodes = virtualNodes;
    }

    public void AddNode(T node)
    {
        for (int i = 0; i < _virtualNodes; i++)
        {
            var hash = GetHash($"{node}-{i}");
            _ring[hash] = node;
        }
    }

    public void RemoveNode(T node)
    {
        for (int i = 0; i < _virtualNodes; i++)
        {
            var hash = GetHash($"{node}-{i}");
            _ring.Remove(hash);
        }
    }

    public T GetNode(string key)
    {
        var hash = GetHash(key);
        foreach (var entry in _ring)
            if (entry.Key >= hash)
                return entry.Value;
        return _ring.First().Value;
    }

    private int GetHash(string key) =>
        BitConverter.ToInt32(SHA256.HashData(Encoding.UTF8.GetBytes(key)), 0);
}
```

### Common Mistakes

- Partitioning by a monotonically increasing key (timestamp, auto-increment) → all writes go to the last partition
- Not planning for hot keys (celebrity effect, viral auction)
- Choosing hash partitioning when your primary access pattern is range queries
- Treating rebalancing as a one-time operation (it should be automated and continuous)

### Connections

- **Scalability**: Partitioning is the primary mechanism for horizontal write scaling
- **CAP Theorem**: Partitions increase the likelihood of network partitions between them
- **Microservices**: Each service's data can be independently partitioned

---

## Chapter 7: Transactions

### Key Ideas

Transactions provide safety guarantees that simplify the programming model. **ACID**:

- **Atomicity**: All-or-nothing. If a transaction fails halfway, all changes are rolled back.
- **Consistency**: The database moves from one valid state to another (application-level invariant, not a DB guarantee).
- **Isolation**: Concurrent transactions don't interfere with each other — they appear to execute serially.
- **Durability**: Once committed, data survives crashes (written to disk/replicated).

The most misunderstood letter is **I** (Isolation). Most databases do NOT provide true serializable isolation by default — they use weaker levels for performance.

### Isolation Levels

| Level | Dirty Reads | Non-repeatable Reads | Phantom Reads | Lost Updates | Performance |
|-------|------------|---------------------|---------------|-------------|-------------|
| Read Uncommitted | ✗ | ✗ | ✗ | ✗ | Fastest |
| Read Committed | ✓ | ✗ | ✗ | ✗ | Fast |
| Repeatable Read / Snapshot | ✓ | ✓ | ✗ | Depends | Medium |
| Serializable | ✓ | ✓ | ✓ | ✓ | Slowest |

(✓ = prevented, ✗ = possible)

**Read Committed** (PostgreSQL default): No dirty reads, no dirty writes. But non-repeatable reads are possible.

**Snapshot Isolation (MVCC)**: Each transaction sees a consistent snapshot of the database. PostgreSQL "Repeatable Read" is actually snapshot isolation. Prevents most anomalies but NOT write skew.

**Write skew**: Two transactions read the same data, make decisions based on it, and write different rows — resulting in an invalid state. Example: Two doctors try to go off-call simultaneously; each checks "at least one doctor on call" passes, both write → zero doctors on call.

**Serializable implementations**:
1. **Actual serial execution**: Run transactions one at a time on a single core (VoltDB, Redis). Surprisingly viable with in-memory data.
2. **Two-Phase Locking (2PL)**: Readers and writers block each other. Strong but slow.
3. **Serializable Snapshot Isolation (SSI)**: Optimistic. Run transactions in parallel; detect conflicts at commit time. PostgreSQL SSI.

### Modern Backend Application

In the auction platform:
- **Placing a bid**: Must atomically check highest bid, validate amount > current highest, insert new bid, update auction's current price → **Serializable** or application-level locking
- **Payment processing**: Transfer from buyer to seller must be atomic → distributed transaction or Saga
- **Inventory management**: "Decrement available quantity" is a classic lost update scenario → `UPDATE ... SET qty = qty - 1 WHERE qty > 0`

### Interview Questions

1. **Explain write skew with an auction example. How would you prevent it?**
2. **Why does PostgreSQL's "Repeatable Read" not prevent all anomalies? What's missing?**
3. **Compare 2PL vs. SSI for a high-throughput bidding system. Which would you choose?**
4. **How do you handle transactions across microservices where each has its own database?**
5. **Design a "Buy It Now" feature that prevents two users from purchasing the same item simultaneously.**

### Practical Exercises

1. Reproduce write skew in PostgreSQL: Two concurrent transactions each check a condition and write — show the anomaly at Repeatable Read, then prevent it at Serializable.
2. Implement optimistic concurrency control in .NET/EF Core using a version/concurrency token on the Auction entity. Demonstrate conflict detection.
3. Benchmark Serializable vs. Read Committed for 1000 concurrent bid operations. Measure throughput difference and conflict rate.

### Common Mistakes

- Assuming "Repeatable Read" in your database is actually serializable (it's usually snapshot isolation)
- Using distributed transactions (2PC) across microservices — they kill availability
- Not considering write skew in business-critical operations (auctions, inventory, scheduling)
- Wrapping huge business flows in a single long transaction (holds locks, hurts concurrency)

### Connections

- **Microservices**: Cross-service transactions don't work → use Sagas instead
- **Consistency**: Transactions are the mechanism for maintaining consistency within a single service
- **CAP Theorem**: Under partitions, distributed transactions must sacrifice availability

---

## Chapter 8: The Trouble with Distributed Systems

### Key Ideas

This chapter is a reality check. Distributed systems suffer from three fundamental problems:

1. **Unreliable networks**: Packets get lost, delayed, duplicated, or reordered. You never know if a remote node received your message.
2. **Unreliable clocks**: Each machine has its own clock. Clocks drift. NTP synchronization is imprecise (milliseconds). You cannot rely on timestamps for ordering events.
3. **Process pauses**: A node can pause for seconds (GC, VM migration, disk I/O) and not know it was paused.

**There is no reliable way to distinguish a slow node from a dead node.**

### Core Concepts & Trade-offs

**Network faults**:
- The only thing you can do is wait for a response, and after a timeout, assume failure
- Timeouts: Too short → false positives (declare healthy nodes dead). Too long → slow detection of actual failures.
- Solution: Adaptive timeouts based on observed RTT distribution

**Clock issues**:
- **Time-of-day clocks**: Wall clock time. Can jump backward (NTP correction). Don't use for measuring durations.
- **Monotonic clocks**: Always move forward. Use for measuring elapsed time.
- **Logical clocks** (Lamport, vector clocks): Don't measure real time; capture causal ordering of events.
- Google's TrueTime: GPS + atomic clocks with bounded uncertainty intervals. Enables Spanner's external consistency.

**Byzantine faults**: Nodes that deliberately lie or send corrupted data (relevant for blockchain, not typical data centers). Most systems assume non-Byzantine faults (nodes are honest but may crash).

### Modern Backend Application

In the auction platform:
- **Bid ordering**: Don't use wall-clock timestamps to determine bid order → use a sequence number from the leader or logical clock
- **Auction close time**: "Auction ends at 3:00 PM" — whose clock? The server's. But clock skew between servers can cause different servers to disagree.
- **Heartbeat / health checks**: Kubernetes liveness probes use timeouts. If GC pauses cause a probe timeout → pod gets killed (false positive)
- **Idempotency**: Since you can't tell if a request was processed or lost, make every operation idempotent with idempotency keys

### Interview Questions

1. **Two users both claim they placed the winning bid at "exactly 2:59:59 PM." How do you determine the real winner?**
2. **How would you implement a distributed lock that's safe despite process pauses?**
3. **Explain why NTP is insufficient for ordering events in a distributed auction system.**
4. **Design a timeout strategy for inter-service communication in a microservices architecture.**
5. **What is the "fencing token" pattern and why is it necessary for distributed locks?**

### Practical Exercises

1. Simulate network partitions in Docker using `tc` (traffic control) to add latency and packet loss. Observe how your .NET services behave.
2. Implement a Lamport clock for ordering bid events across two services (BidService and AuctionService). Show that causal ordering is preserved.
3. Implement a fencing-token-based distributed lock using Redis. Demonstrate that a GC-paused client can't corrupt data after its lock expires.

### Common Mistakes

- Using `DateTime.UtcNow` across services as if clocks are perfectly synchronized
- Setting static timeouts instead of adaptive ones
- Trusting distributed locks without fencing tokens
- Assuming network calls are reliable — always plan for failure

### Connections

- **CAP Theorem**: Network partitions are not theoretical — they happen (this chapter proves it)
- **Reliability**: Every reliability mechanism must account for these three problems
- **Consistency**: True consistency across nodes requires solving the clock and network problems

---

## Chapter 9: Consistency and Consensus

### Key Ideas

This is the climax of the book. It ties together replication, transactions, and the fundamental limits of distributed systems.

**Linearizability** (strongest consistency): Every operation appears to take effect atomically at some point between invocation and response. The system behaves as if there's only one copy of the data.

**Cost of linearizability**: Under network partitions, a linearizable system must become unavailable (this is the CAP theorem).

**Ordering guarantees**:
- **Causal consistency**: If event A caused event B, everyone sees A before B. Weaker than linearizability but achievable without sacrificing availability.
- **Total order broadcast**: All nodes process messages in the same order. Equivalent to consensus.

**Consensus**: Getting multiple nodes to agree on a value. Solved by:
- **Raft** (etcd, Consul)
- **Paxos** (Google Chubby, Azure Cosmos DB)
- **ZAB** (ZooKeeper)
- **Viewstamped Replication**

**FLP impossibility**: In an asynchronous system with even one faulty node, consensus is impossible to guarantee (in theory). In practice, timeouts make it work.

### Consistency Model Hierarchy

```
Strongest:  Linearizability (appears as single copy)
                ↓
            Sequential Consistency (everyone sees same order)
                ↓
            Causal Consistency (cause before effect)
                ↓
            Eventual Consistency (converge... eventually)
Weakest
```

### CAP Theorem — Properly Stated

You can have at most 2 of 3: **Consistency** (linearizability), **Availability** (every request gets a response), **Partition tolerance** (system works despite network splits).

Since partitions are unavoidable in distributed systems, the real choice is:
- **CP**: Sacrifice availability during partitions (e.g., HBase, MongoDB with majority writes)
- **AP**: Sacrifice consistency during partitions (e.g., Cassandra, DynamoDB)

**CAP is a spectrum**, not a binary choice. Most real systems tune their consistency per-operation.

### Modern Backend Application

In the auction platform:
- **Linearizable operation**: "Buy It Now" — exactly one buyer can win. Use consensus (Raft-based lock or serializable transaction).
- **Causal consistency**: Bid notifications — a user should see "You were outbid" only after seeing the original bid. Use causal ordering, not wall-clock.
- **Eventual consistency**: Auction search index — it's okay if a new auction takes a few seconds to appear in search results.
- **Consensus in practice**: etcd (in Kubernetes) stores cluster state via Raft consensus.

### Interview Questions

1. **Explain linearizability vs. eventual consistency with an auction bidding example.**
2. **Why is the CAP theorem often misunderstood? What's the actual practical implication?**
3. **How does the Raft consensus algorithm elect a leader? What happens during a network partition?**
4. **When would you choose causal consistency over linearizability? What do you gain?**
5. **Design the "auction close" logic where exactly one winner must be determined, even during network partitions.**

### Practical Exercises

1. Demonstrate a linearizability violation: Two clients read a value — Client A sees new value, Client B (reading later) sees old value. Set up a PostgreSQL replica and show this with async replication.
2. Implement a simple Raft leader election in C# (3 nodes, term numbers, heartbeats, split vote handling).
3. Implement causal ordering using vector clocks for a chat system where messages reference each other.

### Common Mistakes

- Saying "we use eventual consistency" without understanding the anomalies it allows
- Assuming CAP means you pick two and ignore the third (partitions are not optional)
- Using distributed locks without understanding the consensus protocol underneath
- Treating linearizability and serializability as the same thing (they're different guarantees about different things)

### Connections

- **Microservices**: Each service makes its own consistency-availability trade-off
- **Reliability**: Consensus protocols are the foundation of reliable leader election, distributed locks, and atomic broadcast
- **Scalability**: Linearizability limits scalability; causal consistency scales better

---

## Part III: Derived Data

---

## Chapter 10: Batch Processing

### Key Ideas

Batch processing transforms large volumes of data in one go. The paradigm: **read all input, produce all output, no side effects**.

**Unix philosophy** → MapReduce → modern data pipelines:
- Small, composable tools that do one thing well
- Input is immutable; output is derived
- Explicit dataflow (input → transformation → output)

**MapReduce**:
1. **Map**: Process each record independently, emit (key, value) pairs
2. **Shuffle/Sort**: Group by key across all nodes
3. **Reduce**: Aggregate all values for each key

**Beyond MapReduce**:
- MapReduce is clunky for multi-stage pipelines (writes intermediate results to HDFS)
- **Dataflow engines** (Spark, Flink, Tez): Model computation as a DAG. Keep intermediate data in memory. Much faster.
- Spark: RDD (Resilient Distributed Datasets) — immutable, partitioned collections with lazy evaluation

### Core Concepts & Trade-offs

| Aspect | MapReduce | Spark/Flink | Traditional DB |
|--------|----------|-------------|---------------|
| Latency | Minutes to hours | Seconds to minutes | Milliseconds |
| Throughput | Very high | High | Medium |
| Fault tolerance | Rerun failed tasks (idempotent) | Lineage-based recovery | Transactions |
| State | Stateless between stages | In-memory (Spark), managed (Flink) | Full state management |
| Use case | ETL, data warehousing | Analytics, ML pipelines | OLTP |

**Join strategies in batch**:
- **Sort-merge join**: Both inputs sorted by join key → merge. Efficient for large datasets.
- **Broadcast hash join**: Small table fits in memory → hash it, probe with large table.
- **Partitioned hash join**: Both tables partitioned by join key → join within each partition.

### Modern Backend Application

In the auction platform:
- **Daily analytics**: "Total bid volume, average selling price, conversion rates by category" → batch job in Spark/Databricks
- **Recommendation engine**: "Users who bid on X also bid on Y" → MapReduce-style co-occurrence analysis
- **Data migration**: Migrating auction history from old format to new → idempotent batch transformation
- **ML feature engineering**: Computing bidder behavior features (bid frequency, time patterns) for a fraud detection model

### Interview Questions

1. **Design a batch pipeline that computes daily auction statistics (total bids, highest bid, number of unique bidders per auction).**
2. **Why is MapReduce considered slow compared to Spark? What's the key architectural difference?**
3. **How do you handle failures in a batch job processing 10TB of bid history?**
4. **Explain the difference between sort-merge join and broadcast hash join. When would you use each?**
5. **Design an ETL pipeline that moves data from your OLTP auction database to a data warehouse for analytics.**

### Practical Exercises

1. Write a MapReduce-style job in C# using PLINQ: Given a file of bid records, compute total bid amount per auction and top 10 highest-value auctions.
2. Implement a simple data pipeline that reads auction events from files, joins with user data, and outputs denormalized records for analytics.
3. Compare batch vs. stream processing: Compute "rolling 24-hour bid count per auction" using both approaches. Measure accuracy and latency trade-offs.

### Common Mistakes

- Running batch jobs that aren't idempotent — reruns produce duplicates
- Using batch processing when stream processing would provide faster results
- Not partitioning batch input well — some tasks get all the data (data skew)
- Writing intermediate results to a slow storage layer (HDFS) when they could stay in memory

### Connections

- **Microservices**: Batch jobs often consume event logs from microservices to build derived views
- **CQRS**: The read model can be rebuilt by replaying events (a batch operation)
- **Reliability**: Idempotent processing ensures correctness despite task failures

---

## Chapter 11: Stream Processing

### Key Ideas

Stream processing is batch processing's real-time cousin. Instead of processing bounded datasets, you process **unbounded streams** — events that arrive continuously.

**Event streams**:
- An event is an immutable fact: "User X bid $500 on auction Y at time T"
- Events are produced by **producers** and consumed by **consumers**
- Events are stored in a **log** (Kafka, Kinesis, Pulsar)

**Log-based message brokers** (Kafka) vs. **traditional message queues** (RabbitMQ):

| Aspect | Log-based (Kafka) | Traditional Queue (RabbitMQ) |
|--------|-------------------|------------------------------|
| Delivery | Consumer pulls from log | Broker pushes to consumer |
| Ordering | Per-partition ordering guaranteed | Per-queue ordering (mostly) |
| Replay | Yes — consumers seek to any offset | No — once consumed, gone |
| Consumer groups | Multiple groups read independently | Message goes to one consumer |
| Retention | Time/size-based (days/weeks) | Typically until consumed |
| Backpressure | Natural (consumer controls pace) | Broker manages prefetch |

**Stream processing use cases**:
1. **Complex Event Processing (CEP)**: Detect patterns in event streams (fraud detection)
2. **Stream analytics**: Rolling aggregations, dashboards
3. **Materialized views**: Derive and maintain views from event streams (CQRS read models)
4. **Stream joins**: Enrich events by joining with other streams or tables

**Exactly-once semantics**:
- **At-most-once**: Fire and forget. Messages may be lost.
- **At-least-once**: Retry until ACK. Messages may be duplicated. Consumers must be idempotent.
- **Exactly-once**: Each message processed exactly once. Hard to achieve. Approaches: idempotent consumers, transactional outbox, Kafka transactions.

**Time in stream processing**:
- **Event time**: When the event actually occurred
- **Processing time**: When the system processes the event
- These can differ significantly (late events, network delays)
- **Windowing**: Tumbling (fixed non-overlapping), hopping (fixed overlapping), sliding, session

### Modern Backend Application

In the auction platform:
- **Bid event stream**: Every bid is published to Kafka → consumed by AuctionService (update highest bid), NotificationService (send outbid alert), AnalyticsService (update real-time dashboards)
- **Materialized view**: CQRS read model of "Auction details with current bid and bid count" maintained by consuming BidPlaced events
- **Fraud detection**: CEP pattern — "Same user places bids from 3 different countries within 5 minutes" → flag for review
- **Auction closing**: Time-windowed computation — "When auction end time passes, determine winner from bid stream"

### Architecture Diagram (Text)

```
Producers:                Event Log (Kafka):              Consumers:
                         
BidService ────→ ┌─────────────────────┐ ──→ AuctionService (update state)
                 │ Topic: bid-events   │ ──→ NotificationService (alerts)
AuctionService → │ Partition 0 [||||| ]│ ──→ AnalyticsService (dashboards)
                 │ Partition 1 [||||| ]│ ──→ SearchService (update index)
UserService ───→ │ Partition 2 [||||| ]│ ──→ FraudDetection (CEP)
                 └─────────────────────┘
                 
Each consumer group reads independently.
Each consumer group can be at a different offset.
Events are retained for days/weeks → full replay possible.
```

### Interview Questions

1. **Design the real-time notification system for an auction platform using stream processing.**
2. **How would you handle late-arriving bid events in a stream processing system that determines auction winners?**
3. **Compare Kafka and RabbitMQ for an event-driven microservices architecture. When would you use each?**
4. **How do you achieve exactly-once processing in a Kafka-based system?**
5. **Design a fraud detection system that processes bid events in real-time and flags suspicious patterns.**

### Practical Exercises

1. Set up Kafka with 3 partitions. Produce bid events from multiple services. Implement a consumer group that maintains a "current highest bid per auction" materialized view.
2. Implement an idempotent event consumer in .NET: use a deduplication table to ensure each BidPlaced event is processed exactly once, even with retries.
3. Build a simple CEP: Detect "3 bids from the same user within 10 seconds" using a sliding window. Alert when the pattern matches.

### Common Mistakes

- Using traditional message queues when you need event replay (you'll regret it when you need to rebuild a read model)
- Ignoring the event time vs. processing time distinction → late events cause incorrect aggregations
- Assuming exactly-once is free — it requires careful design (idempotent consumers, transactional outbox)
- Creating a single Kafka topic for all events → no partition-level ordering guarantees across different entity types

### Connections

- **Microservices**: Event-driven communication is the backbone of reactive microservices
- **CQRS/Event Sourcing**: Stream processing is how you build and maintain read models
- **Reliability**: Log-based brokers provide durability; consumer offsets provide exactly-once semantics
- **CAP Theorem**: Eventual consistency in stream processing is the AP choice

---

## Chapter 12: The Future of Data Systems

### Key Ideas

This chapter synthesizes the entire book into a forward-looking vision of data architecture.

**The key insight**: Most data systems are combinations of two fundamental operations:
1. **Deriving state from events** (event sourcing, materialized views, caches)
2. **Propagating changes** (replication, stream processing, CDC)

**Unbundling the database**: A traditional database (PostgreSQL) bundles many things — storage, indexes, caching, replication, transactions. In a modern data architecture, each of these can be a separate, specialized system connected by event streams.

**The total order of events** (event log) becomes the single source of truth. Everything else — databases, caches, search indexes, analytics stores — is a derived view that can be rebuilt from the log.

**End-to-end argument**: Exactly-once semantics must be implemented at the application level (not just the transport level). Even if Kafka provides exactly-once delivery, your application must be idempotent to achieve true exactly-once effect.

**Ethics of data systems**: The chapter concludes with a discussion of privacy, surveillance, data ownership, and the responsibility of engineers building data-intensive systems.

### Core Architecture Pattern

```
Event Log (Source of Truth):
  ┌───────────────────────────────┐
  │  [Event1][Event2][Event3]...  │
  └──────┬──────┬──────┬──────────┘
         │      │      │
         ↓      ↓      ↓
    [PostgreSQL] [Elasticsearch] [Redis Cache]
    (Write model) (Search index) (Hot data)
    
All derived. All rebuildable. All eventually consistent.
```

### Interview Questions

1. **Design an architecture where Kafka is the source of truth and all databases are derived views. What are the trade-offs?**
2. **How would you rebuild a corrupted search index from scratch in a production auction system without downtime?**
3. **Explain the "end-to-end argument" for exactly-once processing and why transport-level guarantees aren't enough.**
4. **Compare a monolithic database vs. an "unbundled" event-log-centric architecture for a growing startup.**
5. **How do you handle data privacy (GDPR right to deletion) in an append-only event log?**

### Practical Exercises

1. Implement CDC (Change Data Capture) from PostgreSQL (using Debezium) → Kafka → Elasticsearch. Show that the search index stays in sync with the database.
2. Build a system where the event log is the source of truth: Write events to Kafka, derive a PostgreSQL materialized view AND a Redis cache from the same stream. Demonstrate that you can rebuild either from the log.
3. Implement GDPR "right to be forgotten" in an event-sourced system: crypto-shredding (encrypt personal data with a per-user key; delete the key to "forget" the user).

### Common Mistakes

- Treating the database as the source of truth in an event-driven architecture (the event log should be)
- Not designing for replayability — you lose the ability to fix bugs in derived views
- Assuming that "eventually consistent" means "slow" — it usually converges in milliseconds
- Ignoring data ethics — engineers are responsible for the systems they build

### Connections

- **Microservices**: The "database per service" pattern naturally leads to the unbundled architecture described here
- **CQRS/Event Sourcing**: This chapter is the theoretical foundation for CQRS
- **Reliability**: Derived data can be rebuilt — the event log provides ultimate disaster recovery
- **Scalability**: Specialized derived stores scale independently

---

## Summary: Key Takeaways from DDIA

| Chapter | One-Line Takeaway |
|---------|-------------------|
| 1 | Design for reliability, scalability, and maintainability — in that order |
| 2 | Choose the data model that matches your access patterns, not the hype |
| 3 | Understand your storage engine — LSM for writes, B-tree for reads, column for analytics |
| 4 | Plan for schema evolution from day one — use Protobuf/Avro, not JSON, for inter-service events |
| 5 | Replication is about availability and read scaling; understand the consistency trade-offs |
| 6 | Partition for write scaling; choose between range and hash based on your query patterns |
| 7 | Transactions are complex; know your isolation level and its anomalies |
| 8 | Networks, clocks, and processes are unreliable — design assuming failure |
| 9 | Consensus is the foundation; CAP forces real trade-offs; linearizability has a cost |
| 10 | Batch processing is powerful for derived data; make it idempotent |
| 11 | Stream processing is the real-time spine of modern architectures |
| 12 | The event log is the source of truth; everything else is derived |
