# MassTransit Outbox Pattern Implementation

## Overview

The AuctionService now uses **MassTransit's Transactional Outbox Pattern** with Entity Framework Core to ensure reliable message delivery even when RabbitMQ is unavailable.

## What Is the Outbox Pattern?

The Outbox Pattern solves the dual-write problem when you need to:

1. Save data to a database
2. Publish a message to a message broker

Without the outbox pattern, if RabbitMQ is down, your messages are lost even though your data was saved.

### How It Works

```
┌─────────────────────────────────────────────────────────┐
│  1. Begin Transaction                                   │
│     ┌──────────────────────┐                            │
│     │  Save Auction Data   │                            │
│     └──────────────────────┘                            │
│     ┌──────────────────────┐                            │
│     │  Save Event to       │                            │
│     │  OutboxMessage Table │                            │
│     └──────────────────────┘                            │
│  2. Commit Transaction (Atomic!)                        │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│  3. Background Process (Every 10s)                      │
│     ┌──────────────────────┐                            │
│     │  Read OutboxMessage  │                            │
│     │  Table                │                            │
│     └──────────────────────┘                            │
│     ┌──────────────────────┐                            │
│     │  Publish to RabbitMQ │ ← Retries if RabbitMQ down│
│     └──────────────────────┘                            │
│     ┌──────────────────────┐                            │
│     │  Mark as Delivered   │                            │
│     └──────────────────────┘                            │
└─────────────────────────────────────────────────────────┘
```

## Database Tables Added

Three new tables were added via migration `AddMassTransitOutbox`:

### 1. `OutboxMessage`

Stores messages waiting to be published to RabbitMQ

- Contains the serialized event data
- Tracks delivery status
- Retries automatically if RabbitMQ is unavailable

### 2. `OutboxState`

Tracks the state of outbox delivery

- Manages locks to prevent duplicate processing
- Tracks last processed sequence number

### 3. `InboxState`

Provides idempotent message consumption

- Prevents duplicate processing of the same message
- Tracks which messages have been consumed

## Code Changes

### 1. Added Package

```xml
<PackageReference Include="MassTransit.EntityFrameworkCore" Version="8.5.5" />
```

### 2. Updated `AuctionDbContext.cs`

Added MassTransit outbox tables to the DbContext:

```csharp
modelBuilder.AddInboxStateEntity();
modelBuilder.AddOutboxStateEntity();
modelBuilder.AddOutboxMessageEntity();
```

### 3. Created `MassTransitOutboxExtensions.cs`

New extension method configures MassTransit with outbox:

```csharp
x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
{
    o.UsePostgres();
    o.QueryDelay = TimeSpan.FromSeconds(10);
    o.UseBusOutbox();
});
```

### 4. Created `IUnitOfWork` Interface

Abstraction for managing database transactions:

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### 5. Updated `AuctionServiceImpl.cs`

Modified to use transactional pattern:

```csharp
// Add auction (not saved yet)
var createdAuction = await _repository.CreateAsync(auction, cancellationToken);

// Publish event (stored in outbox table, not saved yet)
await _eventPublisher.PublishAsync(new AuctionCreatedEvent { ... }, cancellationToken);

// Save both in ONE transaction - atomic operation!
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

### 6. Updated `AuctionRepository.cs`

Removed `SaveChangesAsync()` calls from repository methods to allow service layer to control the transaction.

### 7. Updated `Program.cs`

Changed from direct RabbitMQ publishing to outbox pattern:

```csharp
builder.Services.AddMassTransitWithOutbox(builder.Configuration);
```

## Benefits

✅ **No message loss** - Messages are saved in the database within the same transaction as your data
✅ **RabbitMQ downtime resilience** - Messages are stored and published when RabbitMQ comes back online
✅ **Exactly-once delivery** - Inbox state prevents duplicate message processing
✅ **Automatic retry** - MassTransit automatically retries failed deliveries
✅ **Transactional integrity** - Either both data and message are saved, or neither

## Configuration

### Outbox Query Delay

Messages are checked and published every **10 seconds** by default:

```csharp
o.QueryDelay = TimeSpan.FromSeconds(10);
```

You can adjust this in `MassTransitOutboxExtensions.cs`.

## Testing Scenarios

### Scenario 1: RabbitMQ is Running (Normal Operation)

1. Create/Update/Delete an auction
2. Data is saved to `Auctions` table
3. Event is saved to `OutboxMessage` table
4. Within 10 seconds, the event is published to RabbitMQ
5. Event is marked as delivered in the outbox

### Scenario 2: RabbitMQ is Down

1. Create/Update/Delete an auction
2. Data is saved to `Auctions` table ✅
3. Event is saved to `OutboxMessage` table ✅
4. Background process tries to publish but RabbitMQ is down
5. Message stays in `OutboxMessage` table (retry scheduled)
6. When RabbitMQ comes back online, messages are published automatically

### Scenario 3: Database Transaction Fails

1. Create an auction (validation error or DB constraint violation)
2. Transaction is rolled back
3. Neither auction data nor outbox message is saved ✅ (Atomic!)

## How to Apply the Migration

```powershell
cd AuctionService\Infrastructure
dotnet ef database update --startup-project ..\API\AuctionService.API.csproj
```

Or simply run the application - migrations are applied automatically in `Program.cs`:

```csharp
db.Database.Migrate();
```

## Monitoring

### Check Outbox Messages

Query the database to see pending messages:

```sql
SELECT * FROM "OutboxMessage" WHERE "SentTime" IS NULL;
```

### Check Delivered Messages

```sql
SELECT * FROM "OutboxMessage" WHERE "SentTime" IS NOT NULL;
```

### Check Inbox State (Consumed Messages)

```sql
SELECT * FROM "InboxState";
```

## Performance Considerations

- Outbox messages are queried every 10 seconds (configurable)
- Messages are batched for delivery
- Delivered messages remain in the table (consider cleanup strategy for production)
- Minimal overhead - the outbox write happens in the same transaction as your data

## Comparison: Before vs After

### Before (Direct Publishing)

```csharp
await _repository.CreateAsync(auction);  // ✅ Saved
await _eventPublisher.PublishAsync(...); // ❌ Lost if RabbitMQ is down
```

### After (Outbox Pattern)

```csharp
await _repository.CreateAsync(auction);     // Not saved yet
await _eventPublisher.PublishAsync(...);    // Not saved yet
await _unitOfWork.SaveChangesAsync();       // ✅ Both saved atomically!
// Published to RabbitMQ asynchronously (with retry)
```

## Related Files

- `AuctionService.Infrastructure.csproj` - Package reference
- `AuctionDbContext.cs` - Outbox table configuration
- `MassTransitOutboxExtensions.cs` - Outbox setup
- `OutboxEventPublisher.cs` - Event publisher implementation
- `IUnitOfWork.cs` / `UnitOfWork.cs` - Transaction management
- `AuctionServiceImpl.cs` - Updated service implementation
- `AuctionRepository.cs` - Removed SaveChanges to support transactions
- `Migrations/AddMassTransitOutbox.cs` - Database migration

## Further Reading

- [MassTransit Outbox Documentation](https://masstransit.io/documentation/configuration/middleware/outbox)
- [Outbox Pattern Explained](https://microservices.io/patterns/data/transactional-outbox.html)
- [MassTransit Entity Framework Integration](https://masstransit.io/documentation/configuration/persistence/entity-framework)
