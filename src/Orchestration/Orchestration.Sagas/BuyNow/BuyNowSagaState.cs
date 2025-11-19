using MassTransit;

namespace Orchestration.Sagas.BuyNow;

public class BuyNowSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;

    public Guid AuctionId { get; set; }
    public Guid BuyerId { get; set; }
    public string BuyerUsername { get; set; } = string.Empty;
    public Guid SellerId { get; set; }
    public string SellerUsername { get; set; } = string.Empty;
    public decimal BuyNowPrice { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }

    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? FailureReason { get; set; }

    public int RetryCount { get; set; }

    public Guid? TimeoutTokenId { get; set; }
}
