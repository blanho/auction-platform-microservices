using BidService.Application.DTOs;
using BidService.Domain.Entities;
using Common.Repository.Interfaces;

namespace BidService.Application.Interfaces
{
    public interface IBidRepository : IRepository<Bid>
    {
        Task<List<Bid>> GetBidsByAuctionIdAsync(Guid auctionId, CancellationToken cancellationToken = default);
        Task<List<Bid>> GetBidsByBidderUsernameAsync(string bidderUsername, CancellationToken cancellationToken = default);
        Task<Bid?> GetHighestBidForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default);
        Task<BidStatsDto> GetBidStatsAsync(CancellationToken cancellationToken = default);
        Task<List<DailyBidStatDto>> GetDailyBidStatsAsync(int days, CancellationToken cancellationToken = default);
        Task<List<TopBidderDto>> GetTopBiddersAsync(int limit, CancellationToken cancellationToken = default);
    }
}
