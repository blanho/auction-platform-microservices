# Testing Outbox Pattern - Docker Setup

## Your Setup

- **RabbitMQ**: Running in Docker (port 5672)
- **PostgreSQL**: Running in Docker (port 5433)
- **Redis**: Running in Docker (port 6379)
- **AuctionService API**: Running locally (not in Docker)

## Important: The Warning is Expected!

The RabbitMQ connection warning you're seeing is **NORMAL** when testing the outbox pattern with RabbitMQ down or not fully started. The outbox pattern is specifically designed for this scenario!

```
[WRN] Connection Failed: rabbitmq://localhost/
```

This means:

- ✅ The application will continue to work
- ✅ Auctions can still be created
- ✅ Events will be stored in the OutboxMessage table
- ✅ When RabbitMQ is available, messages will be delivered automatically

## Test Scenario 1: RabbitMQ Running

### Step 1: Start All Services

```powershell
docker-compose up -d
```

### Step 2: Wait for RabbitMQ to be Ready

```powershell
# Check RabbitMQ is ready (wait until healthy)
docker ps | Select-String rabbitmq

# Or check logs
docker logs shared-rabbitmq --tail 20
```

Look for: `Server startup complete`

### Step 3: Run AuctionService Locally

```powershell
cd C:\Projects\auction
dotnet run --project AuctionService\API\AuctionService.API.csproj
```

### Step 4: Create an Auction

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

### Step 5: Verify Message Flow

**Check OutboxMessage (should be sent quickly):**

```powershell
docker exec -it auction-postgres psql -U postgres -d auction_db -c "SELECT \"SequenceNumber\", \"MessageType\", \"EnqueueTime\", \"SentTime\" FROM \"OutboxMessage\" ORDER BY \"SequenceNumber\" DESC LIMIT 5;"
```

**Check RabbitMQ Management UI:**

- Open: http://localhost:15672
- Login: guest / guest
- Go to Queues tab
- You should see messages being processed

## Test Scenario 2: RabbitMQ Down (Outbox Test)

### Step 1: Stop RabbitMQ

```powershell
docker stop shared-rabbitmq
```

### Step 2: Create an Auction (Same as above)

The auction creation should **succeed** even though RabbitMQ is down!

### Step 3: Verify Outbox Has Pending Message

```powershell
docker exec -it auction-postgres psql -U postgres -d auction_db -c "SELECT \"SequenceNumber\", \"MessageType\", \"EnqueueTime\", \"SentTime\" FROM \"OutboxMessage\" WHERE \"SentTime\" IS NULL ORDER BY \"SequenceNumber\" DESC;"
```

**Expected Result:**

- ✅ Auction created successfully
- ✅ Message in OutboxMessage table with `SentTime = NULL`
- ⚠️ Warning in logs about RabbitMQ connection (this is expected!)

### Step 4: Start RabbitMQ and Watch Auto-Delivery

```powershell
# Start RabbitMQ
docker start shared-rabbitmq

# Wait 30-60 seconds for RabbitMQ to fully start
Start-Sleep -Seconds 40
```

### Step 5: Verify Message Was Delivered

```powershell
# Check if SentTime is now populated
docker exec -it auction-postgres psql -U postgres -d auction_db -c "SELECT \"SequenceNumber\", \"MessageType\", \"EnqueueTime\", \"SentTime\", (\"SentTime\" - \"EnqueueTime\") as \"Delay\" FROM \"OutboxMessage\" ORDER BY \"SequenceNumber\" DESC LIMIT 5;"
```

**Expected Result:**

- ✅ `SentTime` is now populated
- ✅ Message delivered to RabbitMQ
- ✅ SearchService processes the event (if running)

## Test Scenario 3: Complete Integration Test

### Step 1: Start Everything

```powershell
# Start all Docker services
docker-compose up -d

# Wait for services to be ready
Start-Sleep -Seconds 30

# Run AuctionService locally
dotnet run --project AuctionService\API\AuctionService.API.csproj
```

### Step 2: Create Multiple Auctions

```powershell
# Create auction 1
$body1 = @{
    make = "Tesla"
    model = "Model 3"
    year = 2024
    color = "Blue"
    mileage = 5000
    imageUrl = "https://example.com/tesla.jpg"
    reservePrice = 45000
    auctionEnd = "2025-12-31T23:59:59Z"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/v1/auctions" -Method POST -Headers @{"Content-Type"="application/json"; "seller"="test@example.com"} -Body $body1

# Create auction 2
$body2 = @{
    make = "BMW"
    model = "M3"
    year = 2023
    color = "Black"
    mileage = 10000
    imageUrl = "https://example.com/bmw.jpg"
    reservePrice = 55000
    auctionEnd = "2025-11-30T23:59:59Z"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/v1/auctions" -Method POST -Headers @{"Content-Type"="application/json"; "seller"="test@example.com"} -Body $body2
```

### Step 3: Verify Complete Flow

**Check Auctions in PostgreSQL:**

```powershell
docker exec -it auction-postgres psql -U postgres -d auction_db -c "SELECT \"Id\", \"Seller\", \"CreatedAt\", \"Status\" FROM \"Auctions\" ORDER BY \"CreatedAt\" DESC LIMIT 5;"
```

**Check Outbox Messages:**

```powershell
docker exec -it auction-postgres psql -U postgres -d auction_db -c "SELECT \"SequenceNumber\", \"MessageType\", \"SentTime\" IS NOT NULL as \"Delivered\" FROM \"OutboxMessage\" ORDER BY \"SequenceNumber\" DESC LIMIT 5;"
```

**Check MongoDB (SearchService):**

```powershell
docker exec -it search-mongo mongosh --eval "db.SearchItems.find().sort({createdAt: -1}).limit(2).pretty()"
```

## Troubleshooting

### Issue: "Connection Failed" Warning

**Status**: This is expected when RabbitMQ is down or starting up
**Action**: No action needed if you're testing outbox functionality

### Issue: OutboxMessage Table is Empty

**Possible causes:**

1. Migration not applied
2. SaveChangesAsync not called
3. Transaction rolled back due to error

**Check migration:**

```powershell
cd AuctionService\Infrastructure
dotnet ef migrations list --startup-project ..\API\AuctionService.API.csproj
```

**Check if tables exist:**

```powershell
docker exec -it auction-postgres psql -U postgres -d auction_db -c "\dt"
```

### Issue: Messages Not Delivering Even After RabbitMQ Starts

**Check:**

1. RabbitMQ is fully started (check logs)
2. Wait at least 10 seconds (QueryDelay setting)
3. Check application logs for errors
4. Verify RabbitMQ credentials

**View RabbitMQ logs:**

```powershell
docker logs shared-rabbitmq --tail 50
```

## Monitoring Commands

**Watch OutboxMessage table in real-time:**

```powershell
# Run this in a separate terminal
while ($true) {
    Clear-Host
    docker exec -it auction-postgres psql -U postgres -d auction_db -c "SELECT \"SequenceNumber\", \"MessageType\", \"EnqueueTime\", \"SentTime\" FROM \"OutboxMessage\" ORDER BY \"SequenceNumber\" DESC LIMIT 10;"
    Start-Sleep -Seconds 5
}
```

**Count pending messages:**

```powershell
docker exec -it auction-postgres psql -U postgres -d auction_db -c "SELECT COUNT(*) as \"PendingMessages\" FROM \"OutboxMessage\" WHERE \"SentTime\" IS NULL;"
```

**Check application logs:**

```powershell
# If running locally, check the terminal
# If running in Docker:
docker logs auction-api --tail 50 -f
```

## Clean Up

**Stop and remove everything:**

```powershell
docker-compose down -v
```

**Keep services but clear data:**

```powershell
docker exec -it auction-postgres psql -U postgres -d auction_db -c "DELETE FROM \"OutboxMessage\";"
docker exec -it auction-postgres psql -U postgres -d auction_db -c "DELETE FROM \"Auctions\";"
```

## Success Criteria

✅ Auction created successfully (API returns 200/201)
✅ Auction data in PostgreSQL `Auctions` table
✅ Event in `OutboxMessage` table (even if RabbitMQ is down)
✅ When RabbitMQ is available, `SentTime` gets populated within 10 seconds
✅ No data loss even if RabbitMQ is down during creation
✅ Messages automatically delivered when RabbitMQ comes back online
