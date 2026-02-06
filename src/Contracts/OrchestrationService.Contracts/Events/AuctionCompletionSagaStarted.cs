using Common.Contracts.Events;

namespace OrchestrationService.Contracts.Events;

public record AuctionCompletionSagaStarted : IVersionedEvent
{
    public int Version => 1;
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public Guid WinnerId { get; init; }
    public string WinnerUsername { get; init; } = string.Empty;
    public decimal WinningBidAmount { get; init; }
    public string ItemTitle { get; init; } = string.Empty;
    public DateTimeOffset AuctionEndedAt { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
