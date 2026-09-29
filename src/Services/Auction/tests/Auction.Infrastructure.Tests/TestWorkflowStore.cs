using AuctionService.Contracts.Events;
using Auctions.Infrastructure.Messaging.Consumers;

namespace Auction.Infrastructure.Tests;

internal sealed class TestWorkflowStore : IAuctionWorkflowStore
{
    private readonly Dictionary<string, (Guid Correlation, ImportTotals Totals)> _results = new();
    public Task<bool> ExistsAsync(string key, CancellationToken ct) => Task.FromResult(_results.ContainsKey(key));
    public Task<ImportTotals?> CompleteAsync(string key, Guid correlationId, int succeeded, int failed,
        List<ImportRowErrorPayload> errors, int totalBatches, CancellationToken ct)
    {
        _results.Add(key, (correlationId, new(succeeded, failed, errors)));
        var results = _results.Values.Where(x => x.Correlation == correlationId).Select(x => x.Totals).ToList();
        return Task.FromResult(results.Count == totalBatches
            ? new ImportTotals(results.Sum(x => x.Succeeded), results.Sum(x => x.Failed), results.SelectMany(x => x.Errors).ToList())
            : null);
    }
}
