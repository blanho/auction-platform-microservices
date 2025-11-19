using Auctions.Api.Grpc;
using Auctions.Domain.Entities;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Auctions.Api.Services.Grpc;

public partial class AuctionGrpcService
{
    public override async Task<AuctionStatsResponse> GetAuctionStats(
        GetAuctionStatsRequest request,
        ServerCallContext context)
    {
        var allAuctions = await _auctionRepository.GetAllAsync(context.CancellationToken);

        var liveAuctions = allAuctions.Count(a => a.Status == BuildingBlocks.Domain.Enums.Status.Live);
        var completedAuctions = allAuctions.Count(a => a.Status == BuildingBlocks.Domain.Enums.Status.Finished);
        var totalRevenue = allAuctions
            .Where(a => a.SoldAmount.HasValue)
            .Sum(a => a.SoldAmount!.Value);

        return new AuctionStatsResponse
        {
            LiveAuctions = liveAuctions,
            TotalAuctions = allAuctions.Count,
            CompletedAuctions = completedAuctions,
            TotalRevenue = decimal.ToDouble(totalRevenue)
        };
    }

    public override async Task<AuctionAnalyticsResponse> GetAuctionAnalytics(
        GetAuctionAnalyticsRequest request,
        ServerCallContext context)
    {
        var startDate = DateTime.TryParse(request.StartDate, out var start) ? start : DateTime.UtcNow.AddMonths(-1);
        var endDate = DateTime.TryParse(request.EndDate, out var end) ? end : DateTime.UtcNow;

        var auctions = await _auctionRepository.GetAllAsync(context.CancellationToken);
        var filteredAuctions = auctions
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
            .ToList();

        var liveAuctions = filteredAuctions.Count(a => a.Status == BuildingBlocks.Domain.Enums.Status.Live);
        var completedAuctions = filteredAuctions.Count(a => a.Status == BuildingBlocks.Domain.Enums.Status.Finished);
        var cancelledAuctions = 0;
        var pendingAuctions = filteredAuctions.Count(a => a.Status == BuildingBlocks.Domain.Enums.Status.Draft || a.Status == BuildingBlocks.Domain.Enums.Status.Scheduled);

        var soldAuctions = filteredAuctions.Where(a => a.SoldAmount.HasValue).ToList();
        var totalRevenue = soldAuctions.Sum(a => a.SoldAmount!.Value);
        var avgFinalPrice = soldAuctions.Any() ? soldAuctions.Average(a => a.SoldAmount!.Value) : 0m;
        var successRate = filteredAuctions.Any() ? soldAuctions.Count * 100.0 / filteredAuctions.Count : 0.0;

        var today = DateTime.UtcNow.Date;
        var endOfWeek = today.AddDays(7);
        var endingToday = auctions.Count(a => a.AuctionEnd.Date == today && a.Status == BuildingBlocks.Domain.Enums.Status.Live);
        var endingThisWeek = auctions.Count(a => a.AuctionEnd.Date <= endOfWeek && a.Status == BuildingBlocks.Domain.Enums.Status.Live);

        var response = new AuctionAnalyticsResponse
        {
            LiveAuctions = liveAuctions,
            CompletedAuctions = completedAuctions,
            CancelledAuctions = cancelledAuctions,
            PendingAuctions = pendingAuctions,
            TotalRevenue = decimal.ToDouble(totalRevenue),
            AverageFinalPrice = decimal.ToDouble(avgFinalPrice),
            SuccessRate = successRate,
            AuctionsEndingToday = endingToday,
            AuctionsEndingThisWeek = endingThisWeek
        };

        var dailyStats = filteredAuctions
            .GroupBy(a => a.CreatedAt.Date)
            .Select(g => new DailyAuctionStat
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Created = g.Count(),
                Completed = g.Count(a => a.Status == BuildingBlocks.Domain.Enums.Status.Finished),
                Revenue = decimal.ToDouble(g.Where(a => a.SoldAmount.HasValue).Sum(a => a.SoldAmount!.Value))
            })
            .OrderBy(d => d.Date);

        response.DailyStats.AddRange(dailyStats);

        return response;
    }

    public override async Task<TopAuctionsResponse> GetTopAuctions(
        GetTopAuctionsRequest request,
        ServerCallContext context)
    {
        var limit = request.Limit > 0 ? request.Limit : 10;
        var auctions = await _auctionRepository.GetAllAsync(context.CancellationToken);

        var query = auctions.Where(a => a.Status == BuildingBlocks.Domain.Enums.Status.Live);

        var topAuctions = query
            .OrderByDescending(a => a.CurrentHighBid ?? 0)
            .Take(limit)
            .ToList();

        var response = new TopAuctionsResponse();
        foreach (var auction in topAuctions)
        {
            response.Auctions.Add(new TopAuction
            {
                AuctionId = auction.Id.ToString(),
                Title = auction.Item?.Title ?? string.Empty,
                SellerUsername = auction.SellerUsername,
                FinalPrice = decimal.ToDouble(auction.CurrentHighBid ?? 0m),
                BidCount = 0
            });
        }

        return response;
    }

    public override async Task<CategoryStatsResponse> GetCategoryStats(
        GetCategoryStatsRequest request,
        ServerCallContext context)
    {
        var auctions = await _auctionRepository.GetAllAsync(context.CancellationToken);

        var categoryStats = auctions
            .Where(a => a.Item?.Category != null)
            .GroupBy(a => a.Item!.Category)
            .Select(g => new CategoryStat
            {
                CategoryId = g.Key.Id.ToString(),
                CategoryName = g.Key.Name,
                AuctionCount = g.Count(),
                Revenue = decimal.ToDouble(g.Where(a => a.SoldAmount.HasValue).Sum(a => a.SoldAmount!.Value)),
                Percentage = 0
            })
            .OrderByDescending(c => c.AuctionCount)
            .ToList();

        var response = new CategoryStatsResponse();
        response.Categories.AddRange(categoryStats);

        return response;
    }
}
