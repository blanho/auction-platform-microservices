using BuildingBlocks.Application.Abstractions.Messaging;
using Identity.Api.DomainEvents;
using IdentityService.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Api.EventHandlers;

public class UserCreatedDomainEventHandler : INotificationHandler<UserCreatedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserCreatedDomainEventHandler> _logger;

    public UserCreatedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<UserCreatedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Processing UserCreatedDomainEvent for User {UserId}",
            notification.UserId);

        await _eventPublisher.PublishAsync(new UserCreatedEvent
        {
            UserId = notification.UserId,
            Username = notification.Username,
            Email = notification.Email,
            EmailConfirmed = notification.EmailConfirmed,
            FullName = notification.FullName,
            Role = notification.Role,
            ConfirmationLink = notification.ConfirmationLink,
            CreatedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
