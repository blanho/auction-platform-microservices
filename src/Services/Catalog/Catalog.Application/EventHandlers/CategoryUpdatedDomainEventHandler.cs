using Catalog.Domain.Events;
using Catalog.Contracts.Events;
using BuildingBlocks.Application.Abstractions.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.EventHandlers;

public class CategoryUpdatedDomainEventHandler : INotificationHandler<CategoryUpdatedDomainEvent>
{
    private readonly IEventPublisher _publishEndpoint;
    private readonly ILogger<CategoryUpdatedDomainEventHandler> _logger;

    public CategoryUpdatedDomainEventHandler(
        IEventPublisher publishEndpoint,
        ILogger<CategoryUpdatedDomainEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(CategoryUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing CategoryUpdatedEvent for CategoryId: {CategoryId}", notification.CategoryId);

        await _publishEndpoint.PublishAsync(new CategoryUpdatedEvent
        {
            CategoryId = notification.CategoryId,
            Name = notification.Name
        }, cancellationToken);
    }
}
