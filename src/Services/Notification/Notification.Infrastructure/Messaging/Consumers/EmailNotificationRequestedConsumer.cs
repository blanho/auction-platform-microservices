using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using NotificationService.Contracts.Events;
using IUnitOfWork = Notification.Application.Interfaces.IUnitOfWork;

namespace Notification.Infrastructure.Consumers;

public class EmailNotificationRequestedConsumer : IConsumer<EmailNotificationRequestedEvent>
{
    private readonly IIdempotencyService _idempotency;
    private readonly ITemplateRepository _templateRepo;
    private readonly INotificationRecordRepository _recordRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailNotificationRequestedConsumer> _logger;

    public EmailNotificationRequestedConsumer(
        IIdempotencyService idempotency,
        ITemplateRepository templateRepo,
        INotificationRecordRepository recordRepo,
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        ILogger<EmailNotificationRequestedConsumer> logger)
    {
        _idempotency = idempotency;
        _templateRepo = templateRepo;
        _recordRepo = recordRepo;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EmailNotificationRequestedEvent> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        _logger.LogInformation(
            "Processing EmailNotificationRequestedEvent: EventId={EventId}, UserId={UserId}, Template={TemplateKey}",
            message.EventId,
            message.UserId,
            message.TemplateKey);

        if (await _idempotency.IsProcessedAsync(message.EventId, "Email", ct))
        {
            _logger.LogDebug("Email already sent for EventId={EventId}", message.EventId);
            return;
        }

        var template = await _templateRepo.GetByKeyAsync(message.TemplateKey, ct);
        if (template == null || !template.IsActive)
        {
            _logger.LogWarning("Template not found or inactive: {TemplateKey}", message.TemplateKey);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(message.EventId, "Email", ct: ct);
        if (lockHandle == null)
        {
            _logger.LogDebug("Could not acquire lock for email, EventId={EventId}", message.EventId);
            return;
        }

        if (await _idempotency.IsProcessedAsync(message.EventId, "Email", ct))
            return;

        var subject = TemplateHelper.RenderTemplate(template.Subject ?? message.Subject, message.Data);
        var body = TemplateHelper.RenderTemplate(template.Body, message.Data);

        var record = Notification.Domain.Entities.NotificationRecord.Create(
            Guid.TryParse(message.UserId, out var uid) ? uid : Guid.Empty,
            message.TemplateKey,
            "Email",
            subject,
            message.RecipientEmail);

        try
        {
            var result = await _emailSender.SendAsync(
                message.RecipientEmail,
                subject,
                body,
                TemplateHelper.StripHtml(body),
                ct);

            if (result.Success)
            {
                record.MarkAsSent(result.MessageId);
                await _idempotency.MarkAsProcessedAsync(message.EventId, "Email", result.MessageId, ct: ct);
                _logger.LogInformation(
                    "Email sent: EventId={EventId}, MessageId={MessageId}",
                    message.EventId,
                    result.MessageId);
            }
            else
            {
                record.MarkAsFailed(result.Error ?? "Unknown error");
                _logger.LogWarning(
                    "Email failed: EventId={EventId}, Error={Error}",
                    message.EventId,
                    result.Error);
                throw new Exception($"Email delivery failed: {result.Error}");
            }
        }
        finally
        {
            await _recordRepo.AddRecordAsync(record, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
