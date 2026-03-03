using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.EventHandlers;

public class FundsDepositedDomainEventHandler : INotificationHandler<FundsDepositedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<FundsDepositedDomainEventHandler> _logger;

    public FundsDepositedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<FundsDepositedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(FundsDepositedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Funds deposited to Wallet {WalletId}, Amount: {Amount}, NewBalance: {NewBalance}",
            notification.WalletId,
            notification.Amount,
            notification.NewBalance);

        await _eventPublisher.PublishAsync(new FundsDepositedEvent
        {
            WalletId = notification.WalletId,
            UserId = notification.UserId,
            Amount = notification.Amount,
            NewBalance = notification.NewBalance,
            DepositedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
