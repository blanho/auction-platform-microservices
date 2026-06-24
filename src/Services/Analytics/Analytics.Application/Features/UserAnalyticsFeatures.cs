using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Application.Helpers;

using Microsoft.Extensions.Logging;

namespace Analytics.Application.Features.UserAnalytics;

public record GetUserDashboardStatsQuery(string Username) : IRequest<UserDashboardStatsDto>;
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
        var auctionStatsTask = _auctionRepository.GetUserAuctionStatsAsync(request.Username, cancellationToken);
        var bidStatsTask = _bidRepository.GetUserBidStatsAsync(request.Username, cancellationToken);
        var recentActivityTask = _auctionRepository.GetRecentActivityAsync(request.Username, 10, cancellationToken);

        await Task.WhenAll(auctionStatsTask, bidStatsTask, recentActivityTask);

        var auctionStats = await auctionStatsTask;
        var bidStats = await bidStatsTask;
        var recentActivity = await recentActivityTask;

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

public record GetSellerAnalyticsQuery(string Username, string TimeRange) : IRequest<SellerAnalyticsDto>;
public class GetSellerAnalyticsQueryHandler : IRequestHandler<GetSellerAnalyticsQuery, SellerAnalyticsDto>
{
    private readonly IFactAuctionRepository _auctionRepository;

    public GetSellerAnalyticsQueryHandler(IFactAuctionRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }

    public async Task<SellerAnalyticsDto> Handle(GetSellerAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = AnalyticsHelper.GetDateRange(request.TimeRange);
        var (previousStartDate, previousEndDate) = AnalyticsHelper.GetPreviousPeriod(startDate, endDate);

        var currentStatsTask = _auctionRepository.GetSellerAnalyticsAsync(request.Username, startDate, endDate, cancellationToken);
        var previousStatsTask = _auctionRepository.GetSellerAnalyticsAsync(request.Username, previousStartDate, previousEndDate, cancellationToken);
        var topListingsTask = _auctionRepository.GetTopListingsAsync(request.Username, 5, cancellationToken);

        await Task.WhenAll(currentStatsTask, previousStatsTask, topListingsTask);

        var currentStats = await currentStatsTask;
        var previousStats = await previousStatsTask;
        var topListings = await topListingsTask;

        var revenueChange = AnalyticsHelper.CalculatePercentageChange(previousStats.TotalRevenue, currentStats.TotalRevenue);
        var itemsSoldChange = AnalyticsHelper.CalculatePercentageChange(previousStats.CompletedAuctions, currentStats.CompletedAuctions);
        var avgPriceChange = AnalyticsHelper.CalculatePercentageChange(previousStats.AverageFinalPrice, currentStats.AverageFinalPrice);

        var salesChart = currentStats.DailyRevenue
            .Select(d => new SalesChartDataDto
            {
                Date = d.Date.ToString("yyyy-MM-dd"),
                Amount = d.Revenue,
                Count = d.AuctionsCompleted
            })
            .ToList();

        return new SellerAnalyticsDto
        {
            TotalRevenue = currentStats.TotalRevenue,
            RevenueChange = revenueChange,
            ItemsSold = currentStats.CompletedAuctions,
            ItemsSoldChange = itemsSoldChange,
            AveragePrice = currentStats.AverageFinalPrice,
            AveragePriceChange = avgPriceChange,
            TotalViews = null,
            ViewsChange = null,
            TopListings = topListings,
            SalesChart = salesChart
        };
    }
}

public record GetQuickStatsQuery() : IRequest<QuickStatsDto>;
public class GetQuickStatsQueryHandler : IRequestHandler<GetQuickStatsQuery, QuickStatsDto>
{
    private readonly IFactAuctionRepository _auctionRepository;

    public GetQuickStatsQueryHandler(IFactAuctionRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }

    public async Task<QuickStatsDto> Handle(GetQuickStatsQuery request, CancellationToken cancellationToken)
    {
        var liveAuctions = await _auctionRepository.GetLiveAuctionsCountAsync(cancellationToken);

        return new QuickStatsDto
        {
            LiveAuctions = liveAuctions,
            LiveAuctionsChange = null,
            ActiveUsers = 0,
            ActiveUsersChange = null,
            EndingSoon = 0,
            EndingSoonChange = null
        };
    }
}

public record GetTrendingSearchesQuery(int Limit) : IRequest<TrendingSearchesResponse>;
public class GetTrendingSearchesQueryHandler : IRequestHandler<GetTrendingSearchesQuery, TrendingSearchesResponse>
{
    private readonly IFactAuctionRepository _auctionRepository;

    public GetTrendingSearchesQueryHandler(IFactAuctionRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }

    public async Task<TrendingSearchesResponse> Handle(GetTrendingSearchesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _auctionRepository.GetCategoryPerformanceAsync(
            DateTimeOffset.UtcNow.AddDays(-7),
            DateTimeOffset.UtcNow,
            cancellationToken);

        var searches = categories
            .Where(c => !string.IsNullOrWhiteSpace(c.CategoryName))
            .OrderByDescending(c => c.BidCount)
            .Take(request.Limit)
            .Select(c => new TrendingSearchDto
            {
                Query = c.CategoryName,
                Count = c.BidCount
            })
            .ToList();

        return new TrendingSearchesResponse { Searches = searches };
    }
}
