using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;
using Payment.Domain.Enums;
using Payment.Domain.Events;

namespace Payment.Domain.Entities;

public class Order : BaseEntity
{
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> AllowedTransitions = new()
    {
        [OrderStatus.Pending] = [OrderStatus.PaymentPending, OrderStatus.Cancelled],
        [OrderStatus.PaymentPending] = [OrderStatus.Paid, OrderStatus.Cancelled],
        [OrderStatus.Paid] = [OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Cancelled, OrderStatus.Disputed],
        [OrderStatus.Processing] = [OrderStatus.Shipped, OrderStatus.Cancelled],
        [OrderStatus.Shipped] = [OrderStatus.Delivered, OrderStatus.Disputed],
        [OrderStatus.Delivered] = [OrderStatus.Completed, OrderStatus.Disputed],
        [OrderStatus.Completed] = [OrderStatus.Disputed],
        [OrderStatus.Disputed] = [OrderStatus.Refunded, OrderStatus.Completed],
        [OrderStatus.Cancelled] = [],
        [OrderStatus.Refunded] = [],
    };

    public Guid AuctionId { get; private set; }
    public Guid BuyerId { get; private set; }
    public string BuyerUsername { get; private set; } = string.Empty;
    public Guid SellerId { get; private set; }
    public string SellerUsername { get; private set; } = string.Empty;
    public string ItemTitle { get; private set; } = string.Empty;
    public decimal WinningBid { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal? ShippingCost { get; private set; }
    public decimal? PlatformFee { get; private set; }

    public OrderStatus Status { get; private set; } = OrderStatus.PaymentPending;

    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;

    public string? PaymentTransactionId { get; private set; }
    public string? ShippingAddress { get; private set; }
    public string? TrackingNumber { get; private set; }
    public string? ShippingCarrier { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public DateTimeOffset? ShippedAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public string? BuyerNotes { get; private set; }
    public string? SellerNotes { get; private set; }

    public bool IsTerminal => Status is OrderStatus.Cancelled or OrderStatus.Refunded or OrderStatus.Completed;

    public static Order Create(
        Guid auctionId,
        Guid buyerId,
        string buyerUsername,
        Guid sellerId,
        string sellerUsername,
        string itemTitle,
        decimal winningBid,
        decimal? platformFeePercent = null)
    {
        decimal? platformFee = platformFeePercent.HasValue
            ? winningBid * platformFeePercent.Value / 100
            : null;

        return new Order
        {
            Id = Guid.NewGuid(),
            AuctionId = auctionId,
            BuyerId = buyerId,
            BuyerUsername = buyerUsername,
            SellerId = sellerId,
            SellerUsername = sellerUsername,
            ItemTitle = itemTitle,
            WinningBid = winningBid,
            TotalAmount = winningBid,
            PlatformFee = platformFee,
            Status = OrderStatus.PaymentPending,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void SetShippingAddress(string address) => ShippingAddress = address;

    public void SetShippingCost(decimal cost)
    {
        ShippingCost = cost;
        TotalAmount = WinningBid + cost;
    }

    public void AddBuyerNotes(string notes) => BuyerNotes = notes;

    public void AddSellerNotes(string notes) => SellerNotes = notes;

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
        GuardTransition(newStatus);

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
        GuardTransition(OrderStatus.Paid);

        PaymentStatus = PaymentStatus.Completed;
        PaymentTransactionId = transactionId;
        PaidAt = DateTimeOffset.UtcNow;
        Status = OrderStatus.Paid;

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
        GuardTransition(OrderStatus.Shipped);

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
        GuardTransition(OrderStatus.Delivered);

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

    public void Cancel(string? reason = null)
    {
        GuardTransition(OrderStatus.Cancelled);

        var oldStatus = Status;
        Status = OrderStatus.Cancelled;

        if (reason != null)
        {
            SellerNotes = SellerNotes != null
                ? $"{SellerNotes}\nCancellation reason: {reason}"
                : $"Cancellation reason: {reason}";
        }

        AddDomainEvent(new OrderStatusChangedDomainEvent
        {
            OrderId = Id,
            AuctionId = AuctionId,
            BuyerId = BuyerId,
            BuyerUsername = BuyerUsername,
            OldStatus = oldStatus,
            NewStatus = OrderStatus.Cancelled
        });
    }

    private void GuardTransition(OrderStatus target)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(target))
        {
            throw new InvalidEntityStateException(
                nameof(Order),
                Status.ToString(),
                $"Cannot transition from {Status} to {target}");
        }
    }
}
