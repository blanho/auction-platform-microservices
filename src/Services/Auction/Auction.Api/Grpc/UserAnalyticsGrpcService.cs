using Auctions.Api.Grpc;
using Auctions.Application.Queries.GetTrendingSearches;
using Grpc.Core;

namespace Auctions.Api.Grpc;

public partial class AuctionGrpcService
{
    public override async Task<UserDashboardStatsResponse> GetUserDashboardStats(
        GetUserDashboardStatsRequest request,
        ServerCallContext context)
    {
        var userAuctions = await _auctionRepository.GetBySellerUsernameAsync(request.Username, context.CancellationToken);
        var wonAuctions = await _auctionRepository.GetWonByUsernameAsync(request.Username, context.CancellationToken);
        var watchlistCount = await _auctionRepository.GetWatchlistCountByUsernameAsync(request.Username, context.CancellationToken);

        var activeAuctions = userAuctions.Count(a => a.Status == BuildingBlocks.Domain.Enums.Status.Live);
        var totalAuctions = userAuctions.Count;
        var itemsWon = wonAuctions.Count;
        var totalSpent = wonAuctions.Sum(a => a.SoldAmount ?? 0m);
        var totalEarned = userAuctions
            .Where(a => a.Status == BuildingBlocks.Domain.Enums.Status.Finished && a.SoldAmount.HasValue)
            .Sum(a => a.SoldAmount ?? 0m);

        return new UserDashboardStatsResponse
        {
            ActiveAuctions = activeAuctions,
            TotalAuctions = totalAuctions,
            WonAuctions = itemsWon,
            LostAuctions = 0,
            TotalSpent = decimal.ToDouble(totalSpent),
            TotalEarned = decimal.ToDouble(totalEarned),
            ActiveBids = 0,
            WatchingCount = watchlistCount
        };
    }

    public override async Task<SellerAnalyticsResponse> GetSellerAnalytics(
        GetSellerAnalyticsRequest request,
        ServerCallContext context)
    {
        var daysBack = request.TimeRange switch
        {
            "7d" => 7,
            "30d" => 30,
            "90d" => 90,
            "1y" => 365,
            _ => 30
        };

        var periodStart = DateTimeOffset.UtcNow.AddDays(-daysBack);
        var previousPeriodStart = periodStart.AddDays(-daysBack);

        var stats = await _auctionRepository.GetSellerStatsAsync(
            request.Username,
            periodStart,
            previousPeriodStart,
            context.CancellationToken);

        var response = new SellerAnalyticsResponse
        {
            TotalAuctions = stats.TotalListings,
            ActiveAuctions = stats.ActiveListings,
            CompletedAuctions = stats.ItemsSold,
            CancelledAuctions = 0,
            TotalRevenue = decimal.ToDouble(stats.TotalRevenue),
            AverageFinalPrice = stats.ItemsSold > 0 ? decimal.ToDouble(stats.TotalRevenue / stats.ItemsSold) : 0,
            TotalBidsReceived = 0,
            SuccessRate = stats.TotalListings > 0 ? (double)stats.ItemsSold / stats.TotalListings * 100 : 0
        };

        foreach (var sale in stats.RecentSales)
        {
            response.DailyRevenue.Add(new DailyRevenue
            {
                Date = sale.SoldAt?.ToString("yyyy-MM-dd") ?? string.Empty,
                Revenue = decimal.ToDouble(sale.SoldAmount ?? 0),
                AuctionsCompleted = 1
            });
        }

        return response;
    }

    public override async Task<TrendingSearchesResponse> GetTrendingSearches(
        GetTrendingSearchesRequest request,
        ServerCallContext context)
    {
        var query = new GetTrendingSearchesQuery(request.Limit);
        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return new TrendingSearchesResponse();
        }

        var response = new TrendingSearchesResponse();
        foreach (var search in result.Value)
        {
            response.Searches.Add(new TrendingSearch
            {
                Keyword = search.SearchTerm,
                SearchCount = search.Count
            });
        }

        return response;
    }
}
