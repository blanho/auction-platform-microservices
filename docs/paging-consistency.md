# Paging guide and consistency audit

The ten backend services were reviewed against the shared
[paging implementation](../src/BuildingBlocks/BuildingBlocks.Application/Paging/QueryParameters.cs).
Paged relational queries now use its bounds and deterministic ordering. Public
response formats and omitted-input defaults still have the compatibility
differences listed below.

## Implementing a paged query

Use `QueryParameters<TFilter>` for typed filters, `FilterBuilder<T>` for conditional
predicates, and `SortMapBuilder<TEntity, TContract>` for an explicit sort allowlist.
See Identity's [filtering implementation](../src/Services/Identity/Identity.Application/Filtering)
for production request and sort-map examples.

Apply filters before count and paging. For read-only EF queries, use
`AsNoTracking`, a projection, and an explicit unique tie-breaker:

```csharp
return await _db.Users
    .AsNoTracking()
    .ApplyFiltering(filterBuilder)
    .ApplySorting(query, UserSortMap.Map, u => u.CreatedAt, tieBreaker: u => u.Id)
    .ToPaginatedResultAsync(query, u => new UserDto
    {
        Id = u.Id,
        Name = u.Name,
        Email = u.Email
    }, ct);
```

Here `query`, `filterBuilder`, `UserSortMap`, and `UserDto` represent the owning
service's query, filters, allowlist, and read model. Execute count and page reads
sequentially on one DbContext and pass cancellation through both. Application
projects with EF query interfaces can use `CountAsync`, `ApplyPaging`, and `Select`
directly; do not add an Infrastructure dependency just to use the EF helper.
Index filtering/ordering combinations based on actual query plans.

HTTP names and multi-sort binding depend on the endpoint's request DTO. Supporting
`Sorts` in an application query does not automatically expose it over HTTP. Worker
batches, category trees, lookups, autocomplete, and export streams have separate
contracts. Use `ApplyOffsetPaging` only for an existing offset/take API; it clamps
negative offsets to zero and applies the common size bounds.

### Register sort fields

Reference properties on the public read model and the entity with typed
expressions. This example assumes `UserDto` exposes the four registered fields:

```csharp
public static class UserSortMap
{
    public static IReadOnlyDictionary<string, Expression<Func<User, object>>> Map { get; } =
        new SortMapBuilder<User, UserDto>()
            .Add(field => field.Name, user => user.Name)
            .Add(field => field.Email, user => user.Email)
            .Add(field => field.CreatedAt, user => user.CreatedAt)
            .Add(field => field.Status, user => user.Status)
            .Build();
}
```

Keys come from the DTO property name, or its `[JsonPropertyName]` when present,
and are matched case-insensitively. Renaming/removing a referenced DTO or entity
property requires updating the expression at compile time. DTO aliases can stay
stable when entity properties change; a retained `[JsonPropertyName]` can also
preserve a public sort name when a DTO's CLR property is renamed. Database column
changes still require the appropriate EF mappings and migrations.

For existing API aliases that differ from response fields, define a small typed
sort contract in the service's `Application/Filtering` folder. Auction's `Price`
and `EndDate`, for example, map to `CurrentHighBid ?? ReservePrice` and
`AuctionEnd`. Preserve the public contract when persistence names change.

The registration is an explicit allowlist. Adding entity properties does not
automatically expose them as sort fields. Computed selectors are supported, such
as `.Add(field => field.FullName, user => user.FullName ?? string.Empty)` for an
explicit null-ordering policy. Expressions stay in the query for EF translation;
they are not compiled into per-row delegates.

Contract field types and stored key types can differ. A string `Status` in a DTO
can map directly to an entity enum; keep the enum selector to preserve database
ordering rather than converting it to display text. Nullable selectors retain
the provider's existing null ordering unless a deliberate coalesce is registered.
Search uses the same registration builder with Elasticsearch's native typed
field inference; relational query extensions are not applied to search requests.

`Build()` returns an immutable, case-insensitive
[`FrozenDictionary`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.frozen.frozendictionary-2?view=net-9.0).
Create it once in a static registration. Subsequent builder changes do not alter
the built map. Duplicate public names and non-property contract expressions fail
during registration. The sorting helper uses dictionary lookup first and retains
compatibility with existing case-sensitive maps.

### Bounds, sorting, and validation

`QueryParameters.Create`, `ApplyPaging`, the EF helpers, and `PaginatedResult.Empty`
share the same normalization: page values below 1 become 1, nonpositive page sizes
become 10, and sizes above 100 become 100. Services may retain explicit defaults
such as 20 or 50 when an input is omitted. Pages whose offsets would overflow
`int` are normalized to the largest page whose offset fits. Metadata and HTTP
pagination headers must use these effective values.

Numeric ranges are normalized rather than rejected; invalid numeric syntax still
fails HTTP binding. `QueryParametersValidator` runs after normalization, validates
sort-name length and malformed sort-list entries, and can compose typed filter
validation. Include it explicitly in the owning request validator: inheriting
`QueryParameters` does not register a validator. Shared validation messages and
field labels support English and Japanese.

Sort fields are trimmed and matched case-insensitively against the map. Unknown
fields are ignored; a list with no supported fields falls back to the default
sort. Repeated fields use their first direction. `Sorts` takes precedence over
`SortBy`. Always supply a default sort and a unique tie-breaker when paging; the
helper does not infer entity identifiers. Cache keys must include every filter
and ordering descriptor.

### Component locations

| Component | Location | Purpose |
|-----------|----------|---------|
| `QueryParameters` | [QueryParameters.cs](../src/BuildingBlocks/BuildingBlocks.Application/Paging/QueryParameters.cs) | Base class with page, pageSize, sorting |
| `QueryParameters<TFilter>` | [QueryParameters.cs](../src/BuildingBlocks/BuildingBlocks.Application/Paging/QueryParameters.cs) | Generic version with typed filter |
| `FilterBuilder<T>` | [FilterBuilder.cs](../src/BuildingBlocks/BuildingBlocks.Application/Filtering/FilterBuilder.cs) | Fluent filter building |
| `QueryExtensions` | [QueryExtensions.cs](../src/BuildingBlocks/BuildingBlocks.Application/Paging/QueryExtensions.cs) | IQueryable extensions (paging, sorting, filtering) |
| `SortMapBuilder<TEntity, TContract>` | [SortMapBuilder.cs](../src/BuildingBlocks/BuildingBlocks.Application/Paging/SortMapBuilder.cs) | Typed public-field registration and immutable sort maps |
| `EfCoreQueryExtensions` | [EfCoreQueryExtensions.cs](../src/BuildingBlocks/BuildingBlocks.Infrastructure/Extensions/EfCoreQueryExtensions.cs) | EF Core specific extensions |
| `PaginatedResult<T>` | [PaginatedResult.cs](../src/BuildingBlocks/BuildingBlocks.Application/Abstractions/PaginatedResult.cs) | Standard response |
| `QueryParametersValidator` | [QueryParametersValidator.cs](../src/BuildingBlocks/BuildingBlocks.Application/Paging/QueryParametersValidator.cs) | Input validation |
| `PaginationDefaults` | [PaginationDefaults.cs](../src/BuildingBlocks/BuildingBlocks.Application/Constants/PaginationDefaults.cs) | Defaults: page=1, pageSize=10, maxPageSize=100 |

## Service coverage

| Service | Paged reads | Implementation after the audit | Compatibility details |
|---|---|---|---|
| Auction | Auctions, reviews, bookmarks | Shared parameters/EF helpers, sort maps, ID tie-breakers; complete filter and multi-sort cache keys | General default 10; featured default 8. Uses `orderBy`/`descending`. |
| Bidding | Auction bids, bidder bids, history, winning bids, auto-bids | Shared bounds and whitelisted ordering; winning bids support the same ordering path | History/winning/auto-bid HTTP requests default to 20; other application queries default to 10. Paid filtering remains before count and paging. |
| Catalog | Brands and the category repository's paged method | Shared parameters/helpers and unique ordering | Brands default 10. Category trees and lookup lists remain unpaged by design. Brand display-order sorting retains name as a secondary sort. |
| Identity | User listing through CQRS and the user service | One user-service pipeline with `Filtering/UserFilter`, shared `IFilter<T>`/query parameters, user sort map, and ID tie-breaker | Default 10, `sortBy`/`sortDescending`. Role names resolve to IDs before filtering; unknown roles return an empty page. Roles are fetched only for selected users. |
| Payment | Buyer/seller/admin orders, wallet transactions | Shared parameters, unique ordering, cancellation through count and page reads | Default 10. Existing HTTP responses remain arrays with `X-Total-Count`, `X-Page`, `X-Page-Size`; headers now reflect effective bounds. |
| Notification | User/admin notifications, records, templates | Shared helpers; unread filtering before count/page; summaries fetch only the recent page | Existing endpoint defaults of 10/20 are retained. Omitted page values work for templates and per-user records. Legacy offset/take reads use the shared adapter. |
| Analytics | Reports, audit logs, paged platform settings | Shared parameters/helpers and ID tie-breakers | Reports/audit logs default 10; dashboard feeds retain their own limit default. Summary/statistics responses are not pages. |
| Jobs | Jobs, items, execution history | Shared EF helper, unique ordering, filtered counts | Jobs default 20, items/history 50. History retains `entries`/`totalLogEntries` and adds effective `page`, `pageSize`, `totalPages`, `hasPreviousPage`, `hasNextPage`. |
| Search | Elasticsearch auction search | Shared numeric normalization, existing score/field plus ID ordering, explicit result-window guard | Default 20 or configured search default; maximum 100 or a lower configured cap. Retains `results`, long `totalCount`, facets, timing, and `hasMore`. |
| Storage | No public paginated list endpoint | Not applicable | File operations and cleanup batches use their own bounded contracts. |

Identity groups `UserFilter`, `UserQueryParams`, and `UserSortMap` under
`Identity.Application.Filtering`. `UserFilter` implements the shared
`IFilter<ApplicationUser>` contract. The user service resolves the public `Role`
name into the filter's `RoleId`, then applies all filters before counting and
paging. The CQRS handler delegates to that same service pipeline. HTTP query
parameter names and the paged response shape are unchanged.
Identity's sort map uses the common `SortMapBuilder<ApplicationUser, AdminUserDto>`
to derive public names from typed DTO members and map them to typed entity
expressions. Its immutable lookup retains the existing names and null ordering;
DTO JSON aliases can preserve those names across CLR property renames.

## Typed sort registration coverage

All request-selected sort registrations now use the common `SortMapBuilder`:
13 immutable maps across eight services. Public keys come from typed DTO members
or dedicated sort contracts where legacy aliases differ from response properties.

| Service | Registrations | Preserved behavior |
|---|---|---|
| Identity | Users | Public user fields, explicit null policies, hidden fields excluded |
| Auction | Auctions, bookmarks, reviews | `price`/`enddate` aliases, computed current price, nested bookmark values, review title fallback |
| Bidding | Bids, auto-bids | Native enum status ordering and nullable last-bid ordering |
| Payment | Orders, wallet transactions | Status/type keys, nullable payment dates, existing default sorts |
| Notification | Notification records | Status/channel fields and nullable sent dates |
| Analytics | Reports, audit logs | Native priority/status/type keys and existing audit fields |
| Catalog | Brands | Display-order then name ordering; default remains ascending regardless of requested direction |
| Search | Auction documents | `price`/`endtime`/`bids`/`created` aliases; relevance remains score descending, date descending, ID ascending |

Catalog now honors application-level multi-sort descriptors through the shared
sorting helper. Its legacy single-field ordering and unknown-field fallback are
unchanged. Search resolves registered expressions with Elasticsearch field
inference rather than maintaining a second string mapping of document properties.
Jobs and Storage have no request-selected sort maps; their fixed typed ordering
and bounded worker contracts remain appropriate.

The allowlist stays explicit: new entity properties are not exposed automatically.
DTO display types may differ from stored keys, so registration preserves native
enum and nullable expressions. Property removal or rename is checked by the C#
compiler; schema/index changes still require EF migrations or Elasticsearch
mapping updates. Existing sort directions, ID tie-breakers, and public aliases
remain compatible.

Paged relational responses use `PaginatedResult<T>` internally. Most APIs expose
that object directly. Payment, Search, and Job history retain their existing
wrappers so clients can continue using their current adapters. A single HTTP
response shape and identical sort parameter names would require a coordinated
API/client migration; those contracts are not yet identical.

## Corrections made

- Replaced 12 repository methods with hand-written page arithmetic with the
  shared EF helper. Page 0, invalid sizes, and excessively large page numbers
  now return metadata matching the executed query.
- Prevented signed integer offset overflow in `QueryParameters`, `ApplyPaging`,
  and both EF projection/entity helpers. Size normalization now consistently
  uses 10 for nonpositive sizes and caps oversized pages at 100.
- Added unique ID ordering to paged relational reads. An unsupported multi-sort
  list now uses the default ordering, and duplicate sort fields are ignored.
- Fixed unread notifications filtering after paging, which previously produced
  underfilled pages and totals containing read notifications.
- Changed notification summaries to database counts plus a bounded recent page,
  avoiding loading every notification for a user.
- Made wallet headers use normalized response values and made page parameters
  optional where endpoint binding had required them despite configured defaults.
- Removed endpoint-specific Bidding clamps and the featured-auction reset-to-8
  rule for large sizes. Featured auctions now request the supported `enddate`
  sort so the feed orders by auction end time.
- Included every filter and multi-sort descriptor in auction list cache keys,
  with a new key version to avoid reusing pages produced under the old ordering.
- Passed buyer/seller order cancellation through handlers and repository queries.
- Enforced Search's existing `MaxResultWindow` before contacting Elasticsearch,
  returning HTTP 400 with an English/Japanese diagnostic when exceeded. Search
  now honors its configured default when page size is omitted.
- Added shared labels for paging/sorting validation and checked malformed
  multi-sort descriptors.

## Guarantees and limits

`page` is one-based. Nonpositive pages become 1; nonpositive page sizes become
10. Explicit endpoint defaults remain meaningful when the parameter is omitted.
Counts apply all filters before paging, and the effective page and size are
returned to clients. Invalid numeric syntax is still rejected by HTTP binding.

Offset paging does not guarantee a frozen dataset across requests. Counts and
items are separate database reads, so concurrent writes can change them between
queries. A unique tie-breaker prevents nondeterministic ties; cursor/snapshot
pagination is the next step for workloads needing stable or deep traversal.
Elasticsearch's configured result window must match the index's supported
window. Requests beyond that window need a future cursor/search-after contract.

The common query validator is reusable rather than automatically attached to
every CQRS request. Numeric normalization is enforced by the shared query
objects/helpers even when a request does not run that validator. Endpoint-specific
filter validation remains owned by the service.

Regression coverage includes shared normalization and sorting, PostgreSQL
projection/count/page behavior, auction cache isolation, unread notification
counts and user isolation, SQL-bounded summaries, job history metadata, order
cancellation, tied winning-bid ordering, and serialized Elasticsearch requests.
An architecture rule prevents repository page-offset arithmetic from drifting
back into services.
