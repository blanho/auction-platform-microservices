# Auction service maintenance guide

Auction owns listings, auction state, bookmarks, reviews, import/export work, and
its participants in the buy-now workflow. Its integration contracts live in
`Auction.Contracts`; persistence and message consumers live in `Auction.Infrastructure`.

## Finding the implementation

| Concern | Entry point |
|---|---|
| Create, edit, delete, and change listing state | `Auction.Api/Endpoints/Auctions/AuctionCrudEndpoints.cs` |
| Start a purchase and read its status | `Auction.Api/Endpoints/Auctions/AuctionPurchaseEndpoints.cs` |
| Browse and query listings | `Auction.Api/Endpoints/Auctions/AuctionQueryEndpoints.cs` |
| Use cases | `Auction.Application/Features/{Area}/{Operation}` |
| Public live-auction browse cache | `Auction.Application/Services/CachedAuctionBrowseService.cs` |
| Import validation and mapping | `Auction.Application/Features/Auctions/ImportAuctions/ImportAuctionRowValidator.cs` and `ImportAuctionMapper.cs` |
| Export row mapping shared by direct and queued exports | `Auction.Application/Features/Auctions/ExportAuctions/ExportAuctionMapper.cs` |
| State transitions and purchase reservations | `Auction.Domain/Entities/Auction.cs` |
| SQL filtering, ordering, paging, and tracked updates | `Auction.Infrastructure/Persistence/Repositories/AuctionRepository.cs` |
| Import receipts, batch progress, and completion messages | `Auction.Infrastructure/Messaging/Consumers/ImportAuctionsConsumer.cs` and `ImportAuctionsBatchConsumer.cs` |
| Seller deletion, suspension, roles, and username changes | `Auction.Infrastructure/Messaging/Consumers/UserDeletedConsumer.cs`, `UserSuspendedConsumer.cs`, `UserRoleChangedConsumer.cs`, and `UserUpdatedConsumer.cs` |
| Shared bidder cancellation notifications | `Auction.Infrastructure/Messaging/AuctionCancellationNotifications.cs` |

## Keeping changes readable

Keep handlers focused on the sequence of the use case. Extract a helper when it
names a distinct responsibility, such as updating item details, resolving catalog
references, or starting a purchase. Avoid generic pipelines that hide domain rules
or side effects. Use named arguments for long command/data mappings.

Import rules and entity creation have one implementation shared by the direct
command handler and both consumers. Map message payloads into application rows at
the consumer boundary. Preserve the message's row numbers; direct imports use the
row's one-based position. Validation returns raw diagnostics, and the response or
event boundary applies the request culture. Pass the current time explicitly to
row validation so date and manufacturing-year boundaries can be tested.

## Workflow invariants

- A purchase retry must resolve its existing purchase before checking whether the
  auction is available for a new purchase. Correlation ownership includes both
  buyer and auction. The distributed lock must cover the lookup and all writes.
- A new purchase reserves the auction, records the purchase, publishes the saga
  start, updates the aggregate, publishes the audit, and saves. Keep cancellation
  and concurrency handling intact when changing this sequence.
- Updates collect modified fields in the order they are applied. Preserve the
  original audit snapshot and validate catalog references before publishing the
  update event, audit, or save.
- Import counts represent rows, not the number of validation messages. Keep
  insert/save/checkpoint ordering, batch sizes, receipt keys, progress identifiers,
  and completion semantics intact when changing import processing.
- Bulk activation and deactivation use the aggregate's `TryActivate` and
  `TryDeactivate` methods. Activation requires an eligible state and an end time
  strictly after the supplied clock value; reserved purchases and terminal states
  must be left alone. Preserve update counts, batch saves, and request order.
- Direct and queued exports share row mapping. Queued exports use storage's
  returned metadata in completion events and distinguish standalone job progress
  from parent-item results. Upload failures must remain retryable without a
  completed receipt; unsupported formats follow the explicit failure/completion
  path.
- Browse filters run before count, sorting, and paging. Keep tracking behavior
  explicit: read-only browse results use `AsNoTracking`; write repositories retain
  the aggregate tracking required by outbox persistence.
- Cancellation uses the aggregate's shared transition guard and status-change
  event. Preserve event payloads and public contracts during internal refactors.
- Seller-event notifications retain one message per affected auction and group
  titles by bidder in encounter order. Suspension and role-change consumers save
  before queuing notifications. Account deletion queues notifications before its
  final save, which also persists finished-auction anonymization. Keep these
  boundaries explicit when changing the shared notification collector.
- Username synchronization updates only changed seller/winner names and saves
  once after both sets have been processed. A replay with matching names performs
  no writes or saves.

## Verification

Run the service's test projects and repository architecture checks:

```bash
for project in src/Services/Auction/tests/*/*.csproj src/Tests/Architecture.Tests/Architecture.Tests.csproj; do
  dotnet test "$project" -c Release -m:1 || exit 1
done
```

Run the repository's analyzer check before merging:

```bash
dotnet build auction.sln -c Release --no-restore --no-incremental --warnaserror -m:1
```

PostgreSQL and RabbitMQ tests need disposable services. See
[backend testing](../../../docs/testing.md) for the provider environment variables
and the full test runner. A run with skipped provider tests does not verify actual
SQL execution, transaction persistence, or broker delivery.
