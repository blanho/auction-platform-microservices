# Testing MassTransit Outbox Pattern

## Prerequisites

- Docker running (PostgreSQL and Redis containers)
- RabbitMQ container (can be started/stopped for testing)

## Test 1: Normal Operation (RabbitMQ Running)

### Start All Services

```powershell
docker-compose up -d
```

### Run AuctionService

```powershell
dotnet run --project AuctionService/API/AuctionService.API.csproj
```

### Create an Auction

```powershell
$body = @{
    make = "Tesla"
    model = "Model 3"
    year = 2024
    color = "Blue"
    mileage = 5000
    imageUrl = "https://example.com/tesla.jpg"
    reservePrice = 45000
    auctionEnd = "2025-12-31T23:59:59Z"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/v1/auctions" `
    -Method POST `
    -Headers @{"seller" = "test@example.com"} `
    -Body $body `
    -ContentType "application/json"
```

### Verify

1. Check auction was created in database
2. Check `OutboxMessage` table - message should be created and sent within 10 seconds
3. Check RabbitMQ Management UI (http://localhost:15672) - message should appear in queue
4. SearchService should receive and process the event

## Test 2: RabbitMQ Down (Resilience Test)

### Stop RabbitMQ

```powershell
docker stop shared-rabbitmq
```

### Create an Auction (Same as above)

The auction should still be created successfully!

### Check Database

```sql
-- Check auction was saved
SELECT * FROM "Auctions" ORDER BY "CreatedAt" DESC LIMIT 1;

-- Check message is in outbox waiting to be delivered
SELECT "SequenceNumber", "MessageType", "SentTime", "EnqueueTime"
FROM "OutboxMessage"
WHERE "SentTime" IS NULL;
```

You should see:

- ✅ Auction data is saved
- ✅ Message is in OutboxMessage table with `SentTime = NULL`

### Start RabbitMQ Again

```powershell
docker start shared-rabbitmq
# Wait ~30 seconds for container to fully start
```

### Verify Automatic Delivery

Within 10 seconds after RabbitMQ is back online:

1. Check `OutboxMessage` table - `SentTime` should now be populated
2. Check RabbitMQ Management UI - message should appear
3. SearchService should process the event

```sql
-- Verify message was delivered
SELECT "SequenceNumber", "MessageType", "SentTime", "EnqueueTime"
FROM "OutboxMessage"
ORDER BY "SequenceNumber" DESC LIMIT 5;
```

## Test 3: Database Transaction Rollback

### Attempt Invalid Operation

Try to create an auction with invalid data to trigger a database constraint error:

```powershell
# This will fail due to validation or constraints
$body = @{
    make = ""  # Empty make might cause validation error
    model = ""
    # ... other fields
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/v1/auctions" `
    -Method POST `
    -Headers @{"seller" = "test@example.com"} `
    -Body $body `
    -ContentType "application/json"
```

### Verify Atomicity

```sql
-- Check that neither auction nor outbox message was created
SELECT COUNT(*) FROM "Auctions" WHERE "Make" = '';
SELECT COUNT(*) FROM "OutboxMessage" WHERE "MessageType" LIKE '%AuctionCreated%'
    AND "EnqueueTime" > NOW() - INTERVAL '1 minute';
```

Both counts should be 0 - proving the transaction was rolled back completely.

## Test 4: Update and Delete Operations

### Update Auction (RabbitMQ Down)

```powershell
docker stop shared-rabbitmq

$updateBody = @{
    make = "Tesla"
    model = "Model S"
    color = "Red"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/v1/auctions/{auctionId}" `
    -Method PUT `
    -Headers @{"seller" = "test@example.com"} `
    -Body $updateBody `
    -ContentType "application/json"
```

### Delete Auction (RabbitMQ Down)

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/auctions/{auctionId}" `
    -Method DELETE
```

### Verify Outbox Messages

```sql
-- Should see AuctionUpdated and AuctionDeleted events waiting
SELECT "MessageType", "SentTime", "EnqueueTime"
FROM "OutboxMessage"
WHERE "SentTime" IS NULL
ORDER BY "SequenceNumber" DESC;
```

### Start RabbitMQ and Verify Delivery

```powershell
docker start shared-rabbitmq
```

Wait 10-20 seconds and verify all pending messages were delivered.

## Monitoring Queries

### Pending Messages

```sql
SELECT COUNT(*) as "PendingMessages"
FROM "OutboxMessage"
WHERE "SentTime" IS NULL;
```

### Recently Delivered Messages

```sql
SELECT "SequenceNumber", "MessageType", "EnqueueTime", "SentTime",
       ("SentTime" - "EnqueueTime") as "DeliveryDelay"
FROM "OutboxMessage"
WHERE "SentTime" IS NOT NULL
ORDER BY "SentTime" DESC
LIMIT 10;
```

### Inbox State (Idempotency Check)

```sql
SELECT "MessageId", "ConsumerId", "Received", "Consumed", "Delivered"
FROM "InboxState"
ORDER BY "Received" DESC
LIMIT 10;
```

### Failed Deliveries (Retry Count)

```sql
SELECT "SequenceNumber", "MessageType", "EnqueueTime",
       EXTRACT(EPOCH FROM (NOW() - "EnqueueTime"))/60 as "MinutesSinceEnqueue"
FROM "OutboxMessage"
WHERE "SentTime" IS NULL
  AND "EnqueueTime" < NOW() - INTERVAL '5 minutes';
```

## Expected Behavior

### ✅ Success Scenarios

- Auction operations succeed even when RabbitMQ is down
- Messages are stored in database transactionally
- Messages are automatically delivered when RabbitMQ comes back online
- No message loss
- No duplicate processing

### ❌ Failure Scenarios That Are Handled

- RabbitMQ connection timeout → Message stored, retried later
- RabbitMQ service down → Message stored, retried later
- Network issues → Message stored, retried later
- Database transaction failure → Neither data nor message saved (atomic)

## Logs to Watch

### AuctionService Logs

```
[INF] Creating auction for seller test@example.com
[INF] Created auction {AuctionId} and queued event in outbox
```

### MassTransit Logs

```
[DBG] Outbox delivered message {MessageId}
[INF] RabbitMQ connection established
```

## Cleanup (Optional)

### Clear Delivered Messages

```sql
-- Be careful in production! This deletes delivered messages
DELETE FROM "OutboxMessage" WHERE "SentTime" IS NOT NULL;
```

### Reset Test Data

```sql
DELETE FROM "OutboxMessage";
DELETE FROM "OutboxState";
DELETE FROM "InboxState";
```

## Performance Tips

- Monitor `OutboxMessage` table size - consider archiving old messages
- Adjust `QueryDelay` if messages need faster delivery (trade-off: database load)
- Index recommendations are already included in the migration
- Consider partitioning outbox tables in high-volume scenarios
