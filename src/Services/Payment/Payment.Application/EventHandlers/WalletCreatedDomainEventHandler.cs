using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.EventHandlers;

public class WalletCreatedDomainEventHandler : INotificationHandler<WalletCreatedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<WalletCreatedDomainEventHandler> _logger;

    public WalletCreatedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<WalletCreatedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(WalletCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Wallet {WalletId} created for User {UserId}, Currency: {Currency}",
            notification.WalletId,
            notification.UserId,
            notification.Currency);

        await _eventPublisher.PublishAsync(new WalletCreatedEvent
        {
            WalletId = notification.WalletId,
            UserId = notification.UserId,
            Username = notification.Username,
            Currency = notification.Currency,
            CreatedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
