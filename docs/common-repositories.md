# Shared EF Core repositories

## Research and design

Microsoft's microservices guidance recommends repositories around aggregate
boundaries and permits generic code to reduce repeated persistence plumbing.
EF Core already supplies repository/unit-of-work behavior, so this project keeps
service contracts and queries while sharing the repeated implementation.
Repositories stage changes; the service's unit of work commits them. See
[persistence-layer design](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
and [EF Core implementation guidance](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-implementation-entity-framework-core).

EF Core tracks scalar changes and concurrency values for entities loaded by the
same context. Calling `Update` on a graph can mark related records for writing.
The shared implementation therefore keeps tracked entities tracked, and its
default detached update marks only the aggregate root modified. Services needing
graph updates opt in explicitly. See [change tracking](https://learn.microsoft.com/en-us/ef/core/change-tracking/)
and [disconnected entities](https://learn.microsoft.com/en-us/ef/core/saving/disconnected-entities).

## Components

| Component | Responsibility |
|---|---|
| `IReadRepository<T>` | Existing read contract for ID lookup, existence, and paging |
| `IWriteRepository<T>`, `IRepository<T>`, `IBatchRepository<T>` | Write contracts constrained to `AggregateRoot` |
| `EfRepository<TEntity, TContext>` | Reusable audited CRUD, batch operations, soft deletion, and default paging |
| Service repository interface | The operations available to that service's handlers |
| Concrete service repository | Domain queries, actor policies, required includes, and graph behavior |
| Existing `BaseUnitOfWork<TContext>` | Domain-event dispatch, SaveChanges, and transaction ownership |

The implementation is in
[`EfRepository.cs`](../src/BuildingBlocks/BuildingBlocks.Infrastructure/Repository/EfRepository.cs).
It is abstract, uses a specific service `DbContext`, and does not expose public
`IQueryable`, `DbSet`, SQL strings, or transaction controls. Application and Domain
projects retain their existing dependency directions.

## Common behavior

- `GetByIdAsync` uses no tracking and excludes soft-deleted rows.
- `GetByIdForUpdateAsync` explicitly enables tracking, even when the context's
  default is no tracking. It is a tracked lookup, not a database row lock.
- `ExistsAsync` and default paging exclude soft-deleted rows. Default ordering is
  `CreatedAt` descending, then ID, with the shared paging bounds.
- `AddAsync` delegates to `CreateAsync`. Creation stamps the actor and UTC time.
- `UpdateAsync` stamps update audit fields. Existing tracked changes retain EF's
  property-level tracking; detached updates mark only the root modified.
- `DeleteAsync` stages a soft deletion. ID-based deletes ignore missing and
  deleted records. Passing a detached aggregate is supported; an existing
  deletion timestamp is preserved.
- Batch operations use the same policies, honor cancellation, and do not save.
  Creation materializes an enumerable once and applies one timestamp per batch.
- No operation calls `SaveChanges`, dispatches domain events, publishes messages,
  starts a transaction, or disposes the context. Queries preserve EF global filters.

`Query` is a protected hook for includes or additional visibility constraints.
Reads and ID-based single/batch deletion use it. Entity-based writes still require
the caller to supply an authorized aggregate; a repository is not an authorization
boundary. `GetCreatedBy`, `GetUpdatedBy`, and `GetDeletedBy` customize audit actors.
`ApplyCreatedAudit` handles aggregate-owned children when needed. `TrackUpdate`
allows an explicit detached-graph policy. `ApplyDeletedAudit` runs the aggregate's
deletion policy for both single and batch soft deletion, using the shared timestamp.

Normal handlers should load with `GetByIdForUpdateAsync`, call domain methods,
and save through the unit of work. Do not submit HTTP DTOs or partially populated
graphs directly to `UpdateAsync`. Preserve the original concurrency values and
avoid attaching multiple object instances with the same key to one context.

## Usage

Derive the concrete repository and retain its service-specific interface:

```csharp
public class BrandRepository : EfRepository<Brand, CatalogDbContext>, IBrandRepository
{
    public BrandRepository(
        CatalogDbContext context,
        IDateTimeProvider clock,
        IAuditContext auditContext)
        : base(context, clock, auditContext)
    {
    }

    public Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        Query.AsNoTracking().FirstOrDefaultAsync(brand => brand.Slug == slug, cancellationToken);
}
```

This example omits the remaining `IBrandRepository` query implementations. Shared
CRUD methods are inherited. Existing `CreateAsync`/`AddAsync` service contract
names remain compatible.

Register the service interface and implementation with a scoped lifetime:

```csharp
services.AddScoped<IBrandRepository, BrandRepository>();
```

The repository and unit of work must resolve the same scoped service context.
Do not register a singleton repository or an unrestricted open generic write
repository for every entity. Inject service contracts into handlers so the
aggregate's supported operations remain explicit. Batch sizes remain a caller
responsibility; streaming imports and provider-specific bulk commands keep their
existing implementations.

## Adoption and preserved policies

| Service | Repositories | Specialized behavior retained |
|---|---|---|
| Auction | Auction, Review, Bookmark | Auction item includes/audit; detached Auction graph updates; review relations; bookmark owner audit; existing cached decorator and query ordering |
| Bidding | Bid, AutoBid | Bid-time paging, winning-bid queries, auto-bid owner creation/update audit, requester deletion audit |
| Catalog | Category, Brand | Category child includes and display ordering; brand filters, slug checks, and shared paging |
| Jobs | Job | Child-item creation audit, detached Job graph updates, priority/recovery queries |
| Payment | Order, Wallet | Domain-owned creation timestamps, system update actor, legacy entity-returning write adapters, optimistic concurrency; payment execution/locking remain specialized |
| Notification | Notification, NotificationPreference, Template | Notification physical deletion and read/archive operations; preference lookup; template name ordering, immediate commits, and physical deletion |
| Storage | StoredFile | Tracked ID lookup, file deletion status/events, explicit physical purge; shared add/batch/update auditing |

All 14 concrete repositories for the current services' `AggregateRoot` models now
inherit `EfRepository<TEntity, TContext>`, across seven services. Existing scoped
service-interface registrations resolve these implementations. Auction's cached
decorator delegates persistence to its shared-base implementation.

Auction and Job explicitly preserve their previous detached-graph update behavior.
For an already tracked aggregate, normal EF change detection persists actual
child edits without marking every loaded child modified. This avoids accidental
updates to unchanged related records while retaining the unit-of-work boundary.

Compatibility adapters retain the existing Application contracts. Payment's
`Task<Order>`/`Task<Wallet>` update methods return the same entity after staging
the shared update; they do not save. Their factories still own creation audit
values, and updates retain the system actor (`Guid.Empty`). Wallet locking,
payment transactions, and PostgreSQL concurrency tokens remain unchanged.

Notification templates deliberately retain their legacy immediate commits in
`CreateAsync`, `UpdateAsync`, and ID deletion. The shared base itself never saves.
Notification and template ID deletion still physically remove rows. Use their
service interfaces: the base's additional entity/batch soft-delete methods are
not substitutes for those specialized operations. A future change to template
transaction ownership must migrate callers explicitly.

Storage's Application contract now exposes `UpdateAsync` with cancellation, and
all callers await it before saving through the unit of work. A file's domain
deletion still sets `FileStatus.Deleted` and raises its deletion event. The
`ApplyDeletedAudit` hook preserves that behavior when common single/batch deletion
is used too. `RemoveRange` remains a separate physical purge operation for cleanup.

The remaining repositories/adapters were reviewed and intentionally keep their
specialized persistence:

| Area | Reason |
|---|---|
| Identity | ASP.NET Identity's UserManager/RoleManager own account persistence and string keys |
| Payment: WalletTransactionRepository, wallet payment executor, order-attempt store | Financial ledger/receipt records are not aggregate roots; payment locking, idempotency, and transaction boundaries remain explicit |
| Notification: NotificationRecordRepository | Delivery records and user-notification rows derive from `BaseEntity`, not `AggregateRoot`; one adapter coordinates both models |
| Analytics: AuditLog, FactAuction, FactBid, FactPayment, DailyStats, Report, PlatformSetting repositories | Plain fact/report/settings models use different keys, upserts, retention, or immediate-save policies rather than `AggregateRoot` |
| Jobs: JobItemRepository, JobExecutionLogRepository | Child-item audit/retry/streaming batches and append-only execution logs remain specialized |
| Auction: AuctionBulkRepository, BuyNowPurchaseRepository, ImportCheckpointRepository | Provider-specific bulk SQL, purchase receipts, and Redis checkpoints have separate persistence contracts |
| Bidding: CachedAuctionSnapshotRepository | Auction snapshots live in Redis rather than an EF aggregate store |
| Search | Elasticsearch and Redis adapters are not EF Core aggregate repositories |

These adapters may adopt a compatible shared policy later. Do not hide these
differences behind flags such as `saveImmediately` or `hardDelete` in the common
repository, and do not introduce a repository for every mapped table.

## Validation

Dedicated shared tests run against PostgreSQL for staged writes, caller-owned
transaction rollback, audit values, soft-delete idempotency, tracked/no-tracking
reads, query visibility, batch enumeration, paging, detached related entities,
and optimistic concurrency. Service tests preserve Auction/Job child behavior,
owner/requester audit policies, Payment's returned entities and concurrency,
Notification's staged writes and immediate template commits, and Storage's
domain deletion events and physical purge. The full backend suite exercises the
existing outbox, bidding, payments, and workflow recovery paths after migration.
