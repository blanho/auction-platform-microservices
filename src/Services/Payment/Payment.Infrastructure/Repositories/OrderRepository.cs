
using System.Linq.Expressions;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Filtering;
using BuildingBlocks.Application.Paging;
using Payment.Application.DTOs;
using Payment.Application.Filtering;
using Payment.Application.Helpers;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly PaymentDbContext _context;

    private static readonly Dictionary<string, Expression<Func<Order, object>>> OrderSortMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdat"] = o => o.CreatedAt,
            ["totalamount"] = o => o.TotalAmount,
            ["status"] = o => o.Status,
            ["paidat"] = o => o.PaidAt!,
            ["itemtitle"] = o => o.ItemTitle
        };

    public OrderRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByAuctionIdAsync(Guid auctionId)
    {
        return await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.AuctionId == auctionId);
    }

    public async Task<PaginatedResult<Order>> GetByBuyerUsernameAsync(OrderQueryParams queryParams)
    {
        return await GetOrdersByQueryParamsAsync(queryParams);
    }

    public async Task<PaginatedResult<Order>> GetBySellerUsernameAsync(OrderQueryParams queryParams)
    {
        return await GetOrdersByQueryParamsAsync(queryParams);
    }

    private async Task<PaginatedResult<Order>> GetOrdersByQueryParamsAsync(OrderQueryParams queryParams)
    {
        var query = _context.Orders.AsNoTracking();

        if (queryParams.Filter != null)
        {
            query = queryParams.Filter.Apply(query);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .ApplySorting(queryParams, OrderSortMap, o => o.CreatedAt)
            .ApplyPaging(queryParams)
            .ToListAsync();

        return new PaginatedResult<Order>(items, totalCount, queryParams.Page, queryParams.PageSize);
    }

    public async Task<Order> AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        return order;
    }

    public Task<Order> UpdateAsync(Order order)
    {
        order.SetUpdatedAudit(Guid.Empty, DateTimeOffset.UtcNow);
        _context.Orders.Update(order);
        return Task.FromResult(order);
    }

    public Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken)
    {
        order.SetUpdatedAudit(Guid.Empty, DateTimeOffset.UtcNow);
        _context.Orders.Update(order);
        return Task.FromResult(order);
    }

    public async Task<int> GetCountByBuyerUsernameAsync(string username)
    {
        return await _context.Orders
            .AsNoTracking()
            .CountAsync(o => o.BuyerUsername == username);
    }

    public async Task<int> GetCountBySellerUsernameAsync(string username)
    {
        return await _context.Orders
            .AsNoTracking()
            .CountAsync(o => o.SellerUsername == username);
    }

    public async Task<RevenueStatsDto> GetRevenueStatsAsync(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var today = now.Date;
        var todayStart = new DateTimeOffset(today, TimeSpan.Zero);
        var tomorrowStart = todayStart.AddDays(1);
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTimeOffset(today.Year, today.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var query = _context.Orders.AsNoTracking().AsQueryable();

        if (startDate.HasValue)
            query = query.Where(o => o.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(o => o.CreatedAt <= endDate.Value);

        var totals = await query
            .GroupBy(_ => 1)
            .Select(orders => new
            {
                TotalTransactions = orders.Count(),
                CompletedOrders = orders.Count(o => o.PaymentStatus == PaymentStatus.Completed),
                PendingOrders = orders.Count(o => o.PaymentStatus == PaymentStatus.Pending),
                RefundedOrders = orders.Count(o => o.PaymentStatus == PaymentStatus.Refunded),
                TotalRevenue = orders.Sum(o => o.PaymentStatus == PaymentStatus.Completed ? o.TotalAmount : 0),
                TotalPlatformFees = orders.Sum(o => o.PaymentStatus == PaymentStatus.Completed ? o.PlatformFee ?? 0 : 0),
                RevenueToday = orders.Sum(o => o.PaymentStatus == PaymentStatus.Completed &&
                    o.PaidAt >= todayStart && o.PaidAt < tomorrowStart ? o.TotalAmount : 0),
                RevenueThisWeek = orders.Sum(o => o.PaymentStatus == PaymentStatus.Completed &&
                    o.PaidAt >= weekStart ? o.TotalAmount : 0),
                RevenueThisMonth = orders.Sum(o => o.PaymentStatus == PaymentStatus.Completed &&
                    o.PaidAt >= monthStart ? o.TotalAmount : 0)
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (totals is null)
            return new RevenueStatsDto(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        return new RevenueStatsDto(
            totals.TotalRevenue,
            totals.TotalPlatformFees,
            totals.TotalTransactions,
            totals.CompletedOrders,
            totals.PendingOrders,
            totals.RefundedOrders,
            totals.CompletedOrders > 0 ? totals.TotalRevenue / totals.CompletedOrders : 0,
            totals.RevenueToday,
            totals.RevenueThisWeek,
            totals.RevenueThisMonth
        );
    }

    public async Task<List<DailyRevenueStatDto>> GetDailyRevenueAsync(int days, CancellationToken cancellationToken = default)
    {
        var startDate = DateTimeOffset.UtcNow.AddDays(-days);

        var dailyTotals = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.PaidAt >= startDate)
            .GroupBy(o => o.PaidAt!.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.TotalAmount),
                PlatformFees = g.Sum(o => o.PlatformFee ?? 0),
                OrderCount = g.Count()
            })
            .OrderBy(stat => stat.Date)
            .ToListAsync(cancellationToken);

        return dailyTotals
            .Select(stat => new DailyRevenueStatDto(
                DateOnly.FromDateTime(stat.Date),
                stat.Revenue,
                stat.PlatformFees,
                stat.OrderCount))
            .ToList();
    }

    public async Task<List<TopSellerDto>> GetTopSellersAsync(int limit, string period, CancellationToken cancellationToken = default)
    {
        var startDate = DateTimeHelper.GetPeriodStartDate(period);

        var topSellers = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.PaidAt >= startDate)
            .GroupBy(o => new { o.SellerId, o.SellerUsername })
            .Select(g => new
            {
                g.Key.SellerId,
                g.Key.SellerUsername,
                TotalSales = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .OrderByDescending(seller => seller.TotalSales)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return topSellers
            .Select(seller => new TopSellerDto(
                seller.SellerId,
                seller.SellerUsername,
                seller.TotalSales,
                seller.OrderCount,
                seller.TotalSales / seller.OrderCount))
            .ToList();
    }

    public async Task<List<TopBuyerDto>> GetTopBuyersAsync(int limit, string period, CancellationToken cancellationToken = default)
    {
        var startDate = DateTimeHelper.GetPeriodStartDate(period);

        var topBuyers = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.PaidAt >= startDate)
            .GroupBy(o => new { o.BuyerId, o.BuyerUsername })
            .Select(g => new
            {
                g.Key.BuyerId,
                g.Key.BuyerUsername,
                TotalSpent = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .OrderByDescending(buyer => buyer.TotalSpent)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return topBuyers
            .Select(buyer => new TopBuyerDto(
                buyer.BuyerId,
                buyer.BuyerUsername,
                buyer.TotalSpent,
                buyer.OrderCount))
            .ToList();
    }

    public async Task<PaginatedResult<Order>> GetAllAsync(
        OrderQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.AsNoTracking();

        if (queryParams.Filter != null)
        {
            query = queryParams.Filter.Apply(query);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplySorting(queryParams, OrderSortMap, o => o.CreatedAt)
            .ApplyPaging(queryParams)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Order>(items, totalCount, queryParams.Page, queryParams.PageSize);
    }

    public Task<List<Order>> GetForReportAsync(
        OrderReportParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.AsNoTracking();

        if (parameters.StatusFilter.HasValue)
            query = query.Where(order => order.Status == parameters.StatusFilter.Value);
        if (parameters.StartDate.HasValue)
            query = query.Where(order => order.CreatedAt >= new DateTimeOffset(parameters.StartDate.Value.DateTime, TimeSpan.Zero));
        if (parameters.EndDate.HasValue)
            query = query.Where(order => order.CreatedAt <= new DateTimeOffset(parameters.EndDate.Value.DateTime, TimeSpan.Zero));
        if (parameters.BuyerIdFilter.HasValue)
            query = query.Where(order => order.BuyerId == parameters.BuyerIdFilter.Value);
        if (parameters.SellerIdFilter.HasValue)
            query = query.Where(order => order.SellerId == parameters.SellerIdFilter.Value);

        return query.OrderByDescending(order => order.CreatedAt)
            .ThenBy(order => order.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderStatsDto> GetOrderStatsAsync(CancellationToken cancellationToken = default)
    {
        var totals = await _context.Orders
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(orders => new
            {
                TotalOrders = orders.Count(),
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.PaymentPending),
                PaidOrders = orders.Count(o => o.Status == OrderStatus.Paid),
                ProcessingOrders = orders.Count(o => o.Status == OrderStatus.Processing),
                ShippedOrders = orders.Count(o => o.Status == OrderStatus.Shipped),
                DeliveredOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
                CompletedOrders = orders.Count(o => o.Status == OrderStatus.Completed),
                CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
                DisputedOrders = orders.Count(o => o.Status == OrderStatus.Disputed),
                RefundedOrders = orders.Count(o => o.Status == OrderStatus.Refunded),
                PaidOrderCount = orders.Count(o => o.PaymentStatus == PaymentStatus.Completed),
                TotalRevenue = orders.Sum(o => o.PaymentStatus == PaymentStatus.Completed ? o.TotalAmount : 0)
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (totals is null)
            return new OrderStatsDto(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        return new OrderStatsDto(
            totals.TotalOrders,
            totals.PendingOrders,
            totals.PaidOrders,
            totals.ProcessingOrders,
            totals.ShippedOrders,
            totals.DeliveredOrders,
            totals.CompletedOrders,
            totals.CancelledOrders,
            totals.DisputedOrders,
            totals.RefundedOrders,
            totals.TotalRevenue,
            totals.PaidOrderCount > 0 ? totals.TotalRevenue / totals.PaidOrderCount : 0
        );
    }
}
