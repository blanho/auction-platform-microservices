using Common.Contracts.Events;

namespace PaymentService.Contracts.Events;

public record FundsWithdrawnEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public decimal NewBalance { get; init; }
    public DateTimeOffset WithdrawnAt { get; init; }
}
