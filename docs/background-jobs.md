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

Deploy the updated Payment, Notification, and Storage consumers before the
updated Job service. Older consumers do not understand the added command routes
or parent-item tracking. No database migration is required for stages 1–2.

## Remaining implementation stages

Only the seven types listed above currently have item dispatch support. The other
enum values are declarations, not executable workflows through Jobs.

| Stage | Types / service | Work still required |
|---|---|---|
| 1 — implemented | `ReportGeneration`, `BulkNotification` | Parent-item dispatch, completion, fault handling, and regression coverage |
| 2 — implemented | `ImageProcessing`, `BulkFileUpload` / Storage | Image validation, checksum and thumbnails, owned upload finalization, outbox results, and replay protection |
| 3 | `SearchReindex` / Search; `AnalyticsAggregation` / Analytics | Define rebuild scope, projection consistency, aggregation periods, and resumable batches. |
| 4 | `UserImport` / Identity | Define accepted columns, account activation/invitation behavior, duplicate handling, and role restrictions. |
| 5 | `AuctionSettlement` / Auction, Bidding, Payment; `PaymentCycle` / Payment | Define settlement eligibility, payout timing, failure/compensation rules, and provider idempotency before enabling money-moving work. |

Catalog and Bidding do not currently have dedicated JobType values. Additional
bulk operations for those services require their own contracts and business
requirements; the gateway does not need a domain job executor.
