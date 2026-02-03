using Bidding.Application.Filtering;
using Bidding.Domain.Entities;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Paging;
using BuildingBlocks.Domain.Entities;

namespace Bidding.Application.Interfaces;

public interface IAutoBidRepository : IRepository<AutoBid>
{
    Task<AutoBid?> GetActiveAutoBidAsync(Guid auctionId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<AutoBid>> GetActiveAutoBidsForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default);
    Task<PaginatedResult<AutoBid>> GetAutoBidsByUserAsync(Guid userId, AutoBidQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<int> GetAutoBidsCountForUserAsync(Guid userId, bool? activeOnly, CancellationToken cancellationToken = default);
}

