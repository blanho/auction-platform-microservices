using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Domain.Constants;

namespace Analytics.Application.Features.PlatformAnalytics.GetPlatformAnalytics;

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
