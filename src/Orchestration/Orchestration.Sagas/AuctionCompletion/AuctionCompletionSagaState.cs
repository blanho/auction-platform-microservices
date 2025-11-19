using MassTransit;

namespace Orchestration.Sagas.AuctionCompletion;

public class AuctionCompletionSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;

    public Guid AuctionId { get; set; }
    public Guid SellerId { get; set; }
    public string SellerUsername { get; set; } = string.Empty;
    public Guid WinnerId { get; set; }
    public string WinnerUsername { get; set; } = string.Empty;
    public decimal WinningBidAmount { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }

    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? OrderCreatedAt { get; set; }
    public DateTimeOffset? NotificationsSentAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? FailureReason { get; set; }

    public int RetryCount { get; set; }

    public Guid? TimeoutTokenId { get; set; }
}
