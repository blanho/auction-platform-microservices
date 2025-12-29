#nullable enable
using AuctionService.Application.DTOs;
using AuctionService.Application.Queries.GetSellerAnalytics;
using AuctionService.Application.Queries.GetQuickStats;
using AuctionService.Application.Queries.GetTrendingSearches;
using AuctionService.Application.Queries.GetUserDashboardStats;
using Carter;
using Common.Core.Authorization;
using Common.Core.Helpers;
using Common.Utilities.Helpers;
using MediatR;

namespace AuctionService.API.Endpoints.Auctions;

public class AuctionAnalyticsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auctions")
            .WithTags("Auction Analytics")
            .WithOpenApi();

        group.MapGet("/dashboard/stats", GetDashboardStats)
            .WithName("GetDashboardStats")
            .RequireAuthorization($"Permission:{Permissions.Analytics.ViewOwn}")
            .Produces<UserDashboardStatsDto>(StatusCodes.Status200OK);

        group.MapGet("/analytics/seller", GetSellerAnalytics)
            .WithName("GetSellerAnalytics")
            .RequireAuthorization($"Permission:{Permissions.Analytics.ViewSeller}")
            .Produces<SellerAnalyticsDto>(StatusCodes.Status200OK);

        group.MapGet("/analytics/quick-stats", GetQuickStats)
            .WithName("GetQuickStats")
            .AllowAnonymous()
            .Produces<QuickStatsDto>(StatusCodes.Status200OK);

        group.MapGet("/analytics/trending-searches", GetTrendingSearches)
            .WithName("GetTrendingSearches")
            .AllowAnonymous()
            .Produces<List<TrendingSearchDto>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetDashboardStats(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var username = UserHelper.GetUsername(httpContext.User);
        var query = new GetUserDashboardStatsQuery(username);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetSellerAnalytics(
        string? timeRange,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var username = UserHelper.GetUsername(httpContext.User);
        var query = new GetSellerAnalyticsQuery(username, timeRange ?? "30d");
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetQuickStats(
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetQuickStatsQuery();
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetTrendingSearches(
        int? limit,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetTrendingSearchesQuery(limit ?? 6);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }
}
