# Background jobs

The Job service persists work items, dispatches commands over RabbitMQ, and tracks
completion and failures. Domain operations execute in the owning service. Quartz
jobs such as auction activation and orphan-file cleanup run separately in their
services.

## Operations available through Jobs

An administrator can create these item-based jobs with `POST /api/v1/jobs/`:

| Job type | Executor | Unit of work |
|---|---|---|
| `AuctionImport` | Auction | One auction import payload |
| `BulkAuctionUpdate` | Auction | One bulk auction update command |
| `DataExport` | Auction | One auction export command |
| `ReportGeneration` | Payment | One order report request |
| `BulkNotification` | Notification | One bounded recipient batch |
| `ImageProcessing` | Storage | Validate and create a thumbnail for one owned image |
| `BulkFileUpload` | Storage | Finalize one previously reserved, uploaded file |
| `SearchReindex` | Search | Reindex 1–500 existing auction documents in place |
| `AnalyticsAggregation` | Analytics | Replace daily summaries for 1–31 inclusive UTC dates |

`ReportGeneration` and `BulkNotification` can also originate from their existing
service endpoints. Those standalone operations retain their correlation-based
progress protocol. Jobs-dispatched operations report to the original job item
and do not create a second tracking job.

## Create a report job

Submit the following JSON body with an administrator's bearer token. The example
uses a fixed correlation ID for illustration; use a unique value for each new job.
The requester is taken from the authenticated identity.

```json
{
  "type": "ReportGeneration",
  "correlationId": "order-summary-2026-10-17",
  "payloadJson": "{}",
  "totalItems": 1,
  "items": [
    {
      "sequenceNumber": 1,
      "payloadJson": "{\"reportType\":\"OrderSummary\",\"format\":\"Csv\",\"culture\":\"en-US\"}"
    }
  ]
}
```

Each item's `payloadJson` contains a serialized
[`GenerateOrderReportCommand`](../src/Services/Payment/Payment.Contracts/Commands/GenerateOrderReportCommand.cs).
Supported report types are `OrderSummary`, `PaymentTransactions`, `SellerPayout`,
`BuyerPurchaseHistory`, `RevenueReport`, and `RefundReport`; formats are `Pdf`,
`Excel`, and `Csv`. Optional filters include `statusFilter`, `buyerIdFilter`,
`sellerIdFilter`, `startDate`, and `endDate`. If both dates are supplied, the end
must be later than the start. The item completes only after report storage
succeeds. The existing `OrderReportGeneratedEvent` carries the download URL.

## Create a notification job

Use `type: "BulkNotification"` with the same request envelope. Each item's
`payloadJson` is a serialized
[`SendBulkNotificationCommand`](../src/Services/Notification/Notification.Contracts/Commands/SendBulkNotificationCommand.cs).
For example, serialize this object into the string field:

```json
{
  "templateKey": "announcement",
  "title": "Auction update",
  "message": "New auctions are available.",
  "channels": ["InApp"],
  "batchSize": 100,
  "recipients": [
    { "userId": "00000000-0000-0000-0000-000000000001" }
  ],
  "globalParameters": {}
}
```

Replace the example recipient with a real user ID. Supported channel names are
`Email`, `Sms`, `Push`, and `InApp` (case-sensitive). Every recipient must have an
email address when selecting `Email`, or a phone number when selecting `Sms`.
Recipient IDs must be unique within an item; each item accepts 1–10,000 recipients
and a batch size of 10–500. `templateKey`, `title`, and `message` are required.
`scheduledAt` is not supported by this execution path; submit the job when due.
The current email/SMS/push sender implementations are placeholders; this change
adds job execution, not production delivery-provider integrations.

A notification item completes only when all its recipients succeed. Failed
recipients cause the item to fail, allowing an operator to retry it. Successful
recipient/channel deliveries use the stable job item ID for deduplication across
attempts. The current Redis receipts expire after 24 hours: retries after expiry,
Redis data loss, or a crash between sending and recording a receipt can resend a
notification. This is not an exactly-once delivery guarantee.

### Notification validation messages

Bulk notification validation returns a stable message key and an array of invariant
string arguments. The Notification consumer publishes these as `errorCode` and
`errorArguments` along with a neutral English `errorMessage` for compatibility.
Both single-item and batch result consumers persist all three fields on the item.

For example, an invalid batch size yields:

```json
{
  "errorCode": "Notification.Bulk.BatchSizeOutOfRange",
  "errorArguments": ["10", "500"],
  "errorMessage": "BatchSize must be between 10 and 500."
}
```

On `GET /api/v1/jobs/{id}/items`, `errorMessage` is formatted from the key and
arguments in the reader's `Accept-Language` culture. English/Japanese responses
use the same code and arguments, and do not depend on matching stored English
wording. Existing rows without a code still use legacy diagnostic translation.
Unknown keys or incomplete arguments fall back to the stored diagnostic.
Successful completion, skipping, and operator retry clear obsolete error metadata.
Delivery failures use the same protocol with failed/total recipient counts.

## Storage jobs

Both storage job types use the same create-job envelope above, with one file per
item. Set `type` to `ImageProcessing` or `BulkFileUpload`, and serialize this object
into each item's `payloadJson`:

```json
{
  "fileId": "00000000-0000-0000-0000-000000000001"
}
```

Use the ID of a file owned by the authenticated administrator creating the job.
The dispatcher supplies the requester and tracking IDs. Storage resolves the
object key from its own database; supplied URLs, paths, owner IDs, or file metadata
cannot redirect the work. Private report files are excluded.

### Image processing

Confirmed JPEG, PNG, GIF, and WebP uploads already request `ImageProcessing` jobs
automatically when they have an owner. Existing queued payloads containing extra
file metadata remain readable; only `fileId` is used. An administrator can also
submit a job for their own confirmed images.

The executor checks the configured file-size limit and actual byte length, rejects
corrupt images and mismatched content types, and limits decoding to 16 megapixels.
It decodes the first frame of animated images, applies EXIF orientation, and makes
a JPEG thumbnail that fits within 320 × 320 pixels without upscaling. Transparent
areas become white; source EXIF/GPS metadata is not copied to the thumbnail. The
original object is not modified.

SHA-256, oriented dimensions, and the thumbnail are saved atomically on the
existing file record. `GET /api/v1/files/{fileId}` exposes `checksum` and an `image`
object containing `width`, `height`, and `thumbnailDataUrl`. The thumbnail is
stored as a data URL in PostgreSQL metadata, capped at 128 KiB of JPEG data before
base64 encoding. This avoids separate thumbnail objects and their cleanup, at the
cost of larger metadata responses and database records. Replays with the same
checksum and processor version reuse the saved result.

Image decoding uses [SkiaSharp](https://www.nuget.org/packages/SkiaSharp/4.153.1),
with matching Linux native assets included for container deployment.

### Bulk upload finalization

`BulkFileUpload` completes the existing direct-to-storage upload flow; it does not
transport binary files through RabbitMQ or download arbitrary URLs:

1. Reserve each upload with `POST /api/v1/files/presigned-upload` as the same owner.
2. Upload the bytes using the returned presigned URL and headers before expiry.
3. Submit one `BulkFileUpload` job item per returned file ID.
4. Check each item's result; retry failed items only after fixing the underlying
   upload. Expired reservations require a new reservation and job item.

Each item validates ownership, allowed file type, reservation expiry, and actual
byte length before changing `Pending` to `Ready`. Finalization raises the usual
file-uploaded event, which can enqueue image processing. Row locking prevents
concurrent confirmations from emitting the event twice; a ready file can be
confirmed again without another event. Local storage does not support presigned
uploads, so use its existing multipart upload endpoint instead.

Storage errors and cancellation propagate for broker retry without marking a
successful item. Invalid or missing files produce terminal item failures. Both
consumers use the Storage database outbox for their results.

## Search reindex jobs

Use `type: "SearchReindex"`, with a serialized
[`ReindexAuctionsCommand`](../src/Services/Search/Search.Contracts/Commands/ReindexAuctionsCommand.cs)
as each item's `payloadJson`:

```json
{
  "auctionIds": ["00000000-0000-0000-0000-000000000001"]
}
```

Supply 1–500 distinct, nonempty auction IDs per item. The executor uses only the
configured Search index; a payload cannot select another index or run a script.
It reindexes the stored Elasticsearch `_source` using the current mapping, which
is useful after compatible mapping additions. Apply the mapping change before
submitting the job. The implementation uses Elasticsearch's
[update-by-query API](https://www.elastic.co/guide/en/elasticsearch/reference/8.19/docs-update-by-query.html)
without a source-editing script, waits for completion, and refreshes the index.
Internal version checks abort on concurrent changes rather than overwrite newer
bid state. Conflicts, timeouts, and shard failures propagate for broker retry;
a successful result requires every requested document to have been updated.

This is an in-place maintenance operation on existing projections. It does not
delete the live index, change aliases, recover missing documents, or fetch canonical
auction data. Missing documents fail the item and require separate projection
recovery. A full rebuild from Auction data remains follow-up work and needs a
snapshot/replay protocol to preserve updates arriving during the rebuild.

Split larger sets into multiple items. Replaying an item is safe for document
contents, but increments Elasticsearch versions and repeats indexing work. Partial
updates are not rolled back when a later document conflicts; retrying the whole
bounded batch reprocesses the current sources. Search buffers result messages in
an in-memory outbox; a crash after indexing can cause the work to run again.

## Analytics aggregation jobs

Use `type: "AnalyticsAggregation"`, with a serialized
[`AggregateDailyStatsCommand`](../src/Services/Analytics/Analytics.Contracts/Commands/AggregateDailyStatsCommand.cs)
as each item's `payloadJson`:

```json
{
  "startDate": "2026-10-01",
  "endDate": "2026-10-17"
}
```

Both dates are required, inclusive, and interpreted as UTC date keys already stored
on the facts. Each item accepts 1–31 dates and cannot end in the future. Use
multiple items for larger periods; successful periods are retained on job retry.

The executor rebuilds `analytics.daily_auction_stats`, `daily_bid_stats`, and
`daily_revenue_stats` from this service's fact tables. These are physical tables
created by the existing migration, despite their read-side EF `ToView` mappings.
It replaces the selected period in one transaction, reads all three fact sources
from one statement snapshot, and serializes aggregation writers with table locks.
Dashboard reads and fact ingestion can continue. Empty days remove old summary
rows rather than creating artificial zero-valued events. Completion is committed
through the Analytics database outbox in the same transaction as the summaries.

The existing daily-stats API exposes the results. Metric definitions are:

- Auction rows count events by date and event type. Sale amounts and sold/unsold
  counts come only from `Finished` events, so a preceding `BuyNowExecuted` event
  does not count the same sale again. An unfinished buy-now workflow contributes
  revenue only when its `Finished` fact arrives.
- Bid rows summarize stored bid facts across all statuses. These are fact counts,
  not a reconstructed count of distinct accepted bids; the current fact model
  lacks a separate logical bid ID for every event source.
- Payment rows count events by date and type. Revenue includes only paid,
  non-refunded `PaymentCompleted` facts. Creation, shipment, and delivery events
  contribute no revenue. Refund totals use facts flagged `IsRefunded`; the current
  consumers do not ingest refund events, so this job alone cannot fill that gap.
- Money values retain the existing fact model's units, with no currency conversion
  or currency dimension in the daily DTOs. They must not be treated as consolidated
  multi-currency accounting totals. Distinct seller counts exclude missing IDs.

Replays replace totals rather than add to them. Facts committed after the snapshot
appear on a later run; rerun recent periods to include late events. Aggregation
uses the facts already ingested, and does not repair missing or duplicate source
events. No new recurring schedule is installed by this stage.

## Progress, failures, and retries

- `GET /api/v1/jobs/{id}` returns aggregate progress.
- `GET /api/v1/jobs/{id}/items` returns individual item status and error messages.
- `POST /api/v1/jobs/{id}/retry` requeues failed items of a failed or partially
  failed job. Successful items are retained.
- `POST /api/v1/jobs/{id}/cancel` stops further job dispatch; it does not recall
  commands that are already executing in another service.

Jobs stamps the requester, parent job/item IDs, and attempt number from persisted
state, overriding payload-supplied values. Stale attempt results and duplicate
terminal results are ignored. Invalid payloads fail the item; service exceptions
use broker retry policies, then a fault consumer records a terminal item failure.
The report and notification paths require an explicit job retry after that point.

Before enabling a job type, deploy its owning service with support for the command
route and parent-item tracking.

Key-based notification errors require Job migration
`20261018023620_LocalizedJobItemErrors`, which adds nullable `ErrorCode` and an
`ErrorArguments` text array to `JobItems`. Existing rows receive an empty argument
array and retain their original diagnostic. Apply the migration before deploying
the updated Job API/worker, then deploy the updated Notification consumer. The Job
service's `--migrate-only` mode supports deployment migrations; configure its usual
database connection. Older producers remain readable, and older consumers can use
the English fallback while they are being upgraded.

## Remaining implementation stages

Only the nine types listed above currently have item dispatch support. The other
enum values are declarations, not executable workflows through Jobs.

| Stage | Types / service | Work still required |
|---|---|---|
| 1 — implemented | `ReportGeneration`, `BulkNotification` | Parent-item dispatch, completion, fault handling, and regression coverage |
| 2 — implemented | `ImageProcessing`, `BulkFileUpload` / Storage | Image validation, checksum and thumbnails, owned upload finalization, outbox results, and replay protection |
| 3 — implemented | `SearchReindex` / Search; `AnalyticsAggregation` / Analytics | Bounded in-place reindexing with conflict checks; transactional date-range aggregation and outbox completion. Full search projection recovery remains separate work. |
| 4 | `UserImport` / Identity | Define accepted columns, account activation/invitation behavior, duplicate handling, and role restrictions. |
| 5 | `AuctionSettlement` / Auction, Bidding, Payment; `PaymentCycle` / Payment | Define settlement eligibility, payout timing, failure/compensation rules, and provider idempotency before enabling money-moving work. |

Catalog and Bidding do not currently have dedicated JobType values. Additional
bulk operations for those services require their own contracts and business
requirements; the gateway does not need a domain job executor.

## Running the stage 3 tests

Set `BACKEND_TEST_POSTGRES` to a disposable PostgreSQL connection string and
`BACKEND_TEST_ELASTICSEARCH` to a disposable Elasticsearch URL. Tests create and
remove their own databases/index; do not point them at production infrastructure.

```bash
dotnet test src/Services/Analytics/tests/Analytics.Application.Tests/Analytics.Application.Tests.csproj
dotnet test src/Services/Search/tests/Search.Infrastructure.Tests/Search.Infrastructure.Tests.csproj
dotnet test src/Services/Job/tests/Job.Infrastructure.Tests/Job.Infrastructure.Tests.csproj
```

The Job broker reliability test additionally needs `BACKEND_TEST_RABBIT_PORT`.
Integration tests report a skip when their required environment variables are absent.
