using BidService.Domain.Entities;
using Common.Repository.Interfaces;

namespace BidService.Application.Interfaces
{
    public interface IBidRepository : IRepository<Bid>
    {
        Task<List<Bid>> GetBidsByAuctionIdAsync(Guid auctionId, CancellationToken cancellationToken = default);
        Task<List<Bid>> GetBidsByBidderAsync(string bidder, CancellationToken cancellationToken = default);
        Task<Bid?> GetHighestBidForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default);
    }
}
