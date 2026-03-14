using BuildingBlocks.Application.Abstractions.Messaging;
using Identity.Api.DomainEvents;
using IdentityService.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Api.EventHandlers;

public class UserUpdatedDomainEventHandler : INotificationHandler<UserUpdatedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserUpdatedDomainEventHandler> _logger;

    public UserUpdatedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<UserUpdatedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(UserUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UserUpdatedDomainEvent for User {UserId} ({Username})",
            notification.UserId,
            notification.Username);

        await _eventPublisher.PublishAsync(new UserUpdatedEvent
        {
            UserId = notification.UserId,
            Username = notification.Username,
            Email = notification.Email,
            FullName = notification.FullName,
            PhoneNumber = notification.PhoneNumber,
            UpdatedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
