using PaymentService.Application.DTOs;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderByAuctionIdAsync(Guid auctionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDto>> GetOrdersByBuyerAsync(string buyerUsername, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDto>> GetOrdersBySellerAsync(string sellerUsername, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateOrderFromAuctionAsync(CreateOrderDto dto, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, string? notes, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateShippingAsync(Guid orderId, UpdateShippingDto dto, CancellationToken cancellationToken = default);
    Task<OrderDto> ProcessPaymentAsync(Guid orderId, ProcessPaymentDto dto, CancellationToken cancellationToken = default);
    Task<OrderDto> ConfirmDeliveryAsync(Guid orderId, string username, CancellationToken cancellationToken = default);
}
