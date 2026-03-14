using BuildingBlocks.Application.Abstractions.Messaging;
using Identity.Api.DomainEvents;
using IdentityService.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Api.EventHandlers;

public class UserEmailConfirmedDomainEventHandler : INotificationHandler<UserEmailConfirmedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserEmailConfirmedDomainEventHandler> _logger;

    public UserEmailConfirmedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<UserEmailConfirmedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(UserEmailConfirmedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UserEmailConfirmedDomainEvent for User {UserId} ({Username})",
            notification.UserId,
            notification.Username);

        await _eventPublisher.PublishAsync(new UserEmailConfirmedEvent
        {
            UserId = notification.UserId,
            Username = notification.Username,
            Email = notification.Email,
            ConfirmedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
