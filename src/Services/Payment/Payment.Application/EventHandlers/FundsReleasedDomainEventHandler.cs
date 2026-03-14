using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.EventHandlers;

public class FundsReleasedDomainEventHandler : INotificationHandler<FundsReleasedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<FundsReleasedDomainEventHandler> _logger;

    public FundsReleasedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<FundsReleasedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(FundsReleasedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Funds released from Wallet {WalletId}, Amount: {Amount}, NewHeldAmount: {NewHeldAmount}",
            notification.WalletId,
            notification.Amount,
            notification.NewHeldAmount);

        await _eventPublisher.PublishAsync(new FundsReleasedEvent
        {
            WalletId = notification.WalletId,
            UserId = notification.UserId,
            Amount = notification.Amount,
            NewHeldAmount = notification.NewHeldAmount,
            ReleasedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
