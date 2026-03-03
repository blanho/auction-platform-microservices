using MassTransit;
using NotificationService.Contracts.Commands;

namespace Notification.Application.Features.Notifications.QueueBulkNotification;

public class QueueBulkNotificationCommandHandler : ICommandHandler<QueueBulkNotificationCommand, BackgroundJobResult>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<QueueBulkNotificationCommandHandler> _logger;

    public QueueBulkNotificationCommandHandler(
        IPublishEndpoint publishEndpoint,
        ILogger<QueueBulkNotificationCommandHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<BackgroundJobResult>> Handle(
        QueueBulkNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();

        _logger.LogInformation(
            "Queueing bulk notification job {CorrelationId} - Template: {TemplateKey}, Recipients: {RecipientCount}",
            correlationId, request.TemplateKey, request.Recipients.Count);

        var command = new SendBulkNotificationCommand
        {
            CorrelationId = correlationId,
            RequestedBy = request.RequestedBy,
            TemplateKey = request.TemplateKey,
            Title = request.Title,
            Message = request.Message,
            Channels = request.Channels.Select(c => c.ToString()).ToList(),
            Recipients = request.Recipients.Select(r => new BulkNotificationRecipient
            {
                UserId = r.UserId,
                Email = r.Email,
                PhoneNumber = r.PhoneNumber,
                Parameters = r.Parameters ?? []
            }).ToList(),
            GlobalParameters = request.GlobalParameters ?? [],
            ScheduledAt = request.ScheduledAt,
            BatchSize = request.BatchSize
        };

        await _publishEndpoint.Publish(command, cancellationToken);

        _logger.LogInformation(
            "Bulk notification job queued {CorrelationId} - Recipients: {RecipientCount}",
            correlationId, request.Recipients.Count);

        return Result.Success(new BackgroundJobResult(
            JobId: correlationId,
            CorrelationId: correlationId.ToString(),
            Status: "Queued",
            Message: $"Bulk notification job queued for {request.Recipients.Count} recipients"));
    }
}
