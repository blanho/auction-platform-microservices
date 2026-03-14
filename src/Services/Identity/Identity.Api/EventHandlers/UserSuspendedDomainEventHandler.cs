using BuildingBlocks.Application.Abstractions.Messaging;
using Identity.Api.DomainEvents;
using IdentityService.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Api.EventHandlers;

public class UserSuspendedDomainEventHandler : INotificationHandler<UserSuspendedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserSuspendedDomainEventHandler> _logger;

    public UserSuspendedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<UserSuspendedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(UserSuspendedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UserSuspendedDomainEvent for User {UserId} ({Username})",
            notification.UserId,
            notification.Username);

        await _eventPublisher.PublishAsync(new UserSuspendedEvent
        {
            UserId = notification.UserId,
            Username = notification.Username,
            Reason = notification.Reason,
            SuspendedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
