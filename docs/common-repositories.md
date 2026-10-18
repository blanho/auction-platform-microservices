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
allows an explicit detached-graph policy.

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

Auction and Job explicitly preserve their previous detached-graph update behavior.
For an already tracked aggregate, normal EF change detection persists actual
child edits without marking every loaded child modified. This avoids accidental
updates to unchanged related records while retaining the unit-of-work boundary.

Other services were reviewed but are not forced into this audited soft-delete
contract:

| Area | Reason |
|---|---|
| Identity | ASP.NET Identity's UserManager/RoleManager own account persistence and string keys |
| Payment | Financial workflows have their own audit, return-value, locking, and transaction contracts |
| Notification | Some legacy repositories save immediately or physically delete; changing them requires a separate unit-of-work migration |
| Analytics | Fact/report/settings models use different keys and persistence policies rather than `AggregateRoot` |
| Storage | Physical file removal and cleanup require explicit hard-delete behavior |
| Job items, checkpoints, workflow receipts | Child/bulk/idempotency persistence has specialized behavior |
| Search | Elasticsearch and Redis adapters are not EF Core aggregate repositories |

These adapters may adopt a compatible shared policy later. Do not hide these
differences behind flags such as `saveImmediately` or `hardDelete` in the common
repository, and do not introduce a repository for every mapped table.

## Validation

Dedicated shared tests run against PostgreSQL for staged writes, caller-owned
transaction rollback, audit values, soft-delete idempotency, tracked/no-tracking
reads, query visibility, batch enumeration, paging, detached related entities,
and optimistic concurrency. Service tests preserve Auction/Job child behavior
and owner/requester audit policies. The full backend suite exercises the existing
outbox, bidding, payments, and workflow recovery paths after migration.
