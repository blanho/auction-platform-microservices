using Bidding.Domain.Entities;
using BuildingBlocks.Domain.Entities;

namespace Bidding.Application.Interfaces;

public interface IAutoBidRepository : IRepository<AutoBid>
{
    Task<AutoBid?> GetActiveAutoBidAsync(Guid auctionId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<AutoBid>> GetActiveAutoBidsForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default);
    Task<List<AutoBid>> GetAutoBidsByUserAsync(Guid userId, bool? activeOnly, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetAutoBidsCountForUserAsync(Guid userId, bool? activeOnly, CancellationToken cancellationToken = default);
}

