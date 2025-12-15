using PaymentService.Domain.Entities;

namespace PaymentService.Application.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public string BuyerUsername { get; set; } = string.Empty;
    public string SellerUsername { get; set; } = string.Empty;
    public string ItemTitle { get; set; } = string.Empty;
    public decimal WinningBid { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? ShippingCost { get; set; }
    public decimal? PlatformFee { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentTransactionId { get; set; }
    public string ShippingAddress { get; set; }
    public string TrackingNumber { get; set; }
    public string ShippingCarrier { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset? ShippedAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public string BuyerNotes { get; set; }
    public string SellerNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CreateOrderDto
{
    public Guid AuctionId { get; set; }
    public string BuyerUsername { get; set; } = string.Empty;
    public string SellerUsername { get; set; } = string.Empty;
    public string ItemTitle { get; set; } = string.Empty;
    public decimal WinningBid { get; set; }
    public decimal? ShippingCost { get; set; }
    public decimal? PlatformFee { get; set; }
    public string ShippingAddress { get; set; }
    public string BuyerNotes { get; set; }
}

public class UpdateOrderDto
{
    public OrderStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public string PaymentTransactionId { get; set; }
    public string ShippingAddress { get; set; }
    public string TrackingNumber { get; set; }
    public string ShippingCarrier { get; set; }
    public string BuyerNotes { get; set; }
    public string SellerNotes { get; set; }
}
