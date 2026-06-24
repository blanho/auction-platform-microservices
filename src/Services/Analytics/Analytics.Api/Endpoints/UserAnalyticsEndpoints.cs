using Carter;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Analytics.Application.DTOs;
using Analytics.Application.Features.UserAnalytics;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;

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
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewOwn))
            .Produces<QuickStatsDto>();

        group.MapGet("/trending-searches", GetTrendingSearches)
            .WithName("GetTrendingSearches")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewOwn))
            .Produces<TrendingSearchesResponse>();
    }

    private static async Task<Ok<UserDashboardStatsDto>> GetDashboardStats(
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(httpContext.User);
        var stats = await sender.Send(new GetUserDashboardStatsQuery(username), cancellationToken);
        return TypedResults.Ok(stats);
    }

    private static async Task<Ok<SellerAnalyticsDto>> GetSellerAnalytics(
        string? timeRange,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(httpContext.User);
        var analytics = await sender.Send(new GetSellerAnalyticsQuery(username, timeRange ?? "30d"), cancellationToken);
        return TypedResults.Ok(analytics);
    }

    private static async Task<Ok<QuickStatsDto>> GetQuickStats(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var stats = await sender.Send(new GetQuickStatsQuery(), cancellationToken);
        return TypedResults.Ok(stats);
    }

    private static async Task<Ok<TrendingSearchesResponse>> GetTrendingSearches(
        int? limit,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var searches = await sender.Send(new GetTrendingSearchesQuery(limit ?? 10), cancellationToken);
        return TypedResults.Ok(searches);
    }
}
