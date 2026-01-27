using Analytics.Api.Interfaces;
using Auctions.Api.Grpc;
using BidService.Contracts.Grpc;
using IdentityService.Contracts.Grpc;
using BuildingBlocks.Application.Abstractions.Caching;

namespace Analytics.Api.Services;

public class UserAnalyticsAggregator(
    AuctionGrpc.AuctionGrpcClient auctionClient,
    BidStatsGrpc.BidStatsGrpcClient bidStatsClient,
    UserStatsGrpc.UserStatsGrpcClient userStatsClient,
    ICacheService cacheService,
    ILogger<UserAnalyticsAggregator> logger)
    : IUserAnalyticsAggregator
{
    private readonly AuctionGrpc.AuctionGrpcClient _auctionClient = auctionClient;
    private readonly BidStatsGrpc.BidStatsGrpcClient _bidStatsClient = bidStatsClient;
    private readonly UserStatsGrpc.UserStatsGrpcClient _userStatsClient = userStatsClient;
    private readonly ICacheService _cacheService = cacheService;
    private readonly ILogger<UserAnalyticsAggregator> _logger = logger;

    private const int CacheDurationMinutes = 5;

    public async Task<UserDashboardStatsDto> GetUserDashboardStatsAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"user-dashboard:{username}";
        
        var cached = await _cacheService.GetAsync<UserDashboardStatsDto>(cacheKey, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for user dashboard stats: {Username}", username);
            return cached;
        }

        var auctionStats = await _auctionClient.GetUserDashboardStatsAsync(
            new GetUserDashboardStatsRequest { Username = username },
            cancellationToken: cancellationToken);

        var bidStats = await _bidStatsClient.GetUserBidStatsAsync(
            new GetUserBidStatsRequest { Username = username },
            cancellationToken: cancellationToken);

        var userStats = await _userStatsClient.GetUserStatsAsync(
            new GetUserStatsRequest { Username = username },
            cancellationToken: cancellationToken);

        var result = new UserDashboardStatsDto
        {
            TotalBids = bidStats.TotalBids,
            ItemsWon = bidStats.AuctionsWon,
            WatchlistCount = auctionStats.WatchingCount,
            ActiveListings = auctionStats.ActiveAuctions,
            TotalListings = auctionStats.TotalAuctions,
            TotalSpent = (decimal)auctionStats.TotalSpent,
            TotalEarnings = (decimal)auctionStats.TotalEarned,
            Balance = (decimal)(auctionStats.TotalEarned - auctionStats.TotalSpent),
            SellerRating = (decimal)userStats.SellerRating,
            ReviewCount = userStats.ReviewCount,
            RecentActivity = []
        };

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(CacheDurationMinutes), cancellationToken);
        _logger.LogDebug("Cached user dashboard stats: {Username}", username);

        return result;
    }

    public async Task<SellerAnalyticsDto> GetSellerAnalyticsAsync(
        string username,
        string timeRange,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"seller-analytics:{username}:{timeRange}";
        
        var cached = await _cacheService.GetAsync<SellerAnalyticsDto>(cacheKey, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for seller analytics: {Username}, {TimeRange}", username, timeRange);
            return cached;
        }

        var sellerStats = await _auctionClient.GetSellerAnalyticsAsync(
            new GetSellerAnalyticsRequest { Username = username, TimeRange = timeRange },
            cancellationToken: cancellationToken);

        var bidStats = await _bidStatsClient.GetUserBidStatsAsync(
            new GetUserBidStatsRequest { Username = username },
            cancellationToken: cancellationToken);

        var salesChart = sellerStats.DailyRevenue
            .Select(d => new SalesChartDataDto
            {
                Date = d.Date,
                Amount = (decimal)d.Revenue,
                Count = d.AuctionsCompleted
            })
            .ToList();

        var result = new SellerAnalyticsDto
        {
            TotalRevenue = (decimal)sellerStats.TotalRevenue,
            RevenueChange = 0,
            ItemsSold = sellerStats.CompletedAuctions,
            ItemsSoldChange = 0,
            AveragePrice = (decimal)sellerStats.AverageFinalPrice,
            AveragePriceChange = 0,
            TotalViews = 0,
            ViewsChange = 0,
            TopListings = [],
            SalesChart = salesChart
        };

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(CacheDurationMinutes), cancellationToken);
        _logger.LogDebug("Cached seller analytics: {Username}, {TimeRange}", username, timeRange);

        return result;
    }

    public async Task<QuickStatsDto> GetQuickStatsAsync(CancellationToken cancellationToken = default)
    {
        var auctionStats = await _auctionClient.GetAuctionStatsAsync(
            new GetAuctionStatsRequest(),
            cancellationToken: cancellationToken);

        var userStats = await _userStatsClient.GetPlatformUserStatsAsync(
            new GetPlatformUserStatsRequest(),
            cancellationToken: cancellationToken);

        return new QuickStatsDto
        {
            LiveAuctions = auctionStats.LiveAuctions,
            LiveAuctionsChange = null,
            ActiveUsers = userStats.ActiveUsers,
            ActiveUsersChange = null,
            EndingSoon = 0,
            EndingSoonChange = null
        };
    }
}
