using Common.Contracts.Events;

namespace PaymentService.Contracts.Events;

public record FundsDeductedFromHeldEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public decimal NewBalance { get; init; }
    public decimal NewHeldAmount { get; init; }
    public DateTimeOffset DeductedAt { get; init; }
}
