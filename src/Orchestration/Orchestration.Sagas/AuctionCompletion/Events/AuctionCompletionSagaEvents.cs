namespace Orchestration.Sagas.AuctionCompletion.Events;

public record AuctionCompletionSagaStarted
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public Guid WinnerId { get; init; }
    public string WinnerUsername { get; init; } = string.Empty;
    public decimal WinningBidAmount { get; init; }
    public string ItemTitle { get; init; } = string.Empty;
    public DateTimeOffset AuctionEndedAt { get; init; }
}

public record CreateAuctionWinnerOrder
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public Guid WinnerId { get; init; }
    public string WinnerUsername { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string ItemTitle { get; init; } = string.Empty;
}

public record AuctionWinnerOrderCreated
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid OrderId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record AuctionWinnerOrderFailed
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record SendAuctionCompletionNotifications
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid OrderId { get; init; }
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public Guid WinnerId { get; init; }
    public string WinnerUsername { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string ItemTitle { get; init; } = string.Empty;
}

public record AuctionCompletionNotificationsSent
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public DateTimeOffset SentAt { get; init; }
}

public record AuctionCompletionNotificationsFailed
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record RevertAuctionCompletion
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record AuctionCompletionReverted
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public DateTimeOffset RevertedAt { get; init; }
}

public record AuctionCompletionSagaCompleted
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid? OrderId { get; init; }
    public bool Success { get; init; }
    public string? FailureReason { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
}

public record AuctionCompletionSagaTimedOut
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public DateTimeOffset TimedOutAt { get; init; }
}
