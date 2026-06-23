using Catalog.Domain.Events;
using Catalog.Contracts.Events;
using BuildingBlocks.Application.Abstractions.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.EventHandlers;

public class BrandUpdatedDomainEventHandler : INotificationHandler<BrandUpdatedDomainEvent>
{
    private readonly IEventPublisher _publishEndpoint;
    private readonly ILogger<BrandUpdatedDomainEventHandler> _logger;

    public BrandUpdatedDomainEventHandler(
        IEventPublisher publishEndpoint,
        ILogger<BrandUpdatedDomainEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(BrandUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing BrandUpdatedEvent for BrandId: {BrandId}", notification.BrandId);

        await _publishEndpoint.PublishAsync(new BrandUpdatedEvent
        {
            BrandId = notification.BrandId,
            Name = notification.Name
        }, cancellationToken);
    }
}
