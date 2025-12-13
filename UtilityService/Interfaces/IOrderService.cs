using UtilityService.DTOs;

namespace UtilityService.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string buyerUsername, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderByAuctionIdAsync(Guid auctionId, CancellationToken cancellationToken = default);
    Task<List<OrderDto>> GetBuyerOrdersAsync(string username, CancellationToken cancellationToken = default);
    Task<List<OrderDto>> GetSellerOrdersAsync(string username, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDto dto, string username, CancellationToken cancellationToken = default);
    Task<OrderSummaryDto> GetOrderSummaryAsync(string username, CancellationToken cancellationToken = default);
    Task MarkAsPaidAsync(Guid id, string transactionId, CancellationToken cancellationToken = default);
    Task MarkAsShippedAsync(Guid id, string trackingNumber, string carrier, CancellationToken cancellationToken = default);
    Task MarkAsDeliveredAsync(Guid id, CancellationToken cancellationToken = default);
}
