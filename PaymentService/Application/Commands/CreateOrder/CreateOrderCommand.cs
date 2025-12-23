using Common.CQRS.Abstractions;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Commands.CreateOrder;

public record CreateOrderCommand(
    Guid AuctionId,
    Guid BuyerId,
    string BuyerUsername,
    Guid SellerId,
    string SellerUsername,
    string ItemTitle,
    decimal WinningBid,
    decimal? ShippingCost,
    decimal? PlatformFee,
    string? ShippingAddress,
    string? BuyerNotes
) : ICommand<OrderDto>;
