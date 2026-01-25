using Auctions.Api.Grpc;
using Auctions.Application.Queries.GetUserDashboardStats;
using Auctions.Application.Queries.GetSellerAnalytics;
using Auctions.Application.Queries.GetQuickStats;
using Auctions.Application.Queries.GetTrendingSearches;
using Grpc.Core;

namespace Auctions.Api.Grpc;

public partial class AuctionGrpcService
{
    public override async Task<UserDashboardStatsResponse> GetUserDashboardStats(
        GetUserDashboardStatsRequest request,
        ServerCallContext context)
    {
        var query = new GetUserDashboardStatsQuery(request.Username);
        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return new UserDashboardStatsResponse();
        }

        var stats = result.Value;
        return new UserDashboardStatsResponse
        {
            ActiveAuctions = stats.ActiveListings,
            TotalAuctions = stats.TotalListings,
            WonAuctions = stats.ItemsWon,
            LostAuctions = 0,
            TotalSpent = decimal.ToDouble(stats.TotalSpent),
            TotalEarned = decimal.ToDouble(stats.TotalEarnings),
            ActiveBids = stats.TotalBids,
            WatchingCount = stats.WatchlistCount
        };
    }

    public override async Task<SellerAnalyticsResponse> GetSellerAnalytics(
        GetSellerAnalyticsRequest request,
        ServerCallContext context)
    {
        var query = new GetSellerAnalyticsQuery(request.Username, request.TimeRange);
        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return new SellerAnalyticsResponse();
        }

        var analytics = result.Value;
        var response = new SellerAnalyticsResponse
        {
            TotalAuctions = analytics.ItemsSold + analytics.ActiveListings,
            ActiveAuctions = analytics.ActiveListings,
            CompletedAuctions = analytics.ItemsSold,
            CancelledAuctions = 0,
            TotalRevenue = decimal.ToDouble(analytics.TotalRevenue),
            AverageFinalPrice = analytics.ItemsSold > 0 ? decimal.ToDouble(analytics.TotalRevenue / analytics.ItemsSold) : 0,
            TotalBidsReceived = 0,
            SuccessRate = 0
        };

        if (analytics.ChartData is not null)
        {
            foreach (var daily in analytics.ChartData)
            {
                response.DailyRevenue.Add(new DailyRevenue
                {
                    Date = daily.Date,
                    Revenue = decimal.ToDouble(daily.Revenue),
                    AuctionsCompleted = daily.Bids
                });
            }
        }

        return response;
    }

    public override async Task<QuickStatsResponse> GetQuickStats(
        GetQuickStatsRequest request,
        ServerCallContext context)
    {
        var query = new GetQuickStatsQuery();
        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return new QuickStatsResponse();
        }

        var stats = result.Value;
        return new QuickStatsResponse
        {
            LiveAuctions = stats.LiveAuctions,
            EndingSoon = stats.EndingSoon,
            NewToday = 0
        };
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
