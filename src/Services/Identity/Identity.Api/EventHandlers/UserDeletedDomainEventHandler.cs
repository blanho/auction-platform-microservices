using BuildingBlocks.Application.Abstractions.Messaging;
using Identity.Api.DomainEvents;
using IdentityService.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Api.EventHandlers;

public class UserDeletedDomainEventHandler : INotificationHandler<UserDeletedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserDeletedDomainEventHandler> _logger;

    public UserDeletedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<UserDeletedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(UserDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UserDeletedDomainEvent for User {UserId} ({Username})",
            notification.UserId,
            notification.Username);

        await _eventPublisher.PublishAsync(new UserDeletedEvent
        {
            UserId = notification.UserId,
            Username = notification.Username,
            DeletedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
