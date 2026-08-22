using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Application.Helpers;

namespace Analytics.Application.Features.UserAnalytics.GetSellerAnalytics;

public class GetSellerAnalyticsQueryHandler : IRequestHandler<GetSellerAnalyticsQuery, SellerAnalyticsDto>
{
    private readonly IFactAuctionRepository _auctionRepository;

    public GetSellerAnalyticsQueryHandler(IFactAuctionRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }

    public async Task<SellerAnalyticsDto> Handle(GetSellerAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = AnalyticsHelper.GetDateRange(request.TimeRange);
        var (previousStartDate, previousEndDate) = AnalyticsHelper.GetPreviousPeriod(startDate, endDate);

        var currentStatsTask = _auctionRepository.GetSellerAnalyticsAsync(request.Username, startDate, endDate, cancellationToken);
        var previousStatsTask = _auctionRepository.GetSellerAnalyticsAsync(request.Username, previousStartDate, previousEndDate, cancellationToken);
        var topListingsTask = _auctionRepository.GetTopListingsAsync(request.Username, 5, cancellationToken);

        await Task.WhenAll(currentStatsTask, previousStatsTask, topListingsTask);

        var currentStats = await currentStatsTask;
        var previousStats = await previousStatsTask;
        var topListings = await topListingsTask;

        var revenueChange = AnalyticsHelper.CalculatePercentageChange(previousStats.TotalRevenue, currentStats.TotalRevenue);
        var itemsSoldChange = AnalyticsHelper.CalculatePercentageChange(previousStats.CompletedAuctions, currentStats.CompletedAuctions);
        var avgPriceChange = AnalyticsHelper.CalculatePercentageChange(previousStats.AverageFinalPrice, currentStats.AverageFinalPrice);

        var salesChart = currentStats.DailyRevenue
            .Select(d => new SalesChartDataDto
            {
                Date = d.Date.ToString("yyyy-MM-dd"),
                Amount = d.Revenue,
                Count = d.AuctionsCompleted
            })
            .ToList();

        return new SellerAnalyticsDto
        {
            TotalRevenue = currentStats.TotalRevenue,
            RevenueChange = revenueChange,
            ItemsSold = currentStats.CompletedAuctions,
            ItemsSoldChange = itemsSoldChange,
            AveragePrice = currentStats.AverageFinalPrice,
            AveragePriceChange = avgPriceChange,
            TotalViews = null,
            ViewsChange = null,
            TopListings = topListings,
            SalesChart = salesChart
        };
    }
}
