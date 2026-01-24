using BuildingBlocks.Application.Abstractions.Messaging;
using Identity.Api.DomainEvents;
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

    public Task Handle(UserLoginDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UserLoginDomainEvent for User {UserId} ({Username}) from IP {IpAddress}",
            notification.UserId,
            notification.Username,
            notification.IpAddress);

        return Task.CompletedTask;
    }
}
