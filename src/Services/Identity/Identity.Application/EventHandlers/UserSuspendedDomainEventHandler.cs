using BuildingBlocks.Application.Abstractions.Messaging;
using Identity.Domain.Events;
using IdentityService.Contracts.Events;
using MediatR;

namespace Identity.Application.EventHandlers;

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
