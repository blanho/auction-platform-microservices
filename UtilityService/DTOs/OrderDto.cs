namespace UtilityService.DTOs;

public record OrderDto(
    Guid Id,
    Guid AuctionId,
    string BuyerUsername,
    string SellerUsername,
    string ItemTitle,
    decimal WinningBid,
    decimal TotalAmount,
    decimal? ShippingCost,
    decimal? PlatformFee,
    string Status,
    string PaymentStatus,
    string? TrackingNumber,
    string? ShippingCarrier,
    DateTimeOffset? PaidAt,
    DateTimeOffset? ShippedAt,
    DateTimeOffset? DeliveredAt,
    DateTimeOffset CreatedAt
);

public record CreateOrderDto(
    Guid AuctionId,
    string SellerUsername,
    string ItemTitle,
    decimal WinningBid,
    decimal? ShippingCost
);

public record UpdateOrderStatusDto(
    string Status,
    string? TrackingNumber,
    string? ShippingCarrier,
    string? Notes
);

public record OrderSummaryDto(
    int TotalOrders,
    int PendingPayment,
    int AwaitingShipment,
    int Shipped,
    int Completed,
    decimal TotalRevenue
);
