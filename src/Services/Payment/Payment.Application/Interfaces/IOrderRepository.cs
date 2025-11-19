using Payment.Application.DTOs;
using Payment.Domain.Entities;

namespace Payment.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid id);
    Task<Order> GetByAuctionIdAsync(Guid auctionId);
    Task<IEnumerable<Order>> GetByBuyerUsernameAsync(string username, int page = PaginationDefaults.DefaultPage, int pageSize = PaginationDefaults.DefaultPageSize);
    Task<IEnumerable<Order>> GetBySellerUsernameAsync(string username, int page = PaginationDefaults.DefaultPage, int pageSize = PaginationDefaults.DefaultPageSize);
    Task<Order> AddAsync(Order order);
    Task<Order> UpdateAsync(Order order);
    Task<int> GetCountByBuyerUsernameAsync(string username);
    Task<int> GetCountBySellerUsernameAsync(string username);
    
    Task<RevenueStatsDto> GetRevenueStatsAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken cancellationToken = default);
    Task<List<DailyRevenueStatDto>> GetDailyRevenueAsync(int days, CancellationToken cancellationToken = default);
    Task<List<TopSellerDto>> GetTopSellersAsync(int limit, string period, CancellationToken cancellationToken = default);
    Task<List<TopBuyerDto>> GetTopBuyersAsync(int limit, string period, CancellationToken cancellationToken = default);
}
