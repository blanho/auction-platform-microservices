using BuildingBlocks.Application.Abstractions;
using Payment.Application.DTOs;
using Payment.Application.Filtering;
using Payment.Domain.Entities;

namespace Payment.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid id);
    Task<Order> GetByAuctionIdAsync(Guid auctionId);
    Task<PaginatedResult<Order>> GetByBuyerUsernameAsync(OrderQueryParams queryParams);
    Task<PaginatedResult<Order>> GetBySellerUsernameAsync(OrderQueryParams queryParams);
    Task<Order> AddAsync(Order order);
    Task<Order> UpdateAsync(Order order);
    Task<int> GetCountByBuyerUsernameAsync(string username);
    Task<int> GetCountBySellerUsernameAsync(string username);
    
    Task<PaginatedResult<Order>> GetAllAsync(OrderQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<OrderStatsDto> GetOrderStatsAsync(CancellationToken cancellationToken = default);
    
    Task<RevenueStatsDto> GetRevenueStatsAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken cancellationToken = default);
    Task<List<DailyRevenueStatDto>> GetDailyRevenueAsync(int days, CancellationToken cancellationToken = default);
    Task<List<TopSellerDto>> GetTopSellersAsync(int limit, string period, CancellationToken cancellationToken = default);
    Task<List<TopBuyerDto>> GetTopBuyersAsync(int limit, string period, CancellationToken cancellationToken = default);
}
