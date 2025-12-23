using Common.CQRS.Abstractions;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Queries.GetOrderByAuctionId;

public record GetOrderByAuctionIdQuery(Guid AuctionId) : IQuery<OrderDto?>;
