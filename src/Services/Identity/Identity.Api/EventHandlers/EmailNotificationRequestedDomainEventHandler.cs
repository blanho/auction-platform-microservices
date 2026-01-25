using BuildingBlocks.Application.Abstractions.Messaging;
using Identity.Api.DomainEvents;
using NotificationService.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Api.EventHandlers;

public class EmailNotificationRequestedDomainEventHandler : INotificationHandler<EmailNotificationRequestedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<EmailNotificationRequestedDomainEventHandler> _logger;

    public EmailNotificationRequestedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<EmailNotificationRequestedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(EmailNotificationRequestedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing EmailNotificationRequestedDomainEvent for User {UserId} with template {TemplateKey}",
            notification.UserId,
            notification.TemplateKey);

        await _eventPublisher.PublishAsync(new EmailNotificationRequestedEvent
        {
            EventId = notification.EventId.ToString("N"),
            UserId = notification.UserId,
            RecipientEmail = notification.RecipientEmail,
            RecipientName = notification.RecipientName,
            TemplateKey = notification.TemplateKey,
            Subject = notification.Subject,
            Data = notification.Data,
            Source = "identity-service"
        }, cancellationToken);
    }
}
