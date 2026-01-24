using BuildingBlocks.Application.Abstractions.Messaging;
using Identity.Api.DomainEvents;
using IdentityService.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Api.EventHandlers;

public class UserReactivatedDomainEventHandler : INotificationHandler<UserReactivatedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserReactivatedDomainEventHandler> _logger;

    public UserReactivatedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<UserReactivatedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(UserReactivatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UserReactivatedDomainEvent for User {UserId} ({Username})",
            notification.UserId,
            notification.Username);

        await _eventPublisher.PublishAsync(new UserReactivatedEvent
        {
            UserId = notification.UserId,
            Username = notification.Username,
            ReactivatedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
