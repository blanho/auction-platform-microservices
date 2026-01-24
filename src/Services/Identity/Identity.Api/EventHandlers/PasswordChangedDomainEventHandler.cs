using BuildingBlocks.Application.Abstractions.Messaging;
using Identity.Api.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Api.EventHandlers;

public class PasswordChangedDomainEventHandler : INotificationHandler<PasswordChangedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<PasswordChangedDomainEventHandler> _logger;

    public PasswordChangedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<PasswordChangedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public Task Handle(PasswordChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing PasswordChangedDomainEvent for User {UserId} ({Username})",
            notification.UserId,
            notification.Username);

        return Task.CompletedTask;
    }
}
