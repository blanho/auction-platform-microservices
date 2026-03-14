using BuildingBlocks.Application.Abstractions.Messaging;
using Identity.Api.DomainEvents;
using IdentityService.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Api.EventHandlers;

public class TwoFactorDisabledDomainEventHandler : INotificationHandler<TwoFactorDisabledDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<TwoFactorDisabledDomainEventHandler> _logger;

    public TwoFactorDisabledDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<TwoFactorDisabledDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(TwoFactorDisabledDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing TwoFactorDisabledDomainEvent for User {UserId} ({Username})",
            notification.UserId,
            notification.Username);

        await _eventPublisher.PublishAsync(new TwoFactorDisabledEvent
        {
            UserId = notification.UserId,
            Username = notification.Username,
            Email = notification.Email,
            DisabledAt = notification.OccurredAt
        }, cancellationToken);
    }
}
