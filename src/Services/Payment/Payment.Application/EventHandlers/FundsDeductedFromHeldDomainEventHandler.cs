using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.EventHandlers;

public class FundsDeductedFromHeldDomainEventHandler : INotificationHandler<FundsDeductedFromHeldDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<FundsDeductedFromHeldDomainEventHandler> _logger;

    public FundsDeductedFromHeldDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<FundsDeductedFromHeldDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(FundsDeductedFromHeldDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Funds deducted from held in Wallet {WalletId}, Amount: {Amount}, NewBalance: {NewBalance}",
            notification.WalletId,
            notification.Amount,
            notification.NewBalance);

        await _eventPublisher.PublishAsync(new FundsDeductedFromHeldEvent
        {
            WalletId = notification.WalletId,
            UserId = notification.UserId,
            Amount = notification.Amount,
            NewBalance = notification.NewBalance,
            NewHeldAmount = notification.NewHeldAmount,
            DeductedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
