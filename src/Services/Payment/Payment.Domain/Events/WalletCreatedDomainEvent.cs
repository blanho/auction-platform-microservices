namespace Payment.Domain.Events;

public record WalletCreatedDomainEvent : DomainEvent
{
    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
}
