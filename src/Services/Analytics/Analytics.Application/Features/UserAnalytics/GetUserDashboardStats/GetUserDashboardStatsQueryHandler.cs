using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;

namespace Analytics.Application.Features.UserAnalytics.GetUserDashboardStats;

public class GetUserDashboardStatsQueryHandler : IRequestHandler<GetUserDashboardStatsQuery, UserDashboardStatsDto>
{
    private readonly IFactAuctionRepository _auctionRepository;
    private readonly IFactBidRepository _bidRepository;

    public GetUserDashboardStatsQueryHandler(IFactAuctionRepository auctionRepository, IFactBidRepository bidRepository)
    {
        _auctionRepository = auctionRepository;
        _bidRepository = bidRepository;
    }

    public async Task<UserDashboardStatsDto> Handle(GetUserDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var auctionStats = await _auctionRepository.GetUserAuctionStatsAsync(request.Username, cancellationToken);
        var bidStats = await _bidRepository.GetUserBidStatsAsync(request.Username, cancellationToken);
        var recentActivity = await _auctionRepository.GetRecentActivityAsync(request.Username, 10, cancellationToken);

        return new UserDashboardStatsDto
        {
            TotalBids = bidStats.TotalBids,
            ItemsWon = bidStats.AuctionsWon,
            WatchlistCount = null,
            ActiveListings = auctionStats.ActiveAuctions,
            TotalListings = auctionStats.TotalAuctions,
            TotalSpent = auctionStats.TotalSpent,
            TotalEarnings = auctionStats.TotalEarned,
            Balance = auctionStats.TotalEarned - auctionStats.TotalSpent,
            SellerRating = null,
            ReviewCount = null,
            RecentActivity = recentActivity
        };
    }
}
