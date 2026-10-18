# Paging consistency audit

The ten backend services were reviewed against the shared
[paging implementation](../src/BuildingBlocks/BuildingBlocks.Application/Paging/README.md).
Paged relational queries now use its bounds and deterministic ordering. Public
response formats and omitted-input defaults still have the compatibility
differences listed below.

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
