using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.GetOrderByAuctionId;

public record GetOrderByAuctionIdQuery(Guid AuctionId) : IQuery<OrderDto?>;
