using Microsoft.EntityFrameworkCore;
using MassTransit;
using Microsoft.Extensions.Logging;
using Analytics.Infrastructure.Persistence;
using Analytics.Application.Interfaces;
using Analytics.Application.DTOs;
using Analytics.Domain.Constants;

namespace Analytics.Infrastructure.Repositories;

public class FactBidRepository : IFactBidRepository
{
    private readonly AnalyticsDbContext _context;

    public FactBidRepository(AnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<BidMetrics> GetBidMetricsAsync(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FactBids.AsNoTracking();

        if (startDate.HasValue)
            query = query.Where(f => f.EventTime >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(f => f.EventTime <= endDate.Value);

        var bids = await query.ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var todayStart = now.Date;
        var weekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
        var monthStart = new DateTime(todayStart.Year, todayStart.Month, 1);

        return new BidMetrics
        {
            TotalBids = bids.Count,
            UniqueBidders = bids.Select(b => b.BidderId).Distinct().Count(),
            AverageBidAmount = bids.Count > 0 ? bids.Average(b => b.BidAmount) : 0,
            BidsToday = bids.Count(b => b.EventTime >= todayStart),
            BidsThisWeek = bids.Count(b => b.EventTime >= weekStart),
            BidsThisMonth = bids.Count(b => b.EventTime >= monthStart)
        };
    }

    public async Task<int> GetBidsInLastHourAsync(CancellationToken cancellationToken = default)
    {
        var hourAgo = DateTimeOffset.UtcNow.AddHours(-1);

        return await _context.FactBids
            .AsNoTracking()
            .Where(f => f.EventTime >= hourAgo)
            .CountAsync(cancellationToken);
    }

    public async Task<UserBidStatsDto> GetUserBidStatsAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var userBids = await _context.FactBids
            .AsNoTracking()
            .Where(f => f.BidderUsername == username)
            .ToListAsync(cancellationToken);

        var auctionsWon = await _context.FactAuctions
            .AsNoTracking()
            .Where(f => f.WinnerUsername == username && f.EventType == AnalyticsEventTypes.Finished && f.Sold)
            .Select(f => f.AuctionId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new UserBidStatsDto
        {
            TotalBids = userBids.Count,
            AuctionsWon = auctionsWon
        };
    }
}
