using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityService.DTOs;
using UtilityService.Interfaces;

namespace UtilityService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminScope")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IAnalyticsService analyticsService,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PlatformAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PlatformAnalyticsDto>> GetAnalytics(
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        [FromQuery] string period = "week",
        [FromQuery] Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new AnalyticsQueryParams
        {
            StartDate = startDate,
            EndDate = endDate,
            Period = period,
            CategoryId = categoryId
        };

        var analytics = await _analyticsService.GetPlatformAnalyticsAsync(query, cancellationToken);
        return Ok(analytics);
    }

    [HttpGet("realtime")]
    [ProducesResponseType(typeof(RealTimeStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RealTimeStatsDto>> GetRealTimeStats(CancellationToken cancellationToken)
    {
        var stats = await _analyticsService.GetRealTimeStatsAsync(cancellationToken);
        return Ok(stats);
    }

    [HttpGet("top-performers")]
    [ProducesResponseType(typeof(TopPerformersDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TopPerformersDto>> GetTopPerformers(
        [FromQuery] int limit = 10,
        [FromQuery] string period = "month",
        CancellationToken cancellationToken = default)
    {
        var performers = await _analyticsService.GetTopPerformersAsync(limit, period, cancellationToken);
        return Ok(performers);
    }

    [HttpGet("trends/revenue")]
    [ProducesResponseType(typeof(List<TrendDataPoint>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TrendDataPoint>>> GetRevenueTrend(
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        [FromQuery] string granularity = "day",
        CancellationToken cancellationToken = default)
    {
        var start = startDate ?? DateTimeOffset.UtcNow.AddDays(-30);
        var end = endDate ?? DateTimeOffset.UtcNow;

        var trend = await _analyticsService.GetRevenueTrendAsync(start, end, granularity, cancellationToken);
        return Ok(trend);
    }

    [HttpGet("trends/auctions")]
    [ProducesResponseType(typeof(List<TrendDataPoint>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TrendDataPoint>>> GetAuctionTrend(
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        [FromQuery] string granularity = "day",
        CancellationToken cancellationToken = default)
    {
        var start = startDate ?? DateTimeOffset.UtcNow.AddDays(-30);
        var end = endDate ?? DateTimeOffset.UtcNow;

        var trend = await _analyticsService.GetAuctionTrendAsync(start, end, granularity, cancellationToken);
        return Ok(trend);
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<CategoryBreakdown>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryBreakdown>>> GetCategoryPerformance(
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        CancellationToken cancellationToken = default)
    {
        var categories = await _analyticsService.GetCategoryPerformanceAsync(startDate, endDate, cancellationToken);
        return Ok(categories);
    }

    [HttpGet("auctions")]
    [ProducesResponseType(typeof(AuctionMetrics), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuctionMetrics>> GetAuctionMetrics(
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = new AnalyticsQueryParams
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var metrics = await _analyticsService.GetAuctionMetricsAsync(query, cancellationToken);
        return Ok(metrics);
    }

    [HttpGet("bids")]
    [ProducesResponseType(typeof(BidMetrics), StatusCodes.Status200OK)]
    public async Task<ActionResult<BidMetrics>> GetBidMetrics(
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = new AnalyticsQueryParams
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var metrics = await _analyticsService.GetBidMetricsAsync(query, cancellationToken);
        return Ok(metrics);
    }

    [HttpGet("revenue")]
    [ProducesResponseType(typeof(RevenueMetrics), StatusCodes.Status200OK)]
    public async Task<ActionResult<RevenueMetrics>> GetRevenueMetrics(
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = new AnalyticsQueryParams
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var metrics = await _analyticsService.GetRevenueMetricsAsync(query, cancellationToken);
        return Ok(metrics);
    }
}
