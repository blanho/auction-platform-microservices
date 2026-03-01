
using System.Linq.Expressions;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Filtering;
using BuildingBlocks.Application.Paging;
using Microsoft.EntityFrameworkCore;
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

    public async Task<Order> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> GetByAuctionIdAsync(Guid auctionId)
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

    public async Task<Order> UpdateAsync(Order order)
    {
        order.SetUpdatedAudit(Guid.Empty, DateTimeOffset.UtcNow);
        _context.Orders.Update(order);
        return order;
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
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTimeOffset(today.Year, today.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var query = _context.Orders.AsNoTracking().AsQueryable();
        
        if (startDate.HasValue)
            query = query.Where(o => o.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(o => o.CreatedAt <= endDate.Value);

        var allOrders = await query.ToListAsync(cancellationToken);
        
        var completedOrders = allOrders.Where(o => o.PaymentStatus == PaymentStatus.Completed).ToList();
        var pendingOrders = allOrders.Where(o => o.PaymentStatus == PaymentStatus.Pending).ToList();
        var refundedOrders = allOrders.Where(o => o.PaymentStatus == PaymentStatus.Refunded).ToList();

        var totalRevenue = completedOrders.Sum(o => o.TotalAmount);
        var totalPlatformFees = completedOrders.Sum(o => o.PlatformFee ?? 0);
        var averageOrderValue = completedOrders.Count > 0 ? totalRevenue / completedOrders.Count : 0;

        var todayOrders = completedOrders.Where(o => o.PaidAt?.Date == today).ToList();
        var weekOrders = completedOrders.Where(o => o.PaidAt >= weekStart).ToList();
        var monthOrders = completedOrders.Where(o => o.PaidAt >= monthStart).ToList();

        return new RevenueStatsDto(
            totalRevenue,
            totalPlatformFees,
            allOrders.Count,
            completedOrders.Count,
            pendingOrders.Count,
            refundedOrders.Count,
            averageOrderValue,
            todayOrders.Sum(o => o.TotalAmount),
            weekOrders.Sum(o => o.TotalAmount),
            monthOrders.Sum(o => o.TotalAmount)
        );
    }

    public async Task<List<DailyRevenueStatDto>> GetDailyRevenueAsync(int days, CancellationToken cancellationToken = default)
    {
        var startDate = DateTimeOffset.UtcNow.AddDays(-days);

        var dailyStats = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.PaidAt >= startDate)
            .GroupBy(o => o.PaidAt.Value.Date)
            .Select(g => new DailyRevenueStatDto(
                DateOnly.FromDateTime(g.Key),
                g.Sum(o => o.TotalAmount),
                g.Sum(o => o.PlatformFee ?? 0),
                g.Count()
            ))
            .OrderBy(s => s.Date)
            .ToListAsync(cancellationToken);

        return dailyStats;
    }

    public async Task<List<TopSellerDto>> GetTopSellersAsync(int limit, string period, CancellationToken cancellationToken = default)
    {
        var startDate = DateTimeHelper.GetPeriodStartDate(period);

        var topSellers = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.PaidAt >= startDate)
            .GroupBy(o => new { o.SellerId, o.SellerUsername })
            .Select(g => new TopSellerDto(
                g.Key.SellerId,
                g.Key.SellerUsername,
                g.Sum(o => o.TotalAmount),
                g.Count(),
                g.Sum(o => o.TotalAmount) / g.Count()
            ))
            .OrderByDescending(s => s.TotalSales)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return topSellers;
    }

    public async Task<List<TopBuyerDto>> GetTopBuyersAsync(int limit, string period, CancellationToken cancellationToken = default)
    {
        var startDate = DateTimeHelper.GetPeriodStartDate(period);

        var topBuyers = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.PaidAt >= startDate)
            .GroupBy(o => new { o.BuyerId, o.BuyerUsername })
            .Select(g => new TopBuyerDto(
                g.Key.BuyerId,
                g.Key.BuyerUsername,
                g.Sum(o => o.TotalAmount),
                g.Count()
            ))
            .OrderByDescending(b => b.TotalSpent)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return topBuyers;
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

    public async Task<OrderStatsDto> GetOrderStatsAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalRevenue = orders.Where(o => o.PaymentStatus == PaymentStatus.Completed).Sum(o => o.TotalAmount);
        var completedOrdersCount = orders.Count(o => o.PaymentStatus == PaymentStatus.Completed);
        var averageOrderValue = completedOrdersCount > 0 ? totalRevenue / completedOrdersCount : 0;

        return new OrderStatsDto(
            orders.Count,
            orders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.PaymentPending),
            orders.Count(o => o.Status == OrderStatus.Paid),
            orders.Count(o => o.Status == OrderStatus.Processing),
            orders.Count(o => o.Status == OrderStatus.Shipped),
            orders.Count(o => o.Status == OrderStatus.Delivered),
            orders.Count(o => o.Status == OrderStatus.Completed),
            orders.Count(o => o.Status == OrderStatus.Cancelled),
            orders.Count(o => o.Status == OrderStatus.Disputed),
            orders.Count(o => o.Status == OrderStatus.Refunded),
            totalRevenue,
            averageOrderValue
        );
    }
}
