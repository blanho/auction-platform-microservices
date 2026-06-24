using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Features.PlatformAnalytics;

public record GetPlatformAnalyticsQuery(AnalyticsQueryParams Query) : IRequest<PlatformAnalyticsDto>;
public class GetPlatformAnalyticsQueryHandler : IRequestHandler<GetPlatformAnalyticsQuery, PlatformAnalyticsDto>
{
    private readonly IFactAuctionRepository _auctionRepository;
    private readonly IFactBidRepository _bidRepository;
    private readonly IFactPaymentRepository _paymentRepository;

    public GetPlatformAnalyticsQueryHandler(IFactAuctionRepository auctionRepository, IFactBidRepository bidRepository, IFactPaymentRepository paymentRepository)
    {
        _auctionRepository = auctionRepository;
        _bidRepository = bidRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<PlatformAnalyticsDto> Handle(GetPlatformAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.Query.StartDate ?? DateTimeOffset.UtcNow.AddDays(-AnalyticsDefaults.DefaultDays);
        var endDate = request.Query.EndDate ?? DateTimeOffset.UtcNow;

        var auctionTask = _auctionRepository.GetAuctionMetricsAsync(startDate, endDate, cancellationToken);
        var bidTask = _bidRepository.GetBidMetricsAsync(startDate, endDate, cancellationToken);
        var revenueTask = _paymentRepository.GetRevenueMetricsAsync(startDate, endDate, cancellationToken);
        var categoryTask = _auctionRepository.GetCategoryPerformanceAsync(startDate, endDate, cancellationToken);

        await Task.WhenAll(auctionTask, bidTask, revenueTask, categoryTask);

        var auctionMetrics = await auctionTask;
        var bidMetrics = await bidTask;
        var revenueMetrics = await revenueTask;
        var categoryPerformance = await categoryTask;

        return new PlatformAnalyticsDto
        {
            Overview = new OverviewMetrics
            {
                TotalAuctions = auctionMetrics.LiveAuctions + auctionMetrics.CompletedAuctions,
                TotalBids = bidMetrics.TotalBids,
                TotalRevenue = revenueMetrics.TotalRevenue
            },
            Auctions = auctionMetrics,
            Bids = bidMetrics,
            Revenue = revenueMetrics,
            CategoryPerformance = categoryPerformance
        };
    }
}

public record GetTopPerformersQuery(int Limit, string Period) : IRequest<TopPerformersDto>;
public class GetTopPerformersQueryHandler : IRequestHandler<GetTopPerformersQuery, TopPerformersDto>
{
    private readonly IFactAuctionRepository _auctionRepository;
    private readonly IFactPaymentRepository _paymentRepository;

    public GetTopPerformersQueryHandler(IFactAuctionRepository auctionRepository, IFactPaymentRepository paymentRepository)
    {
        _auctionRepository = auctionRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<TopPerformersDto> Handle(GetTopPerformersQuery request, CancellationToken cancellationToken)
    {
        var startDate = GetPeriodStartDate(request.Period);
        var topAuctionsTask = _auctionRepository.GetTopAuctionsAsync(request.Limit, cancellationToken);
        var topSellersTask = _paymentRepository.GetTopSellersAsync(request.Limit, startDate, cancellationToken);
        var topBuyersTask = _paymentRepository.GetTopBuyersAsync(request.Limit, startDate, cancellationToken);

        await Task.WhenAll(topAuctionsTask, topSellersTask, topBuyersTask);

        return new TopPerformersDto
        {
            TopAuctions = await topAuctionsTask,
            TopSellers = await topSellersTask,
            TopBuyers = await topBuyersTask
        };
    }

    private static DateTimeOffset? GetPeriodStartDate(string period)
    {
        var now = DateTimeOffset.UtcNow;
        return period.ToLowerInvariant() switch
        {
            "day" => now.AddDays(-1),
            "week" => now.AddDays(-7),
            "month" => now.AddMonths(-1),
            "year" => now.AddYears(-1),
            "all" => null,
            _ => now.AddMonths(-1)
        };
    }
}

public record GetRealTimeStatsQuery() : IRequest<RealTimeStatsDto>;
public class GetRealTimeStatsQueryHandler : IRequestHandler<GetRealTimeStatsQuery, RealTimeStatsDto>
{
    private readonly IFactAuctionRepository _auctionRepository;
    private readonly IFactBidRepository _bidRepository;

    public GetRealTimeStatsQueryHandler(IFactAuctionRepository auctionRepository, IFactBidRepository bidRepository)
    {
        _auctionRepository = auctionRepository;
        _bidRepository = bidRepository;
    }

    public async Task<RealTimeStatsDto> Handle(GetRealTimeStatsQuery request, CancellationToken cancellationToken)
    {
        var liveAuctionsTask = _auctionRepository.GetLiveAuctionsCountAsync(cancellationToken);
        var bidsLastHourTask = _bidRepository.GetBidsInLastHourAsync(cancellationToken);

        await Task.WhenAll(liveAuctionsTask, bidsLastHourTask);

        return new RealTimeStatsDto
        {
            ActiveAuctions = await liveAuctionsTask,
            BidsLastHour = await bidsLastHourTask
        };
    }
}

public record GetRevenueTrendQuery(DateTimeOffset StartDate, DateTimeOffset EndDate, string Granularity) : IRequest<List<TrendDataPoint>>;
public class GetRevenueTrendQueryHandler : IRequestHandler<GetRevenueTrendQuery, List<TrendDataPoint>>
{
    private readonly IFactPaymentRepository _paymentRepository;
    public GetRevenueTrendQueryHandler(IFactPaymentRepository paymentRepository) => _paymentRepository = paymentRepository;

    public async Task<List<TrendDataPoint>> Handle(GetRevenueTrendQuery request, CancellationToken cancellationToken)
    {
        return await _paymentRepository.GetRevenueTrendAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}

public record GetAuctionTrendQuery(DateTimeOffset StartDate, DateTimeOffset EndDate, string Granularity) : IRequest<List<TrendDataPoint>>;
public class GetAuctionTrendQueryHandler : IRequestHandler<GetAuctionTrendQuery, List<TrendDataPoint>>
{
    private readonly IFactAuctionRepository _auctionRepository;
    public GetAuctionTrendQueryHandler(IFactAuctionRepository auctionRepository) => _auctionRepository = auctionRepository;

    public async Task<List<TrendDataPoint>> Handle(GetAuctionTrendQuery request, CancellationToken cancellationToken)
    {
        return await _auctionRepository.GetAuctionTrendAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}

public record GetCategoryPerformanceQuery(DateTimeOffset? StartDate, DateTimeOffset? EndDate) : IRequest<List<CategoryBreakdown>>;
public class GetCategoryPerformanceQueryHandler : IRequestHandler<GetCategoryPerformanceQuery, List<CategoryBreakdown>>
{
    private readonly IFactAuctionRepository _auctionRepository;
    public GetCategoryPerformanceQueryHandler(IFactAuctionRepository auctionRepository) => _auctionRepository = auctionRepository;

    public async Task<List<CategoryBreakdown>> Handle(GetCategoryPerformanceQuery request, CancellationToken cancellationToken)
    {
        return await _auctionRepository.GetCategoryPerformanceAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}

public record GetAuctionMetricsQuery(AnalyticsQueryParams Query) : IRequest<AuctionMetrics>;
public class GetAuctionMetricsQueryHandler : IRequestHandler<GetAuctionMetricsQuery, AuctionMetrics>
{
    private readonly IFactAuctionRepository _auctionRepository;
    public GetAuctionMetricsQueryHandler(IFactAuctionRepository auctionRepository) => _auctionRepository = auctionRepository;

    public async Task<AuctionMetrics> Handle(GetAuctionMetricsQuery request, CancellationToken cancellationToken)
    {
        return await _auctionRepository.GetAuctionMetricsAsync(request.Query.StartDate, request.Query.EndDate, cancellationToken);
    }
}

public record GetBidMetricsQuery(AnalyticsQueryParams Query) : IRequest<BidMetrics>;
public class GetBidMetricsQueryHandler : IRequestHandler<GetBidMetricsQuery, BidMetrics>
{
    private readonly IFactBidRepository _bidRepository;
    public GetBidMetricsQueryHandler(IFactBidRepository bidRepository) => _bidRepository = bidRepository;

    public async Task<BidMetrics> Handle(GetBidMetricsQuery request, CancellationToken cancellationToken)
    {
        return await _bidRepository.GetBidMetricsAsync(request.Query.StartDate, request.Query.EndDate, cancellationToken);
    }
}

public record GetRevenueMetricsQuery(AnalyticsQueryParams Query) : IRequest<RevenueMetrics>;
public class GetRevenueMetricsQueryHandler : IRequestHandler<GetRevenueMetricsQuery, RevenueMetrics>
{
    private readonly IFactPaymentRepository _paymentRepository;
    public GetRevenueMetricsQueryHandler(IFactPaymentRepository paymentRepository) => _paymentRepository = paymentRepository;

    public async Task<RevenueMetrics> Handle(GetRevenueMetricsQuery request, CancellationToken cancellationToken)
    {
        return await _paymentRepository.GetRevenueMetricsAsync(request.Query.StartDate, request.Query.EndDate, cancellationToken);
    }
}

public record GetAggregatedDailyStatsQuery(DateOnly? StartDate, DateOnly? EndDate) : IRequest<AggregatedDailyStatsDto>;
public class GetAggregatedDailyStatsQueryHandler : IRequestHandler<GetAggregatedDailyStatsQuery, AggregatedDailyStatsDto>
{
    private readonly IDailyStatsRepository _dailyStatsRepository;
    public GetAggregatedDailyStatsQueryHandler(IDailyStatsRepository dailyStatsRepository) => _dailyStatsRepository = dailyStatsRepository;

    public async Task<AggregatedDailyStatsDto> Handle(GetAggregatedDailyStatsQuery request, CancellationToken cancellationToken)
    {
        return await _dailyStatsRepository.GetAggregatedStatsAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}
