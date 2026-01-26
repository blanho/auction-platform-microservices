using BuildingBlocks.Application.Abstractions.Messaging;
using Identity.Api.DomainEvents;
using IdentityService.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Api.EventHandlers;

public class SecurityAlertDomainEventHandler : INotificationHandler<SecurityAlertDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<SecurityAlertDomainEventHandler> _logger;

    public SecurityAlertDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<SecurityAlertDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(SecurityAlertDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Processing SecurityAlertDomainEvent: UserId={UserId}, AlertType={AlertType}, IP={IpAddress}",
            notification.UserId,
            notification.AlertType,
            notification.IpAddress ?? "unknown");

        await _eventPublisher.PublishAsync(new SecurityAlertEvent
        {
            UserId = notification.UserId,
            Username = notification.Username,
            Email = notification.Email,
            AlertType = notification.AlertType,
            Description = notification.Description,
            IpAddress = notification.IpAddress,
            OccurredAt = notification.OccurredAt
        }, cancellationToken);
    }
}
