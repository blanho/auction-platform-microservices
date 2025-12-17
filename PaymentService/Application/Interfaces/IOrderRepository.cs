using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

public record RevenueStatsDto(
    decimal TotalRevenue,
    decimal TotalPlatformFees,
    int TotalTransactions,
    int CompletedOrders,
    int PendingOrders,
    int RefundedOrders,
    decimal AverageOrderValue,
    decimal RevenueToday,
    decimal RevenueThisWeek,
    decimal RevenueThisMonth
);

public record DailyRevenueStatDto(DateOnly Date, decimal Revenue, decimal PlatformFees, int OrderCount);

public record TopSellerDto(Guid SellerId, string Username, decimal TotalSales, int OrderCount, decimal AverageOrderValue);

public record TopBuyerDto(Guid BuyerId, string Username, decimal TotalSpent, int OrderCount);

public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid id);
    Task<Order> GetByAuctionIdAsync(Guid auctionId);
    Task<IEnumerable<Order>> GetByBuyerUsernameAsync(string username, int page = 1, int pageSize = 10);
    Task<IEnumerable<Order>> GetBySellerUsernameAsync(string username, int page = 1, int pageSize = 10);
    Task<Order> AddAsync(Order order);
    Task<Order> UpdateAsync(Order order);
    Task<int> GetCountByBuyerUsernameAsync(string username);
    Task<int> GetCountBySellerUsernameAsync(string username);
    
    Task<RevenueStatsDto> GetRevenueStatsAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken cancellationToken = default);
    Task<List<DailyRevenueStatDto>> GetDailyRevenueAsync(int days, CancellationToken cancellationToken = default);
    Task<List<TopSellerDto>> GetTopSellersAsync(int limit, string period, CancellationToken cancellationToken = default);
    Task<List<TopBuyerDto>> GetTopBuyersAsync(int limit, string period, CancellationToken cancellationToken = default);
}
