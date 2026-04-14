using System.Text.Json;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;
using StorageService.Contracts.Events;

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

        if (!ImageContentTypes.Contains(message.ContentType))
        {
            _logger.LogDebug(
                "File {FileId} is not an image ({ContentType}), skipping image processing",
                message.FileId, message.ContentType);
            return;
        }

        _logger.LogInformation(
            "Triggering image processing for file {FileId} ({ContentType}, {FileSize} bytes)",
            message.FileId, message.ContentType, message.FileSize);

        var payload = JsonSerializer.Serialize(new ImageProcessingPayload
        {
            FileId = message.FileId,
            FileName = message.FileName,
            ContentType = message.ContentType,
            FileSize = message.FileSize,
            Url = message.Url,
            OwnerId = message.OwnerId
        });

        var correlationId = $"img-proc-{message.FileId}";

        await _publishEndpoint.Publish(new RequestJobCommand
        {
            JobType = nameof(JobService.Contracts.Enums.JobType.ImageProcessing),
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

public record ImageProcessingPayload
{
    public Guid FileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string Url { get; init; } = string.Empty;
    public Guid? OwnerId { get; init; }
}
