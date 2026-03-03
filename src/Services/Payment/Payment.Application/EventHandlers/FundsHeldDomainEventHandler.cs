using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.EventHandlers;

public class FundsHeldDomainEventHandler : INotificationHandler<FundsHeldDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<FundsHeldDomainEventHandler> _logger;

    public FundsHeldDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<FundsHeldDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(FundsHeldDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Funds held in Wallet {WalletId}, Amount: {Amount}, NewHeldAmount: {NewHeldAmount}",
            notification.WalletId,
            notification.Amount,
            notification.NewHeldAmount);

        await _eventPublisher.PublishAsync(new FundsHeldEvent
        {
            WalletId = notification.WalletId,
            UserId = notification.UserId,
            Amount = notification.Amount,
            NewHeldAmount = notification.NewHeldAmount,
            HeldAt = notification.OccurredAt
        }, cancellationToken);
    }
}
