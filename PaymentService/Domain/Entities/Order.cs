using Common.Domain.Entities;
using PaymentService.Domain.Events;

namespace PaymentService.Domain.Entities;

public class Order : BaseEntity
{
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
    public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? PaymentTransactionId { get; set; }
    public string? ShippingAddress { get; set; }
    public string? TrackingNumber { get; set; }
    public string? ShippingCarrier { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset? ShippedAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public string? BuyerNotes { get; set; }
    public string? SellerNotes { get; set; }

    public void RaiseCreatedEvent()
    {
        AddDomainEvent(new OrderCreatedDomainEvent
        {
            OrderId = Id,
            AuctionId = AuctionId,
            BuyerId = BuyerId,
            BuyerUsername = BuyerUsername,
            SellerId = SellerId,
            SellerUsername = SellerUsername,
            ItemTitle = ItemTitle,
            TotalAmount = TotalAmount
        });
    }

    public void ChangeStatus(OrderStatus newStatus)
    {
        var oldStatus = Status;
        Status = newStatus;
        AddDomainEvent(new OrderStatusChangedDomainEvent
        {
            OrderId = Id,
            AuctionId = AuctionId,
            BuyerId = BuyerId,
            BuyerUsername = BuyerUsername,
            OldStatus = oldStatus,
            NewStatus = newStatus
        });
    }

    public void CompletePayment(string? transactionId)
    {
        PaymentStatus = PaymentStatus.Completed;
        PaymentTransactionId = transactionId;
        PaidAt = DateTimeOffset.UtcNow;
        Status = OrderStatus.PaymentReceived;

        AddDomainEvent(new PaymentCompletedDomainEvent
        {
            OrderId = Id,
            AuctionId = AuctionId,
            BuyerId = BuyerId,
            BuyerUsername = BuyerUsername,
            SellerId = SellerId,
            SellerUsername = SellerUsername,
            Amount = TotalAmount,
            TransactionId = transactionId
        });
    }

    public void MarkAsShipped(string? trackingNumber, string? carrier)
    {
        TrackingNumber = trackingNumber;
        ShippingCarrier = carrier;
        ShippedAt = DateTimeOffset.UtcNow;
        Status = OrderStatus.Shipped;

        AddDomainEvent(new OrderShippedDomainEvent
        {
            OrderId = Id,
            AuctionId = AuctionId,
            BuyerId = BuyerId,
            BuyerUsername = BuyerUsername,
            TrackingNumber = trackingNumber,
            ShippingCarrier = carrier
        });
    }

    public void MarkAsDelivered()
    {
        DeliveredAt = DateTimeOffset.UtcNow;
        Status = OrderStatus.Delivered;

        AddDomainEvent(new OrderDeliveredDomainEvent
        {
            OrderId = Id,
            AuctionId = AuctionId,
            BuyerId = BuyerId,
            BuyerUsername = BuyerUsername,
            SellerId = SellerId,
            SellerUsername = SellerUsername
        });
    }
}

public enum OrderStatus
{
    PendingPayment,
    PaymentReceived,
    Processing,
    Shipped,
    Delivered,
    Completed,
    Cancelled,
    Disputed,
    Refunded
}

public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded
}
