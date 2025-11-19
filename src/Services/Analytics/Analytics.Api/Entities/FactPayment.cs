namespace Analytics.Api.Entities;

public class FactPayment
{

    public Guid EventId { get; set; }
    public DateTimeOffset EventTime { get; set; }
    public DateTimeOffset IngestedAt { get; set; }

    public Guid OrderId { get; set; }
    public Guid AuctionId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid? SellerId { get; set; }
    public DateOnly DateKey { get; set; }

    public string BuyerUsername { get; set; } = string.Empty;
    public string SellerUsername { get; set; } = string.Empty;
    public string? AuctionTitle { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? TransactionId { get; set; }
    public string? TrackingNumber { get; set; }
    public string? ShippingCarrier { get; set; }

    public bool IsPaid { get; set; }
    public bool IsRefunded { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset? ShippedAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }

    public string EventType { get; set; } = string.Empty;

    public short EventVersion { get; set; }
}
