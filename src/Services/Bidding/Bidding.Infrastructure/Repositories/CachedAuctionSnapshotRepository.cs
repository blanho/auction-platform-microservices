using Bidding.Application.Interfaces;
using BuildingBlocks.Infrastructure.Caching;
using Microsoft.Extensions.Logging;

namespace Bidding.Infrastructure.Repositories;

public class CachedAuctionSnapshotRepository : IAuctionSnapshotRepository
{
    private readonly ICacheService _cache;
    private readonly ILogger<CachedAuctionSnapshotRepository> _logger;
    private static readonly TimeSpan SnapshotTtl = TimeSpan.FromMinutes(5);

    public CachedAuctionSnapshotRepository(
        ICacheService cache,
        ILogger<CachedAuctionSnapshotRepository> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<AuctionSnapshot?> GetAsync(Guid auctionId, CancellationToken cancellationToken = default)
    {
        var key = GetCacheKey(auctionId);
        var snapshot = await _cache.GetAsync<AuctionSnapshot>(key, cancellationToken);

        if (snapshot != null)
        {
            _logger.LogDebug("Auction snapshot cache HIT for {AuctionId}", auctionId);
        }
        else
        {
            _logger.LogDebug("Auction snapshot cache MISS for {AuctionId}", auctionId);
        }

        return snapshot;
    }

    public async Task UpsertAsync(AuctionSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var key = GetCacheKey(snapshot.AuctionId);
        await _cache.SetAsync(key, snapshot, SnapshotTtl, cancellationToken);
        _logger.LogDebug("Auction snapshot cached for {AuctionId}", snapshot.AuctionId);
    }

    public async Task DeleteAsync(Guid auctionId, CancellationToken cancellationToken = default)
    {
        var key = GetCacheKey(auctionId);
        await _cache.RemoveAsync(key, cancellationToken);
        _logger.LogDebug("Auction snapshot removed for {AuctionId}", auctionId);
    }

    private static string GetCacheKey(Guid auctionId) => $"auction:snapshot:{auctionId}";
}
