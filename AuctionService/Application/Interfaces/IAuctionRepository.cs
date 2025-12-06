using AuctionService.Domain.Entities;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Interfaces;

public interface IAuctionRepository : IRepository<Auction>
{
    Task<IEnumerable<Auction>> AddRangeAsync(IEnumerable<Auction> auctions, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<Auction> auctions, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetFinishedAuctionsAsync(CancellationToken cancellationToken = default);
}
