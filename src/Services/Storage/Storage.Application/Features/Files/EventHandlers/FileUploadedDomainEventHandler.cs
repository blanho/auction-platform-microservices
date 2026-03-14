using MassTransit;
using MediatR;
using Storage.Domain.Entities;
using StorageService.Contracts.Events;

namespace Storage.Application.Features.Files.EventHandlers;

public class FileUploadedDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    ILogger<FileUploadedDomainEventHandler> logger)
    : INotificationHandler<FileUploadedDomainEvent>
{
    public async Task Handle(FileUploadedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogDebug("Publishing FileUploadedEvent for FileId: {FileId}", notification.FileId);

        await publishEndpoint.Publish(new FileUploadedEvent
        {
            FileId = notification.FileId,
            FileName = notification.FileName,
            ContentType = notification.ContentType,
            FileSize = notification.FileSize,
            Url = notification.Url,
            OwnerId = notification.OwnerId,
            UploadedAt = DateTimeOffset.UtcNow
        }, cancellationToken);
    }
}
