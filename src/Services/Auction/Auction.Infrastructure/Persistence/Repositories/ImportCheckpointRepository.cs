using Auctions.Application.Features.Auctions.ImportAuctions;
using BuildingBlocks.Infrastructure.Caching;

namespace Auctions.Infrastructure.Persistence.Repositories;

public class ImportCheckpointRepository : IImportCheckpointRepository
{
    private const string CacheKeyPrefix = "import-checkpoint:";
    private static readonly TimeSpan CheckpointTtl = TimeSpan.FromHours(24);

    private readonly ICacheService _cache;

    public ImportCheckpointRepository(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<ImportCheckpoint?> GetCheckpointAsync(
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(correlationId);
        return await _cache.GetAsync<ImportCheckpoint>(key, cancellationToken);
    }

    public async Task SaveCheckpointAsync(
        ImportCheckpoint checkpoint,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(checkpoint.CorrelationId);
        await _cache.SetAsync(key, checkpoint, CheckpointTtl, cancellationToken);
    }

    public async Task DeleteCheckpointAsync(
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(correlationId);
        await _cache.RemoveAsync(key, cancellationToken);
    }

    private static string BuildKey(string correlationId) => $"{CacheKeyPrefix}{correlationId}";
}
