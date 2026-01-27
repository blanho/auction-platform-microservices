#nullable enable
using Auctions.Application.Interfaces;

namespace Auctions.Infrastructure.Persistence.Repositories;

public class CachedAuctionViewRepository : IAuctionViewRepository
{
    private readonly IAuctionViewRepository _inner;

    public CachedAuctionViewRepository(IAuctionViewRepository inner)
    {
        _inner = inner;
    }

    public async Task<int> GetViewCountForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default)
    {
        return await _inner.GetViewCountForAuctionAsync(auctionId, cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetViewCountsForAuctionsAsync(List<Guid> auctionIds, CancellationToken cancellationToken = default)
    {
        return await _inner.GetViewCountsForAuctionsAsync(auctionIds, cancellationToken);
    }

    public async Task RecordViewAsync(Guid auctionId, string? userId, string? ipAddress, CancellationToken cancellationToken = default)
    {
        await _inner.RecordViewAsync(auctionId, userId, ipAddress, cancellationToken);
    }
}
