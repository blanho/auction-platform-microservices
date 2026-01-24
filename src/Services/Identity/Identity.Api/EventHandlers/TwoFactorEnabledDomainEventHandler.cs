using BuildingBlocks.Application.Abstractions.Messaging;
using Identity.Api.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Api.EventHandlers;

public class TwoFactorEnabledDomainEventHandler : INotificationHandler<TwoFactorEnabledDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<TwoFactorEnabledDomainEventHandler> _logger;

    public TwoFactorEnabledDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<TwoFactorEnabledDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public Task Handle(TwoFactorEnabledDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing TwoFactorEnabledDomainEvent for User {UserId} ({Username})",
            notification.UserId,
            notification.Username);

        return Task.CompletedTask;
    }
}
