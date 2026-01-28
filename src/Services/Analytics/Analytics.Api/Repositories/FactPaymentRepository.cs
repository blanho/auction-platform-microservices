using Analytics.Api.Data;
using Analytics.Api.Interfaces;
using Analytics.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Api.Repositories;

public class FactPaymentRepository : IFactPaymentRepository
{
    private readonly AnalyticsDbContext _context;

    public FactPaymentRepository(AnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<RevenueMetrics> GetRevenueMetricsAsync(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FactPayments.AsNoTracking();

        if (startDate.HasValue)
            query = query.Where(f => f.EventTime >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(f => f.EventTime <= endDate.Value);

        var payments = await query.ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var todayStart = now.Date;
        var weekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
        var monthStart = new DateTime(todayStart.Year, todayStart.Month, 1);

        var paidPayments = payments.Where(p => p.IsPaid).ToList();
        var totalRevenue = paidPayments.Sum(p => p.TotalAmount);

        return new RevenueMetrics
        {
            TotalRevenue = totalRevenue,
            TotalPlatformFees = totalRevenue * 0.05m,
            TotalTransactions = payments.Count,
            CompletedOrders = paidPayments.Count,
            PendingOrders = payments.Count(p => p.Status == "Pending"),
            RefundedOrders = payments.Count(p => p.IsRefunded),
            AverageOrderValue = paidPayments.Count > 0 ? paidPayments.Average(p => p.TotalAmount) : 0,
            RevenueToday = paidPayments.Where(p => p.EventTime >= todayStart).Sum(p => p.TotalAmount),
            RevenueThisWeek = paidPayments.Where(p => p.EventTime >= weekStart).Sum(p => p.TotalAmount),
            RevenueThisMonth = paidPayments.Where(p => p.EventTime >= monthStart).Sum(p => p.TotalAmount)
        };
    }

    public async Task<List<TrendDataPoint>> GetRevenueTrendAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken = default)
    {
        var data = await _context.FactPayments
            .AsNoTracking()
            .Where(f => f.IsPaid && f.EventTime >= startDate && f.EventTime <= endDate)
            .GroupBy(f => f.DateKey)
            .Select(g => new
            {
                Date = g.Key,
                Revenue = g.Sum(f => f.TotalAmount)
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return data.Select(d => new TrendDataPoint
        {
            Date = new DateTimeOffset(d.Date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
            Value = d.Revenue,
            Label = d.Date.ToString("yyyy-MM-dd")
        }).ToList();
    }

    public async Task<List<TopSellerDto>> GetTopSellersAsync(
        int limit,
        DateTimeOffset? startDate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FactPayments.AsNoTracking().Where(p => p.IsPaid);

        if (startDate.HasValue)
            query = query.Where(p => p.EventTime >= startDate.Value);

        var topSellers = await query
            .Where(p => p.SellerId.HasValue)
            .GroupBy(p => new { p.SellerId, p.SellerUsername })
            .Select(g => new TopSellerDto
            {
                SellerId = g.Key.SellerId ?? Guid.Empty,
                Username = g.Key.SellerUsername,
                TotalSales = g.Sum(p => p.TotalAmount),
                OrderCount = g.Count(),
                AverageOrderValue = g.Average(p => p.TotalAmount)
            })
            .OrderByDescending(s => s.TotalSales)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return topSellers;
    }

    public async Task<List<TopBuyerDto>> GetTopBuyersAsync(
        int limit,
        DateTimeOffset? startDate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FactPayments.AsNoTracking().Where(p => p.IsPaid);

        if (startDate.HasValue)
            query = query.Where(p => p.EventTime >= startDate.Value);

        var topBuyers = await query
            .GroupBy(p => new { p.BuyerId, p.BuyerUsername })
            .Select(g => new TopBuyerDto
            {
                BuyerId = g.Key.BuyerId,
                Username = g.Key.BuyerUsername,
                TotalSpent = g.Sum(p => p.TotalAmount),
                OrderCount = g.Count(),
                AuctionsWon = g.Select(p => p.AuctionId).Distinct().Count()
            })
            .OrderByDescending(b => b.TotalSpent)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return topBuyers;
    }
}
