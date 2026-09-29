using AuctionService.Contracts.Events;

namespace Auctions.Infrastructure.Messaging.Consumers;

public interface IAuctionWorkflowStore
{
    Task<bool> ExistsAsync(string key, CancellationToken ct);
    Task<ImportTotals?> CompleteAsync(string key, Guid correlationId, int succeeded, int failed,
        List<ImportRowErrorPayload> errors, int totalBatches, CancellationToken ct);
}
