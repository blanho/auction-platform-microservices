using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using BuildingBlocks.Web.Authorization;

namespace Analytics.Api.Endpoints;

public class AnalyticsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/analytics")
            .WithTags("Analytics")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewPlatform));

        group.MapGet("", GetAnalytics)
            .WithName("GetAnalytics")
            .Produces<PlatformAnalyticsDto>();

        group.MapGet("/realtime", GetRealTimeStats)
            .WithName("GetRealTimeStats")
            .Produces<RealTimeStatsDto>();

        group.MapGet("/top-performers", GetTopPerformers)
            .WithName("GetTopPerformers")
            .Produces<TopPerformersDto>();

        group.MapGet("/trends/revenue", GetRevenueTrend)
            .WithName("GetRevenueTrend")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewRevenue))
            .Produces<List<TrendDataPoint>>();

        group.MapGet("/trends/auctions", GetAuctionTrend)
            .WithName("GetAuctionTrend")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewAuctions))
            .Produces<List<TrendDataPoint>>();

        group.MapGet("/categories", GetCategoryPerformance)
            .WithName("GetCategoryPerformance")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewCategories))
            .Produces<List<CategoryBreakdown>>();

        group.MapGet("/auctions", GetAuctionMetrics)
            .WithName("GetAuctionMetrics")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewAuctions))
            .Produces<AuctionMetrics>();

        group.MapGet("/bids", GetBidMetrics)
            .WithName("GetBidMetrics")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewBids))
            .Produces<BidMetrics>();

        group.MapGet("/revenue", GetRevenueMetrics)
            .WithName("GetRevenueMetrics")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewRevenue))
            .Produces<RevenueMetrics>();
    }

    private static async Task<Ok<PlatformAnalyticsDto>> GetAnalytics(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        string period,
        Guid? categoryId,
        IAnalyticsService analyticsService,
        CancellationToken cancellationToken)
    {
        var query = new AnalyticsQueryParams
        {
            StartDate = startDate,
            EndDate = endDate,
            Period = period ?? "week",
            CategoryId = categoryId
        };

        var analytics = await analyticsService.GetPlatformAnalyticsAsync(query, cancellationToken);
        return TypedResults.Ok(analytics);
    }

    private static async Task<Ok<RealTimeStatsDto>> GetRealTimeStats(
        IAnalyticsService analyticsService,
        CancellationToken cancellationToken)
    {
        var stats = await analyticsService.GetRealTimeStatsAsync(cancellationToken);
        return TypedResults.Ok(stats);
    }

    private static async Task<Ok<TopPerformersDto>> GetTopPerformers(
        int? limit,
        string? period,
        IAnalyticsService analyticsService,
        CancellationToken cancellationToken)
    {
        var performers = await analyticsService.GetTopPerformersAsync(
            limit ?? AnalyticsDefaults.DefaultLimit,
            period ?? AnalyticsDefaults.DefaultPeriod,
            cancellationToken);
        return TypedResults.Ok(performers);
    }

    private static async Task<Ok<List<TrendDataPoint>>> GetRevenueTrend(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        string? granularity,
        IAnalyticsService analyticsService,
        CancellationToken cancellationToken)
    {
        var start = startDate ?? DateTimeOffset.UtcNow.AddDays(-AnalyticsDefaults.DefaultDays);
        var end = endDate ?? DateTimeOffset.UtcNow;

        var trend = await analyticsService.GetRevenueTrendAsync(
            start, end, granularity ?? AnalyticsDefaults.DefaultGranularity, cancellationToken);
        return TypedResults.Ok(trend);
    }

    private static async Task<Ok<List<TrendDataPoint>>> GetAuctionTrend(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        string? granularity,
        IAnalyticsService analyticsService,
        CancellationToken cancellationToken)
    {
        var start = startDate ?? DateTimeOffset.UtcNow.AddDays(-AnalyticsDefaults.DefaultDays);
        var end = endDate ?? DateTimeOffset.UtcNow;

        var trend = await analyticsService.GetAuctionTrendAsync(
            start, end, granularity ?? AnalyticsDefaults.DefaultGranularity, cancellationToken);
        return TypedResults.Ok(trend);
    }

    private static async Task<Ok<List<CategoryBreakdown>>> GetCategoryPerformance(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        IAnalyticsService analyticsService,
        CancellationToken cancellationToken)
    {
        var categories = await analyticsService.GetCategoryPerformanceAsync(startDate, endDate, cancellationToken);
        return TypedResults.Ok(categories);
    }

    private static async Task<Ok<AuctionMetrics>> GetAuctionMetrics(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        IAnalyticsService analyticsService,
        CancellationToken cancellationToken)
    {
        var query = new AnalyticsQueryParams
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var metrics = await analyticsService.GetAuctionMetricsAsync(query, cancellationToken);
        return TypedResults.Ok(metrics);
    }

    private static async Task<Ok<BidMetrics>> GetBidMetrics(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        IAnalyticsService analyticsService,
        CancellationToken cancellationToken)
    {
        var query = new AnalyticsQueryParams
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var metrics = await analyticsService.GetBidMetricsAsync(query, cancellationToken);
        return TypedResults.Ok(metrics);
    }

    private static async Task<Ok<RevenueMetrics>> GetRevenueMetrics(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        IAnalyticsService analyticsService,
        CancellationToken cancellationToken)
    {
        var query = new AnalyticsQueryParams
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var metrics = await analyticsService.GetRevenueMetricsAsync(query, cancellationToken);
        return TypedResults.Ok(metrics);
    }
}
