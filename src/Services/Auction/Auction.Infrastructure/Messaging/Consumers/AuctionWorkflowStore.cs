using System.Text.Json;
using AuctionService.Contracts.Events;
using Auctions.Infrastructure.Persistence;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class AuctionWorkflowStore(AuctionDbContext db) : IAuctionWorkflowStore
{
    public async Task<bool> ExistsAsync(string key, CancellationToken ct)
    {
        var correlation = key.Split(':').ElementAtOrDefault(1) ?? key;
        if (db.Database.CurrentTransaction is not null)
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({correlation}, 0))", ct);
        return await db.Set<AuctionWorkflowReceipt>().AnyAsync(x => x.Key == key, ct);
    }

    public async Task<ImportTotals?> CompleteAsync(string key, Guid correlationId, int succeeded, int failed,
        List<ImportRowErrorPayload> errors, int totalBatches, CancellationToken ct)
    {
        db.Set<AuctionWorkflowReceipt>().Add(new AuctionWorkflowReceipt
        {
            Key = key,
            CorrelationId = correlationId,
            Succeeded = succeeded,
            Failed = failed,
            ErrorsJson = JsonSerializer.Serialize(errors)
        });
        await db.SaveChangesAsync(ct);
        var receipts = await db.Set<AuctionWorkflowReceipt>()
            .Where(x => x.CorrelationId == correlationId).ToListAsync(ct);
        if (receipts.Count != totalBatches) return null;
        return new ImportTotals(receipts.Sum(x => x.Succeeded), receipts.Sum(x => x.Failed),
            receipts.SelectMany(x => JsonSerializer.Deserialize<List<ImportRowErrorPayload>>(x.ErrorsJson)!).ToList());
    }
}
