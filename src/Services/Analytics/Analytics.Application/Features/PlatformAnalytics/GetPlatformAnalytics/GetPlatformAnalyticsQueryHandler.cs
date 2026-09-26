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

        var auctionMetrics = await _auctionRepository.GetAuctionMetricsAsync(startDate, endDate, cancellationToken);
        var bidMetrics = await _bidRepository.GetBidMetricsAsync(startDate, endDate, cancellationToken);
        var revenueMetrics = await _paymentRepository.GetRevenueMetricsAsync(startDate, endDate, cancellationToken);
        var categoryPerformance = await _auctionRepository.GetCategoryPerformanceAsync(startDate, endDate, cancellationToken);

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
