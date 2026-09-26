# Backend refactoring review

The later [file-level backend audit](backend-exhaustive-review.md) and [complete file ledger](backend-file-audit.csv) extend this focused refactoring review.

This pass extends the earlier Bidding and Identity cleanup to Analytics, Auction, Catalog, Job, Notification, Payment, Search, and Storage. It inventories backend C# sources and reviews service/application workflows, repositories, batching, and shared infrastructure hotspots. Changes are targeted; unchanged files have not all received an exhaustive line-by-line audit.

## Findings before refactoring

- Readability: nested bid/status decisions, resource acquisition hidden inside LINQ, and category ancestor validation mixed into update orchestration.
- Duplication: five custom auction batch helpers, winning-bid selection, permission queries, notification persistence, and job batch flushing.
- Complexity: search ordering spread over several tiny methods; identical CSV/Excel report branches.
- Risk: broad exception catches, concurrent use of scoped notification dependencies, and several existing output/data inconsistencies listed below.
- Performance: repeated dictionary lookups in analytics, unused date calculations, and custom batching allocations. Database query counts, tracking, and materialization boundaries are intentionally preserved.

## Service coverage and complete refactored code

The links below point to the complete source files in the working tree.

| Area | Changes and preserved behavior |
| --- | --- |
| Analytics | [FactAuctionRepository](../src/Services/Analytics/Analytics.Infrastructure/Repositories/FactAuctionRepository.cs): use one lookup per listing, retain fallback prices and sorting, remove unused date calculations. |
| Auction | Replace private batching implementations with `Enumerable.Chunk`; preserve fixed sizes, row order, eager materialization where previously used, saves, checkpoint placement, and cancellation checks. Simplify nested deactivation logic. Full files are listed below. |
| Bidding | [BidPlacementService](../src/Services/Bidding/Bidding.Application/Services/BidPlacementService.cs): early return and shared unsuccessful responses. [BidRepository](../src/Services/Bidding/Bidding.Infrastructure/Repositories/BidRepository.cs): shared winning-bid selection with the same acceptance filters and amount/time tie-breaking. Completed in the first pass. |
| Catalog | [UpdateCategoryCommandHandler](../src/Services/Catalog/Catalog.Application/Features/Categories/UpdateCategory/UpdateCategoryCommandHandler.cs): separate ancestor traversal from update orchestration; retain validation order, cycle detection, error codes, and query sequence. |
| Identity | [RolePermissionService](../src/Services/Identity/Identity.Application/Services/RolePermissionService.cs): reuse the enabled-permission query while preserving public async exception behavior. Completed in the first pass. |
| Job | [JobItemRepository](../src/Services/Job/Job.Infrastructure/Persistence/Repositories/JobItemRepository.cs): one implementation of add/save/clear for full and trailing batches; keep per-item timestamps and save frequency. |
| Notification | [NotificationSender](../src/Services/Notification/Notification.Application/Services/NotificationSender.cs): share record persistence outside channel catch blocks; name the SMS preview limit and parsed user ID. Delivery exceptions and persistence exceptions retain their different handling. |
| Payment | [OrderReportGenerator](../src/Services/Payment/Payment.Infrastructure/Services/OrderReportGenerator.cs): name the report query cap, combine identical format branches, and clarify report-buffer naming. Preserve report bytes, culture handling, filtering, and failure results. |
| Search | [AuctionSearchService](../src/Services/Search/Search.Infrastructure/Services/AuctionSearchService.cs): keep ordering construction together; retain fallback relevance, direction parsing, and stable ID tie-breaking. |
| Storage | [FileEndpoints](../src/Services/Storage/Storage.Api/Endpoints/Files/FileEndpoints.cs): explicit loop for acquiring and registering streams, with the existing `try/finally` disposal order and request behavior. |
| Shared building blocks | Reviewed paging/sorting, validation, slug generation, and storage touchpoints. No edits in this pass: public normalization and exception semantics remain unchanged. |
| Gateway | Reviewed rate-limit setup and rejection formatting. No edits: partition keys, limiter policies, and response format remain unchanged. |
| Orchestration | Reviewed buy-now saga transitions and auction-completion scheduling. No edits: event contracts, timing, and compensation behavior remain unchanged. |

Complete Auction files:

- [ImportAuctionsCommandHandler](../src/Services/Auction/Auction.Application/Features/Auctions/ImportAuctions/ImportAuctionsCommandHandler.cs)
- [ImportAuctionsConsumer](../src/Services/Auction/Auction.Infrastructure/Messaging/Consumers/ImportAuctionsConsumer.cs)
- [ImportAuctionsBatchConsumer](../src/Services/Auction/Auction.Infrastructure/Messaging/Consumers/ImportAuctionsBatchConsumer.cs)
- [BulkUpdateAuctionsConsumer](../src/Services/Auction/Auction.Infrastructure/Messaging/Consumers/BulkUpdateAuctionsConsumer.cs)
- [AuctionBulkRepository](../src/Services/Auction/Auction.Infrastructure/Persistence/Repositories/AuctionBulkRepository.cs)

## Validation

- `dotnet build auction.sln --no-restore --verbosity quiet -m:1 -p:UseSharedCompilation=false`: passed, zero warnings and errors.
- Full solution test run: 151 passing tests across 25 test projects.
- Subsequent Auction application run, including five new batching cases: eight passing tests. This brings the distinct passing total to 156.
- `git diff --check`: passed.
- No dependencies, public APIs, database migrations, or configuration changes were introduced.

Regression coverage added in this pass:

- [ImportBatchingTests](../src/Services/Auction/tests/Auction.Application.Tests/ImportBatchingTests.cs): empty input, single/full/trailing batches, resume checkpoints, row order, cancellation-token forwarding, and checkpoint-after-save ordering.
- [NotificationSenderTests](../src/Services/Notification/tests/Notification.Application.Tests/NotificationSenderTests.cs): SMS preview boundaries, successful/failed delivery persistence, invalid-user-ID fallback, save order, and propagation of persistence failures.
- [SearchSortRequestTests](../src/Services/Search/tests/Search.Infrastructure.Tests/SearchSortRequestTests.cs): inspect serialized Elasticsearch requests for supported fields, case handling, default direction, fallback relevance, and tie-breakers.

Existing Catalog hierarchy tests passed after the extraction. The earlier [Bidding regression tests](../src/Services/Bidding/tests/Bidding.Application.Tests/BidPlacementServiceTests.cs) also passed.

Tests use local fakes/in-memory transport where applicable. Live PostgreSQL, Elasticsearch, message-broker, payment-provider, and notification-provider integration behavior was not verified. Several services currently have only assembly-reference smoke tests; a green solution test run does not imply comprehensive behavioral coverage.

## Behavior issues kept separate from refactoring

These findings need focused fixes and regression/integration tests; this pass does not silently change them.

1. [OrderReportGenerator](../src/Services/Payment/Payment.Infrastructure/Services/OrderReportGenerator.cs) labels plain-text bytes as PDF, produces CSV for Excel requests, and uses summary rows for some specialized report headers. The report also requests a capped first page before applying buyer/seller filters.
2. [FactAuctionRepository](../src/Services/Analytics/Analytics.Infrastructure/Repositories/FactAuctionRepository.cs) computes both ending-today and ending-this-week metrics from the same live/unended count, without checking an end-date window. Top-auction bid counts load aggregate data for all auctions before selecting the top results.
3. [AuctionBulkRepository.CountByCorrelationIdAsync](../src/Services/Auction/Auction.Infrastructure/Persistence/Repositories/AuctionBulkRepository.cs) counts all non-deleted auctions rather than filtering by its correlation argument. Changing it needs a defined correlation-storage contract.
4. [ImportAuctionsBatchConsumer](../src/Services/Auction/Auction.Infrastructure/Messaging/Consumers/ImportAuctionsBatchConsumer.cs) publishes completion from the final batch using that batch's counts. Whole-import totals need separate review, including out-of-order batch delivery.
5. [SendBulkNotificationConsumer](../src/Services/Notification/Notification.Infrastructure/Messaging/Consumers/SendBulkNotificationConsumer.cs) runs recipients and channels concurrently while sharing scoped repository/unit-of-work dependencies. Confirm service lifetimes and DbContext usage before adjusting concurrency. Its broad catches can also treat cancellation as recipient failure.
6. [UploadMultipleFilesCommandHandler](../src/Services/Storage/Storage.Application/Features/Files/UploadMultipleFiles/UploadMultipleFilesCommandHandler.cs) catches upload exceptions broadly; rollback uses the original cancellation token and may itself throw, replacing the original save exception. Altering these semantics requires explicit failure/cancellation tests.
7. Some batch repository methods enumerate an `IEnumerable` for auditing and again for persistence. Support for single-use inputs should be decided and tested before changing enumeration timing.

The pre-existing edits to `LocalFileStorageService.cs` and `LocalFileStorageServiceTests.cs` were left untouched. Frontend code was outside this pass.
