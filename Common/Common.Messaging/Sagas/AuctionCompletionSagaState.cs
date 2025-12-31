using MassTransit;

namespace Common.Messaging.Sagas;

/// <summary>
/// State for the Auction Completion Saga.
/// Tracks the progress of finalizing an auction after it ends with a winner.
/// </summary>
public class AuctionCompletionSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    
    // Auction details
    public Guid AuctionId { get; set; }
    public Guid SellerId { get; set; }
    public string SellerUsername { get; set; } = string.Empty;
    public Guid WinnerId { get; set; }
    public string WinnerUsername { get; set; } = string.Empty;
    public decimal WinningBidAmount { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    
    // Order details
    public Guid? OrderId { get; set; }
    
    // Timestamps
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? OrderCreatedAt { get; set; }
    public DateTimeOffset? NotificationsSentAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    
    // Error tracking
    public string? FailureReason { get; set; }
    public int RetryCount { get; set; }
}
