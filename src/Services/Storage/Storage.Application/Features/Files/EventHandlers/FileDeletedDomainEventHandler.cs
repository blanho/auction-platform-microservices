using MassTransit;
using MediatR;
using Storage.Domain.Entities;
using StorageService.Contracts.Events;

namespace Storage.Application.Features.Files.EventHandlers;

public class FileDeletedDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    ILogger<FileDeletedDomainEventHandler> logger)
    : INotificationHandler<FileDeletedDomainEvent>
{
    public async Task Handle(FileDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogDebug("Publishing FileDeletedEvent for FileId: {FileId}", notification.FileId);

        await publishEndpoint.Publish(new FileDeletedEvent
        {
            FileId = notification.FileId,
            FileName = notification.FileName,
            OwnerId = notification.OwnerId,
            DeletedAt = DateTimeOffset.UtcNow
        }, cancellationToken);
    }
}
