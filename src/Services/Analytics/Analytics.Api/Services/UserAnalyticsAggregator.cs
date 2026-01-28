using Analytics.Api.Helpers;
using Analytics.Api.Interfaces;

namespace Analytics.Api.Services;

public class UserAnalyticsAggregator : IUserAnalyticsAggregator
{
    private readonly IFactAuctionRepository _auctionRepository;
    private readonly IFactBidRepository _bidRepository;
    private readonly ILogger<UserAnalyticsAggregator> _logger;

    public UserAnalyticsAggregator(
        IFactAuctionRepository auctionRepository,
        IFactBidRepository bidRepository,
        ILogger<UserAnalyticsAggregator> logger)
    {
        _auctionRepository = auctionRepository;
        _bidRepository = bidRepository;
        _logger = logger;
    }

    public async Task<UserDashboardStatsDto> GetUserDashboardStatsAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var auctionStatsTask = _auctionRepository.GetUserAuctionStatsAsync(username, cancellationToken);
        var bidStatsTask = _bidRepository.GetUserBidStatsAsync(username, cancellationToken);
        var recentActivityTask = _auctionRepository.GetRecentActivityAsync(username, 10, cancellationToken);

        await Task.WhenAll(auctionStatsTask, bidStatsTask, recentActivityTask);

        var auctionStats = await auctionStatsTask;
        var bidStats = await bidStatsTask;
        var recentActivity = await recentActivityTask;

        var result = new UserDashboardStatsDto
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

        return result;
    }

    public async Task<SellerAnalyticsDto> GetSellerAnalyticsAsync(
        string username,
        string timeRange,
        CancellationToken cancellationToken = default)
    {
        var (startDate, endDate) = AnalyticsHelper.GetDateRange(timeRange);
        var (previousStartDate, previousEndDate) = AnalyticsHelper.GetPreviousPeriod(startDate, endDate);

        var currentStatsTask = _auctionRepository.GetSellerAnalyticsAsync(
            username, startDate, endDate, cancellationToken);
        var previousStatsTask = _auctionRepository.GetSellerAnalyticsAsync(
            username, previousStartDate, previousEndDate, cancellationToken);
        var topListingsTask = _auctionRepository.GetTopListingsAsync(username, 5, cancellationToken);

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

        var result = new SellerAnalyticsDto
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

        return result;
    }

    public async Task<QuickStatsDto> GetQuickStatsAsync(CancellationToken cancellationToken = default)
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
