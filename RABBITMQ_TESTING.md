# RabbitMQ Integration - Testing Guide

## ✅ Setup Complete

### Infrastructure Added

1. **RabbitMQ** container (port 5672 AMQP, 15672 Management UI)
2. **Common.Messaging** library with MassTransit
3. **Event Publishers** in AuctionService
4. **Event Consumers** in SearchService

### Services Status

- ✅ docker-compose.yml updated
- ✅ Both services have MassTransit configured
- ✅ Events: AuctionCreated, AuctionUpdated, AuctionDeleted
- ✅ Cache invalidation automatic in SearchService

---

## 🚀 How to Test

### 1. Start Infrastructure

```powershell
docker-compose up -d redis auction-postgres rabbitmq search-mongo
```

### 2. Verify RabbitMQ is Running

- Management UI: http://localhost:15672
- Login: guest / guest

### 3. Run AuctionService

```powershell
dotnet run --project AuctionService/API/AuctionService.API.csproj
```

### 4. Run SearchService (in separate terminal)

```powershell
dotnet run --project SearchService/API/SearchService.API.csproj
```

### 5. Test Event Flow

#### Create Auction (publishes AuctionCreatedEvent)

```powershell
curl -X POST http://localhost:5000/api/v1/auctions `
  -H "Content-Type: application/json" `
  -d '{
    "reservePrice": 50000,
    "auctionEnd": "2025-12-31T23:59:59Z",
    "make": "BMW",
    "model": "M3",
    "year": 2023,
    "color": "Black",
    "mileage": 5000,
    "imageUrl": "https://example.com/bmw-m3.jpg"
  }'
```

#### Update Auction (publishes AuctionUpdatedEvent)

```powershell
curl -X PUT http://localhost:5000/api/v1/auctions/{id} `
  -H "Content-Type: application/json" `
  -d '{
    "make": "BMW",
    "model": "M3 Competition",
    "mileage": 6000
  }'
```

#### Verify in SearchService

```powershell
# Check if auction appears in search
curl http://localhost:5001/api/v1/search/items

# Search by query
curl "http://localhost:5001/api/v1/search?query=BMW"
```

#### Delete Auction (publishes AuctionDeletedEvent)

```powershell
curl -X DELETE http://localhost:5000/api/v1/auctions/{id}
```

---

## 📊 Monitor Events

### RabbitMQ Management UI

1. Go to http://localhost:15672
2. Navigate to "Queues" tab
3. You'll see consumer queues for SearchService
4. Check message rates and consumer status

### Application Logs

**AuctionService** logs will show:

```
Published AuctionCreatedEvent for auction {id}
Published AuctionUpdatedEvent for auction {id}
Published AuctionDeletedEvent for auction {id}
```

**SearchService** logs will show:

```
Consuming AuctionCreatedEvent for auction {id}
Successfully indexed auction {id} to search
Cache invalidated for search item {id}
```

---

## 🔍 What to Verify

1. ✅ Create auction → Check SearchService has it immediately
2. ✅ Update auction → Verify changes reflected in search
3. ✅ Delete auction → Confirm removed from search index
4. ✅ Cache invalidation → Check logs show cache removal
5. ✅ RabbitMQ UI → See messages flowing through queues

---

## 🐛 Troubleshooting

### RabbitMQ Connection Failed

```
Check: docker ps | grep rabbitmq
Fix: docker-compose up -d rabbitmq
```

### Events Not Arriving

```
Check RabbitMQ Management UI:
- Connections tab → Should see 2 connections (both services)
- Queues tab → Should see consumer queues
- Exchanges tab → Should see default exchange
```

### SearchService Not Consuming

```
Check logs for:
- "MassTransit started" message
- Consumer registration errors
Verify MongoDB is running: docker ps | grep search-mongo
```

---

## 🎯 Architecture Achieved

```
┌──────────────────┐         ┌──────────────┐         ┌─────────────────┐
│  AuctionService  │         │   RabbitMQ   │         │  SearchService  │
│                  │         │              │         │                 │
│  Create/Update   │─[pub]──→│   Exchange   │─[sub]──→│   Consumers     │
│  Delete Auction  │         │   + Queues   │         │   Sync MongoDB  │
│                  │         │              │         │   Clear Cache   │
└──────────────────┘         └──────────────┘         └─────────────────┘
        ↓                                                       ↓
   [Postgres]                                             [MongoDB]
        ↓                                                       ↓
    [Redis Cache]                                        [Redis Cache]
```

**Benefits:**

- ✅ Services decoupled (no HTTP calls between them)
- ✅ Eventual consistency (search syncs automatically)
- ✅ Resilient (RabbitMQ handles retry/dead-letter)
- ✅ Both caches remain valuable (different workloads)
- ✅ Scalable (can add more consumers)

---

## 📦 What Was Created

### Common.Messaging

- `Events/AuctionCreatedEvent.cs`
- `Events/AuctionUpdatedEvent.cs`
- `Events/AuctionDeletedEvent.cs`
- `Abstractions/IEventPublisher.cs`
- `Implementations/MassTransitEventPublisher.cs`
- `Extensions/MassTransitExtensions.cs`

### AuctionService

- Event publishing after Create/Update/Delete
- MassTransit registration in Program.cs

### SearchService

- `Consumers/AuctionCreatedConsumer.cs`
- `Consumers/AuctionUpdatedConsumer.cs`
- `Consumers/AuctionDeletedConsumer.cs`
- MassTransit consumer registration

### Docker

- RabbitMQ container with management UI
- Health checks and volume persistence
