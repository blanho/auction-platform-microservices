using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using Auctions.Api.Grpc;
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
            .Produces<UserDashboardStatsResponse>();

        group.MapGet("/seller", GetSellerAnalytics)
            .WithName("GetUserSellerAnalytics")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewOwn))
            .Produces<SellerAnalyticsResponse>();

        group.MapGet("/quick-stats", GetQuickStats)
            .WithName("GetQuickStats")
            .AllowAnonymous()
            .Produces<QuickStatsResponse>();

        group.MapGet("/trending-searches", GetTrendingSearches)
            .WithName("GetTrendingSearches")
            .AllowAnonymous()
            .Produces<TrendingSearchesResponse>();
    }

    private static async Task<Ok<UserDashboardStatsResponse>> GetDashboardStats(
        HttpContext httpContext,
        AuctionGrpc.AuctionGrpcClient auctionClient,
        CancellationToken cancellationToken)
    {
        var username = httpContext.User.Identity?.Name
            ?? httpContext.User.FindFirst("username")?.Value
            ?? "Anonymous";

        var response = await auctionClient.GetUserDashboardStatsAsync(
            new GetUserDashboardStatsRequest { Username = username },
            cancellationToken: cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<Ok<SellerAnalyticsResponse>> GetSellerAnalytics(
        string? timeRange,
        HttpContext httpContext,
        AuctionGrpc.AuctionGrpcClient auctionClient,
        CancellationToken cancellationToken)
    {
        var username = httpContext.User.Identity?.Name
            ?? httpContext.User.FindFirst("username")?.Value
            ?? "Anonymous";

        var response = await auctionClient.GetSellerAnalyticsAsync(
            new GetSellerAnalyticsRequest 
            { 
                Username = username, 
                TimeRange = timeRange ?? "30d" 
            },
            cancellationToken: cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<Ok<QuickStatsResponse>> GetQuickStats(
        AuctionGrpc.AuctionGrpcClient auctionClient,
        CancellationToken cancellationToken)
    {
        var response = await auctionClient.GetQuickStatsAsync(
            new GetQuickStatsRequest(),
            cancellationToken: cancellationToken);

        return TypedResults.Ok(response);
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
