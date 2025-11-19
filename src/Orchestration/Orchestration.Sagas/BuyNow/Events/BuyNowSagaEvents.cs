namespace Orchestration.Sagas.BuyNow.Events;

public record BuyNowSagaStarted
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BuyerId { get; init; }
    public string BuyerUsername { get; init; } = string.Empty;
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public decimal BuyNowPrice { get; init; }
    public string ItemTitle { get; init; } = string.Empty;
    public DateTimeOffset StartedAt { get; init; }
}

public record ReserveAuctionForBuyNow
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BuyerId { get; init; }
    public string BuyerUsername { get; init; } = string.Empty;
}

public record AuctionReservedForBuyNow
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public decimal BuyNowPrice { get; init; }
    public string ItemTitle { get; init; } = string.Empty;
    public DateTimeOffset ReservedAt { get; init; }
}

public record AuctionReservationFailed
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset FailedAt { get; init; }
}

public record CreateBuyNowOrder
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BuyerId { get; init; }
    public string BuyerUsername { get; init; } = string.Empty;
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public decimal BuyNowPrice { get; init; }
    public string ItemTitle { get; init; } = string.Empty;
}

public record BuyNowOrderCreated
{
    public Guid CorrelationId { get; init; }
    public Guid OrderId { get; init; }
    public Guid AuctionId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record BuyNowOrderCreationFailed
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset FailedAt { get; init; }
}

public record CompleteBuyNowAuction
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid OrderId { get; init; }
    public Guid BuyerId { get; init; }
    public string BuyerUsername { get; init; } = string.Empty;
}

public record BuyNowAuctionCompleted
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid OrderId { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
}

public record ReleaseAuctionReservation
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record AuctionReservationReleased
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public DateTimeOffset ReleasedAt { get; init; }
}

public record BuyNowSagaCompleted
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid OrderId { get; init; }
    public bool Success { get; init; }
    public string? FailureReason { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
}

public record BuyNowSagaTimedOut
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BuyerId { get; init; }
    public string BuyerUsername { get; init; } = string.Empty;
    public DateTimeOffset TimedOutAt { get; init; }
}
