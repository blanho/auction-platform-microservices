using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.Commands.CreateOrder;

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
