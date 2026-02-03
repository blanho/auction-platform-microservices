using Payment.Domain.Entities;

namespace Payment.Application.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid BuyerId { get; set; }
    public string BuyerUsername { get; set; } = string.Empty;
    public Guid SellerId { get; set; }
    public string SellerUsername { get; set; } = string.Empty;
    public string ItemTitle { get; set; } = string.Empty;
    public decimal WinningBid { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? ShippingCost { get; set; }
    public decimal? PlatformFee { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? PaymentTransactionId { get; set; }
    public string? ShippingAddress { get; set; }
    public string? TrackingNumber { get; set; }
    public string? ShippingCarrier { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset? ShippedAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public string? BuyerNotes { get; set; }
    public string? SellerNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class CreateOrderDto
{
    public Guid AuctionId { get; set; }
    public Guid? BuyerId { get; set; }
    public string? BuyerUsername { get; set; }
    public Guid? SellerId { get; set; }
    public string? SellerUsername { get; set; }
    public string? ItemTitle { get; set; }
    public decimal? WinningBid { get; set; }
    public decimal? ShippingCost { get; set; }
    public decimal? PlatformFee { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BuyerNotes { get; set; }
}

public class UpdateOrderDto
{
    public OrderStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public string? PaymentTransactionId { get; set; }
    public string? ShippingAddress { get; set; }
    public string? TrackingNumber { get; set; }
    public string? ShippingCarrier { get; set; }
    public string? BuyerNotes { get; set; }
    public string? SellerNotes { get; set; }
}

public class UpdateShippingDto
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string ShippingCarrier { get; set; } = string.Empty;
    public string? SellerNotes { get; set; }
}

public class ProcessPaymentDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ExternalTransactionId { get; set; }
}

public record RevenueStatsDto(
    decimal TotalRevenue,
    decimal TotalPlatformFees,
    int TotalTransactions,
    int CompletedOrders,
    int PendingOrders,
    int RefundedOrders,
    decimal AverageOrderValue,
    decimal RevenueToday,
    decimal RevenueThisWeek,
    decimal RevenueThisMonth
);

public record DailyRevenueStatDto(DateOnly Date, decimal Revenue, decimal PlatformFees, int OrderCount);

public record TopSellerDto(Guid SellerId, string Username, decimal TotalSales, int OrderCount, decimal AverageOrderValue);

public record TopBuyerDto(Guid BuyerId, string Username, decimal TotalSpent, int OrderCount);

public record OrderStatsDto(
    int TotalOrders,
    int PendingOrders,
    int PaidOrders,
    int ProcessingOrders,
    int ShippedOrders,
    int DeliveredOrders,
    int CompletedOrders,
    int CancelledOrders,
    int DisputedOrders,
    int RefundedOrders,
    decimal TotalRevenue,
    decimal AverageOrderValue
);

public class CancelOrderDto
{
    public string? Reason { get; set; }
}
