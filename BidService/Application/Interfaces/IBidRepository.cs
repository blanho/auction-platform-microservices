using BidService.Domain.Entities;
using Common.Repository.Interfaces;

namespace BidService.Application.Interfaces
{
    public record BidStatsDto(
        int TotalBids,
        int UniqueBidders,
        decimal TotalBidAmount,
        decimal AverageBidAmount,
        int BidsToday,
        int BidsThisWeek,
        int BidsThisMonth
    );

    public record DailyBidStatDto(DateOnly Date, int BidCount, decimal TotalAmount);

    public record TopBidderDto(
        Guid BidderId,
        string Username,
        int BidCount,
        decimal TotalAmount,
        int AuctionsWon
    );

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
