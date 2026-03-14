using BuildingBlocks.Application.Abstractions.Messaging;
using Identity.Api.DomainEvents;
using IdentityService.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Api.EventHandlers;

public class UserLoginDomainEventHandler : INotificationHandler<UserLoginDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserLoginDomainEventHandler> _logger;

    public UserLoginDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<UserLoginDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(UserLoginDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Processing UserLoginDomainEvent for User {UserId}",
            notification.UserId);

        await _eventPublisher.PublishAsync(new UserLoginEvent
        {
            UserId = notification.UserId,
            Username = notification.Username,
            Email = notification.Email,
            IpAddress = notification.IpAddress,
            LoginAt = notification.OccurredAt
        }, cancellationToken);
    }
}
