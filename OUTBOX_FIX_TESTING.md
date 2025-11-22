# Outbox Pattern Testing Guide

## The Issue and Fix

**Problem**: Messages were not being stored in the OutboxMessage table when RabbitMQ was down.

**Root Cause**: The `OutboxEventPublisher` was using `IPublishEndpoint` directly from DI, which doesn't participate in the DbContext transaction scope.

**Solution**: Changed to get `IPublishEndpoint` from the `DbContext.GetService<IPublishEndpoint>()`, which ensures the publish operation uses the same DbContext transaction scope and stores messages in the outbox.

## Testing Steps

### 1. Start Dependencies (Without RabbitMQ)

```powershell
# Start only PostgreSQL and Redis
docker-compose up -d auction-postgres redis

# Stop RabbitMQ to test outbox
docker stop shared-rabbitmq
```

### 2. Run AuctionService

```powershell
dotnet run --project AuctionService/API/AuctionService.API.csproj
```

### 3. Create an Auction (Postman or PowerShell)

**Using PowerShell:**

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

$headers = @{
    "Content-Type" = "application/json"
    "seller" = "test@example.com"
}

Invoke-RestMethod -Uri "http://localhost:5000/api/v1/auctions" `
    -Method POST `
    -Headers $headers `
    -Body $body
```

**Using Postman:**

- Method: `POST`
- URL: `http://localhost:5000/api/v1/auctions`
- Headers:
  - `Content-Type`: `application/json`
  - `seller`: `test@example.com`
- Body (raw JSON):

```json
{
  "make": "Tesla",
  "model": "Model 3",
  "year": 2024,
  "color": "Blue",
  "mileage": 5000,
  "imageUrl": "https://example.com/tesla.jpg",
  "reservePrice": 45000,
  "auctionEnd": "2025-12-31T23:59:59Z"
}
```

### 4. Verify Outbox Message

**Connect to PostgreSQL:**

```powershell
docker exec -it auction-postgres psql -U admin -d auction_db
```

**Check OutboxMessage Table:**

```sql
-- Should see the AuctionCreatedEvent message waiting to be sent
SELECT
    "SequenceNumber",
    "MessageType",
    "EnqueueTime",
    "SentTime",
    LEFT("Body", 100) as "BodyPreview"
FROM "OutboxMessage"
ORDER BY "SequenceNumber" DESC
LIMIT 5;
```

**Expected Result:**

- You should see a row with `MessageType` containing `AuctionCreatedEvent`
- `EnqueueTime` should be populated (when it was created)
- `SentTime` should be `NULL` (because RabbitMQ is down)

### 5. Check Auction Data

```sql
-- Verify auction was created successfully
SELECT "Id", "Seller", "CreatedAt", "Status"
FROM "Auctions"
ORDER BY "CreatedAt" DESC
LIMIT 1;
```

### 6. Start RabbitMQ and Watch Delivery

```powershell
# Start RabbitMQ
docker start shared-rabbitmq

# Wait about 30 seconds for RabbitMQ to fully start
```

**Check OutboxMessage Again:**

```sql
-- Now SentTime should be populated
SELECT
    "SequenceNumber",
    "MessageType",
    "EnqueueTime",
    "SentTime",
    ("SentTime" - "EnqueueTime") as "DeliveryDelay"
FROM "OutboxMessage"
ORDER BY "SequenceNumber" DESC
LIMIT 5;
```

**Expected Result:**

- `SentTime` should now be populated
- Message should appear in RabbitMQ Management UI (http://localhost:15672)
- SearchService should receive and process the event (check MongoDB)

### 7. Verify in MongoDB (SearchService)

```powershell
# Connect to MongoDB
docker exec -it search-mongo mongosh

# Switch to database
use searchdb

# Check if auction was indexed
db.items.find().sort({createdAt: -1}).limit(1).pretty()
```

## Troubleshooting

### If OutboxMessage table is still empty:

1. **Check the migration was applied:**

```powershell
cd AuctionService\Infrastructure
dotnet ef migrations list --startup-project ..\API\AuctionService.API.csproj
```

Should show: `20251122131446_AddMassTransitOutbox (Applied)`

2. **Check if tables exist:**

```sql
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
AND table_name LIKE '%Outbox%' OR table_name LIKE '%Inbox%';
```

Should show: `InboxState`, `OutboxMessage`, `OutboxState`

3. **Check application logs** for errors during publish

4. **Verify DbContext is being used correctly** - make sure `SaveChangesAsync()` is called after publishing

## Key Points

✅ **Before the fix**: `IPublishEndpoint` from DI → Doesn't use DbContext transaction → Messages not saved to outbox

✅ **After the fix**: `DbContext.GetService<IPublishEndpoint>()` → Uses DbContext transaction scope → Messages saved to outbox

✅ **Outbox query interval**: 10 seconds (configurable in `MassTransitOutboxExtensions.cs`)

✅ **Atomic operation**: Both auction data and outbox message are saved in the same transaction

## Clean Up

```powershell
# Stop all services
docker-compose down

# Or keep running for more testing
```
