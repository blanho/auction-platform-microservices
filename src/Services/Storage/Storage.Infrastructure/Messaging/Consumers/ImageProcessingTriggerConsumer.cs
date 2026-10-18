using System.Text.Json;
using JobService.Contracts.Commands;
using JobService.Contracts.Enums;
using Microsoft.Extensions.Logging;
using StorageService.Contracts.Events;
using StorageService.Contracts.Commands;

namespace Storage.Infrastructure.Messaging.Consumers;

public class ImageProcessingTriggerConsumer : IConsumer<FileUploadedEvent>
{
    private static readonly HashSet<string> ImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp"
    };

    private const int ImageProcessingJobPriority = 1;
    private const int ImageProcessingMaxRetryCount = 3;

    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ImageProcessingTriggerConsumer> _logger;

    public ImageProcessingTriggerConsumer(
        IPublishEndpoint publishEndpoint,
        ILogger<ImageProcessingTriggerConsumer> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FileUploadedEvent> context)
    {
        var message = context.Message;

        if (!ImageContentTypes.Contains(message.ContentType) || message.OwnerId is null || message.OwnerId == Guid.Empty)
        {
            _logger.LogDebug(
                "File {FileId} is not an image ({ContentType}), skipping image processing",
                message.FileId, message.ContentType);
            return;
        }

        _logger.LogInformation(
            "Triggering image processing for file {FileId} ({ContentType}, {FileSize} bytes)",
            message.FileId, message.ContentType, message.FileSize);

        var payload = JsonSerializer.Serialize(new ProcessImageCommand
        {
            FileId = message.FileId
        });

        var correlationId = $"img-proc-{message.FileId}";

        await _publishEndpoint.Publish(new RequestJobCommand
        {
            JobType = nameof(JobType.ImageProcessing),
            CorrelationId = correlationId,
            RequestedBy = message.OwnerId ?? Guid.Empty,
            PayloadJson = payload,
            Priority = ImageProcessingJobPriority,
            MaxRetryCount = ImageProcessingMaxRetryCount,
            TotalItems = 0,
            Items =
            [
                new RequestJobItemPayload
                {
                    PayloadJson = payload,
                    SequenceNumber = 1
                }
            ]
        }, context.CancellationToken);

        _logger.LogInformation(
            "Published ImageProcessing job request for file {FileId}, correlationId: {CorrelationId}",
            message.FileId, correlationId);
    }
}
