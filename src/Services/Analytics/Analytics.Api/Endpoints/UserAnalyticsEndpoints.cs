using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using Auctions.Api.Grpc;
using Analytics.Api.Interfaces;
using BuildingBlocks.Web.Authorization;

namespace Analytics.Api.Endpoints;

public class UserAnalyticsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/analytics/user")
            .WithTags("User Analytics");

        group.MapGet("/dashboard", GetDashboardStats)
            .WithName("GetUserDashboardStats")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewOwn))
            .Produces<UserDashboardStatsDto>();

        group.MapGet("/seller", GetSellerAnalytics)
            .WithName("GetUserSellerAnalytics")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewOwn))
            .Produces<SellerAnalyticsDto>();

        group.MapGet("/quick-stats", GetQuickStats)
            .WithName("GetQuickStats")
            .AllowAnonymous()
            .Produces<QuickStatsDto>();

        group.MapGet("/trending-searches", GetTrendingSearches)
            .WithName("GetTrendingSearches")
            .AllowAnonymous()
            .Produces<TrendingSearchesResponse>();
    }

    private static async Task<Ok<UserDashboardStatsDto>> GetDashboardStats(
        HttpContext httpContext,
        IUserAnalyticsAggregator aggregator,
        CancellationToken cancellationToken)
    {
        var username = httpContext.User.Identity?.Name
            ?? httpContext.User.FindFirst("username")?.Value
            ?? "Anonymous";

        var stats = await aggregator.GetUserDashboardStatsAsync(username, cancellationToken);

        return TypedResults.Ok(stats);
    }

    private static async Task<Ok<SellerAnalyticsDto>> GetSellerAnalytics(
        string? timeRange,
        HttpContext httpContext,
        IUserAnalyticsAggregator aggregator,
        CancellationToken cancellationToken)
    {
        var username = httpContext.User.Identity?.Name
            ?? httpContext.User.FindFirst("username")?.Value
            ?? "Anonymous";

        var analytics = await aggregator.GetSellerAnalyticsAsync(
            username, 
            timeRange ?? "30d", 
            cancellationToken);

        return TypedResults.Ok(analytics);
    }

    private static async Task<Ok<QuickStatsDto>> GetQuickStats(
        IUserAnalyticsAggregator aggregator,
        CancellationToken cancellationToken)
    {
        var stats = await aggregator.GetQuickStatsAsync(cancellationToken);

        return TypedResults.Ok(stats);
    }

    private static async Task<Ok<TrendingSearchesResponse>> GetTrendingSearches(
        int? limit,
        AuctionGrpc.AuctionGrpcClient auctionClient,
        CancellationToken cancellationToken)
    {
        var response = await auctionClient.GetTrendingSearchesAsync(
            new GetTrendingSearchesRequest { Limit = limit ?? 6 },
            cancellationToken: cancellationToken);

        return TypedResults.Ok(response);
    }
}
