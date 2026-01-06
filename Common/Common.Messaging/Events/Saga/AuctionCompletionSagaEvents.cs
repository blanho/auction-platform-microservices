namespace Common.Messaging.Events.Saga;

/// <summary>
/// Initiates the auction completion saga when an auction ends with a winner.
/// </summary>
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

/// <summary>
/// Command to create an order for the auction winner.
/// </summary>
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

/// <summary>
/// Event indicating the winner's order was created successfully.
/// </summary>
public record AuctionWinnerOrderCreated
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid OrderId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Event indicating the winner's order creation failed.
/// </summary>
public record AuctionWinnerOrderFailed
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Command to send notifications about auction completion.
/// </summary>
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

/// <summary>
/// Event indicating notifications were sent successfully.
/// </summary>
public record AuctionCompletionNotificationsSent
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public DateTimeOffset SentAt { get; init; }
}

/// <summary>
/// Event indicating notification sending failed.
/// </summary>
public record AuctionCompletionNotificationsFailed
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Command to revert auction status if saga fails.
/// </summary>
public record RevertAuctionCompletion
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Event indicating auction completion was reverted.
/// </summary>
public record AuctionCompletionReverted
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public DateTimeOffset RevertedAt { get; init; }
}

/// <summary>
/// Final event indicating the saga completed (success or failure).
/// </summary>
public record AuctionCompletionSagaCompleted
{
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid? OrderId { get; init; }
    public bool Success { get; init; }
    public string? FailureReason { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
}
