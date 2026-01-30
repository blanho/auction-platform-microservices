using Bidding.Domain.Entities;
using Bidding.Domain.Enums;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Paging;
using BuildingBlocks.Domain.Entities;

namespace Bidding.Application.Interfaces
{
    public interface IBidRepository : IRepository<Bid>
    {
        Task<List<Bid>> GetBidsByAuctionIdAsync(Guid auctionId, CancellationToken cancellationToken = default);
        Task<List<Bid>> GetBidsByBidderUsernameAsync(string bidderUsername, CancellationToken cancellationToken = default);
        Task<Bid?> GetHighestBidForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default);
        Task<Bid?> GetSecondHighestBidForAuctionAsync(Guid auctionId, Guid excludeBidId, CancellationToken cancellationToken = default);
        Task<BidStatsDto> GetBidStatsAsync(CancellationToken cancellationToken = default);
        Task<UserBidStatsDto> GetUserBidStatsAsync(string username, CancellationToken cancellationToken = default);
        Task<List<DailyBidStatDto>> GetDailyBidStatsAsync(int days, CancellationToken cancellationToken = default);
        Task<List<TopBidderDto>> GetTopBiddersAsync(int limit, CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, int>> GetBidCountsForAuctionsAsync(List<Guid> auctionIds, CancellationToken cancellationToken = default);

        Task<PaginatedResult<Bid>> GetWinningBidsForUserAsync(Guid userId, QueryParameters queryParams, CancellationToken cancellationToken = default);
        Task<int> GetWinningBidsCountForUserAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<PaginatedResult<Bid>> GetBidHistoryAsync(BidHistoryQueryParams queryParams, CancellationToken cancellationToken = default);
    }

    public class BidHistoryFilter
    {
        public Guid? AuctionId { get; init; }
        public Guid? UserId { get; init; }
        public BidStatus? Status { get; init; }
        public DateTimeOffset? FromDate { get; init; }
        public DateTimeOffset? ToDate { get; init; }
    }

    public class BidHistoryQueryParams : QueryParameters<BidHistoryFilter> { }
}
