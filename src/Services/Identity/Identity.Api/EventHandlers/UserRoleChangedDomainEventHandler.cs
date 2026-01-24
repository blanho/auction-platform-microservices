using BuildingBlocks.Application.Abstractions.Messaging;
using Identity.Api.DomainEvents;
using IdentityService.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Api.EventHandlers;

public class UserRoleChangedDomainEventHandler : INotificationHandler<UserRoleChangedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserRoleChangedDomainEventHandler> _logger;

    public UserRoleChangedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<UserRoleChangedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(UserRoleChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UserRoleChangedDomainEvent for User {UserId} ({Username}) with roles [{Roles}]",
            notification.UserId,
            notification.Username,
            string.Join(", ", notification.Roles));

        await _eventPublisher.PublishAsync(new UserRoleChangedEvent
        {
            UserId = notification.UserId,
            Username = notification.Username,
            Roles = notification.Roles,
            ChangedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
