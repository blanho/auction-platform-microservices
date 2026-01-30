using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Paging;
using Payment.Application.DTOs;
using Payment.Domain.Entities;
using Payment.Domain.Enums;

namespace Payment.Application.Interfaces;

public class OrderFilter
{
    public string? SearchTerm { get; init; }
    public OrderStatus? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? BuyerUsername { get; init; }
    public string? SellerUsername { get; init; }
}

public class OrderQueryParams : QueryParameters<OrderFilter> { }

public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid id);
    Task<Order> GetByAuctionIdAsync(Guid auctionId);
    Task<PaginatedResult<Order>> GetByBuyerUsernameAsync(string username, QueryParameters queryParams);
    Task<PaginatedResult<Order>> GetBySellerUsernameAsync(string username, QueryParameters queryParams);
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
