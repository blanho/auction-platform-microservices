using Auctions.Api.Grpc;
using Auctions.Domain.Entities;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Auctions.Api.Grpc;

public partial class AuctionGrpcService
{
    public override async Task<AuctionStatsResponse> GetAuctionStats(
        GetAuctionStatsRequest request,
        ServerCallContext context)
    {
        var liveAuctions = await _auctionRepository.CountLiveAuctionsAsync(context.CancellationToken);
        var completedAuctions = await _auctionRepository.GetCountByStatusAsync(BuildingBlocks.Domain.Enums.Status.Finished, context.CancellationToken);
        var totalAuctions = await _auctionRepository.GetTotalCountAsync(context.CancellationToken);
        var totalRevenue = await _auctionRepository.GetTotalRevenueAsync(context.CancellationToken);

        return new AuctionStatsResponse
        {
            LiveAuctions = liveAuctions,
            TotalAuctions = totalAuctions,
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

        var liveAuctions = await _auctionRepository.CountLiveAuctionsAsync(context.CancellationToken);
        var completedAuctions = await _auctionRepository.GetCountByStatusAsync(BuildingBlocks.Domain.Enums.Status.Finished, context.CancellationToken);
        var pendingAuctions = await _auctionRepository.GetCountByStatusAsync(BuildingBlocks.Domain.Enums.Status.Draft, context.CancellationToken);
        pendingAuctions += await _auctionRepository.GetCountByStatusAsync(BuildingBlocks.Domain.Enums.Status.Scheduled, context.CancellationToken);
        var totalRevenue = await _auctionRepository.GetTotalRevenueAsync(context.CancellationToken);
        var totalAuctions = await _auctionRepository.GetTotalCountAsync(context.CancellationToken);
        
        var avgFinalPrice = totalAuctions > 0 ? totalRevenue / totalAuctions : 0m;
        var successRate = totalAuctions > 0 ? completedAuctions * 100.0 / totalAuctions : 0.0;

        var today = DateTime.UtcNow.Date;
        var endOfDay = today.AddDays(1);
        var endOfWeek = today.AddDays(7);
        var endingToday = await _auctionRepository.GetCountEndingBetweenAsync(today, endOfDay, context.CancellationToken);
        var endingThisWeek = await _auctionRepository.GetCountEndingBetweenAsync(today, endOfWeek, context.CancellationToken);

        var response = new AuctionAnalyticsResponse
        {
            LiveAuctions = liveAuctions,
            CompletedAuctions = completedAuctions,
            CancelledAuctions = 0,
            PendingAuctions = pendingAuctions,
            TotalRevenue = decimal.ToDouble(totalRevenue),
            AverageFinalPrice = decimal.ToDouble(avgFinalPrice),
            SuccessRate = successRate,
            AuctionsEndingToday = endingToday,
            AuctionsEndingThisWeek = endingThisWeek
        };

        return response;
    }

    public override async Task<TopAuctionsResponse> GetTopAuctions(
        GetTopAuctionsRequest request,
        ServerCallContext context)
    {
        var limit = request.Limit > 0 ? request.Limit : 10;
        var topAuctions = await _auctionRepository.GetTrendingItemsAsync(limit, context.CancellationToken);

        var response = new TopAuctionsResponse();
        foreach (var auction in topAuctions)
        {
            response.Auctions.Add(new TopAuction
            {
                Id = auction.Id.ToString(),
                Title = auction.Item?.Title ?? string.Empty,
                Seller = auction.SellerUsername,
                CurrentBid = (int)decimal.ToDouble(auction.CurrentHighBid ?? 0m),
                BidCount = 0,
                EndTime = auction.AuctionEnd.ToString("O"),
                Status = auction.Status.ToString()
            });
        }

        return response;
    }

    public override async Task<CategoryStatsResponse> GetCategoryStats(
        GetCategoryStatsRequest request,
        ServerCallContext context)
    {
        var categoryStats = await _auctionRepository.GetCategoryStatsAsync(context.CancellationToken);

        var response = new CategoryStatsResponse();
        foreach (var stat in categoryStats)
        {
            response.Categories.Add(new CategoryStat
            {
                CategoryName = stat.CategoryName,
                ActiveAuctions = stat.AuctionCount,
                TotalAuctions = stat.AuctionCount,
                TotalValue = decimal.ToDouble(stat.Revenue)
            });
        }

        return response;
    }
}
