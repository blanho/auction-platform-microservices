using Common.Contracts.Events;

namespace PaymentService.Contracts.Events;

public record WalletCreatedEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
