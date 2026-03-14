namespace Payment.Domain.Events;

public record FundsDepositedDomainEvent : DomainEvent
{
    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public decimal NewBalance { get; init; }
}
