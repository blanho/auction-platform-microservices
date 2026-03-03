using Common.Contracts.Events;

namespace PaymentService.Contracts.Events;

public record FundsReleasedEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public decimal NewHeldAmount { get; init; }
    public DateTimeOffset ReleasedAt { get; init; }
}
