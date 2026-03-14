using Analytics.Api.Data;
using Analytics.Api.Interfaces;
using Analytics.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Api.Repositories;

public class DailyStatsRepository : IDailyStatsRepository
{
    private readonly AnalyticsDbContext _context;

    public DailyStatsRepository(AnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<List<DailyAuctionStatsDto>> GetDailyAuctionStatsAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DailyAuctionStats.AsNoTracking();

        if (startDate.HasValue)
            query = query.Where(s => s.DateKey >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(s => s.DateKey <= endDate.Value);

        var stats = await query
            .OrderByDescending(s => s.DateKey)
            .ThenBy(s => s.EventType)
            .ToListAsync(cancellationToken);

        return stats.Select(s => new DailyAuctionStatsDto
        {
            DateKey = s.DateKey,
            EventType = s.EventType,
            EventCount = s.EventCount,
            UniqueSellers = s.UniqueSellers,
            UniqueWinners = s.UniqueWinners,
            TotalRevenue = s.TotalRevenue,
            AvgSalePrice = s.AvgSalePrice,
            MinSalePrice = s.MinSalePrice,
            MaxSalePrice = s.MaxSalePrice,
            SoldCount = s.SoldCount,
            UnsoldCount = s.UnsoldCount
        }).ToList();
    }

    public async Task<List<DailyBidStatsDto>> GetDailyBidStatsAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DailyBidStats.AsNoTracking();

        if (startDate.HasValue)
            query = query.Where(s => s.DateKey >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(s => s.DateKey <= endDate.Value);

        var stats = await query
            .OrderByDescending(s => s.DateKey)
            .ToListAsync(cancellationToken);

        return stats.Select(s => new DailyBidStatsDto
        {
            DateKey = s.DateKey,
            TotalBids = s.TotalBids,
            UniqueBidders = s.UniqueBidders,
            AuctionsWithBids = s.AuctionsWithBids,
            TotalBidValue = s.TotalBidValue,
            AvgBidAmount = s.AvgBidAmount,
            MinBidAmount = s.MinBidAmount,
            MaxBidAmount = s.MaxBidAmount
        }).ToList();
    }

    public async Task<List<DailyRevenueStatsDto>> GetDailyRevenueStatsAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DailyRevenueStats.AsNoTracking();

        if (startDate.HasValue)
            query = query.Where(s => s.DateKey >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(s => s.DateKey <= endDate.Value);

        var stats = await query
            .OrderByDescending(s => s.DateKey)
            .ThenBy(s => s.EventType)
            .ToListAsync(cancellationToken);

        return stats.Select(s => new DailyRevenueStatsDto
        {
            DateKey = s.DateKey,
            EventType = s.EventType,
            TransactionCount = s.TransactionCount,
            UniqueBuyers = s.UniqueBuyers,
            UniqueSellers = s.UniqueSellers,
            TotalRevenue = s.TotalRevenue,
            AvgTransactionAmount = s.AvgTransactionAmount,
            RefundedAmount = s.RefundedAmount,
            RefundCount = s.RefundCount
        }).ToList();
    }

    public async Task<AggregatedDailyStatsDto> GetAggregatedStatsAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default)
    {
        var auctionStatsTask = GetDailyAuctionStatsAsync(startDate, endDate, cancellationToken);
        var bidStatsTask = GetDailyBidStatsAsync(startDate, endDate, cancellationToken);
        var revenueStatsTask = GetDailyRevenueStatsAsync(startDate, endDate, cancellationToken);

        await Task.WhenAll(auctionStatsTask, bidStatsTask, revenueStatsTask);

        var auctionStats = await auctionStatsTask;
        var bidStats = await bidStatsTask;
        var revenueStats = await revenueStatsTask;

        return new AggregatedDailyStatsDto
        {
            StartDate = startDate,
            EndDate = endDate,
            AuctionStats = auctionStats,
            BidStats = bidStats,
            RevenueStats = revenueStats,
            Summary = new DailyStatsSummaryDto
            {
                TotalAuctionEvents = auctionStats.Sum(s => s.EventCount),
                TotalAuctionRevenue = auctionStats.Sum(s => s.TotalRevenue ?? 0),
                TotalBids = bidStats.Sum(s => s.TotalBids),
                TotalBidValue = bidStats.Sum(s => s.TotalBidValue),
                TotalTransactions = revenueStats.Sum(s => s.TransactionCount),
                TotalRevenue = revenueStats.Sum(s => s.TotalRevenue),
                TotalRefunds = revenueStats.Sum(s => s.RefundedAmount ?? 0)
            }
        };
    }
}
