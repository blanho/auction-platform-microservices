using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.EventHandlers;

public class FundsWithdrawnDomainEventHandler : INotificationHandler<FundsWithdrawnDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<FundsWithdrawnDomainEventHandler> _logger;

    public FundsWithdrawnDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<FundsWithdrawnDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(FundsWithdrawnDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Funds withdrawn from Wallet {WalletId}, Amount: {Amount}, NewBalance: {NewBalance}",
            notification.WalletId,
            notification.Amount,
            notification.NewBalance);

        await _eventPublisher.PublishAsync(new FundsWithdrawnEvent
        {
            WalletId = notification.WalletId,
            UserId = notification.UserId,
            Amount = notification.Amount,
            NewBalance = notification.NewBalance,
            WithdrawnAt = notification.OccurredAt
        }, cancellationToken);
    }
}
