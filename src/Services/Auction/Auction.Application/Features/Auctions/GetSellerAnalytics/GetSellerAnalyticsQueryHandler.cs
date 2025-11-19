using Auctions.Application.DTOs;
using BuildingBlocks.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Queries.GetSellerAnalytics;

public class GetSellerAnalyticsQueryHandler : IRequestHandler<GetSellerAnalyticsQuery, Result<SellerAnalyticsDto>>
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly ILogger<GetSellerAnalyticsQueryHandler> _logger;

    public GetSellerAnalyticsQueryHandler(
        IAuctionRepository auctionRepository,
        ILogger<GetSellerAnalyticsQueryHandler> logger)
    {
        _auctionRepository = auctionRepository;
        _logger = logger;
    }

    public async Task<Result<SellerAnalyticsDto>> Handle(
        GetSellerAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var daysBack = request.TimeRange switch
            {
                "7d" => 7,
                "30d" => 30,
                "90d" => 90,
                "1y" => 365,
                _ => 30
            };

            var periodStart = DateTimeOffset.UtcNow.AddDays(-daysBack);
            var previousPeriodStart = periodStart.AddDays(-daysBack);

            var stats = await _auctionRepository.GetSellerStatsAsync(
                request.Username, 
                periodStart, 
                previousPeriodStart, 
                cancellationToken);

            var revenueChange = stats.PreviousPeriodRevenue > 0 
                ? ((stats.TotalRevenue - stats.PreviousPeriodRevenue) / stats.PreviousPeriodRevenue) * 100 
                : 0;

            var itemsChange = stats.PreviousPeriodItemsSold > 0 
                ? ((decimal)(stats.ItemsSold - stats.PreviousPeriodItemsSold) / stats.PreviousPeriodItemsSold) * 100 
                : 0;

            var userAuctions = await _auctionRepository.GetBySellerUsernameAsync(request.Username, cancellationToken);

            var topListings = userAuctions
                .Where(a => a.Status == Status.Live)
                .OrderByDescending(a => a.CurrentHighBid)
                .Take(5)
                .Select(a => new TopListingDto
                {
                    Id = a.Id.ToString(),
                    Title = a.Item?.Title ?? "Unknown",
                    CurrentBid = a.CurrentHighBid ?? 0,
                    Views = 0,
                    Bids = 0
                })
                .ToList();

            var salesForChart = userAuctions
                .Where(a => a.Status == Status.Finished && 
                           a.SoldAmount.HasValue && 
                           a.UpdatedAt >= periodStart)
                .ToList();
                
            var chartData = GenerateChartData(salesForChart, daysBack);

            var analytics = new SellerAnalyticsDto
            {
                TotalRevenue = stats.TotalRevenue,
                RevenueChange = decimal.Round(revenueChange, 1),
                ItemsSold = stats.ItemsSold,
                ItemsChange = decimal.Round(itemsChange, 1),
                ActiveListings = stats.ActiveListings,
                ViewsToday = 0,
                ViewsChange = 0,
                TopListings = topListings,
                ChartData = chartData
            };

            return Result<SellerAnalyticsDto>.Success(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting seller analytics for user {Username}", request.Username);
            return Result.Failure<SellerAnalyticsDto>(Error.Create("Analytics.Error", "Failed to retrieve seller analytics"));
        }
    }

    private static List<ChartDataPointDto> GenerateChartData(List<Domain.Entities.Auction> sales, int daysBack)
    {
        var chartData = new List<ChartDataPointDto>();
        var labelCount = daysBack <= 7 ? daysBack : 7;
        var dayGroupSize = daysBack / labelCount;

        for (int i = labelCount - 1; i >= 0; i--)
        {
            var groupEndDate = DateTimeOffset.UtcNow.AddDays(-i * dayGroupSize);
            var groupStartDate = groupEndDate.AddDays(-dayGroupSize);

            var groupSales = sales
                .Where(a => a.UpdatedAt >= groupStartDate && a.UpdatedAt < groupEndDate)
                .ToList();

            var label = daysBack <= 7 
                ? groupEndDate.ToString("ddd") 
                : groupEndDate.ToString("MMM dd");

            chartData.Add(new ChartDataPointDto
            {
                Date = label,
                Revenue = groupSales.Sum(a => a.SoldAmount ?? 0),
                Bids = 0
            });
        }

        return chartData;
    }
}

