namespace Payment.Domain.Events;

public record FundsReleasedDomainEvent : DomainEvent
{
    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public decimal NewHeldAmount { get; init; }
}
