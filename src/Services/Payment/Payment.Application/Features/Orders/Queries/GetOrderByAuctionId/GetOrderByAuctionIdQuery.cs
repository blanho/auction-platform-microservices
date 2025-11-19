using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.Queries.GetOrderByAuctionId;

public record GetOrderByAuctionIdQuery(Guid AuctionId) : IQuery<OrderDto?>;
