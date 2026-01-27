using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using NotificationService.Contracts.Events;
using NotificationService.Contracts.Enums;
using IUnitOfWork = Notification.Application.Interfaces.IUnitOfWork;

namespace Notification.Infrastructure.Consumers;

public class NotificationRequestedConsumer : IConsumer<NotificationRequestedEvent>
{
    private readonly IIdempotencyService _idempotency;
    private readonly ITemplateRepository _templateRepo;
    private readonly INotificationRecordRepository _recordRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly IPushSender _pushSender;
    private readonly INotificationHubService _hubService;
    private readonly ILogger<NotificationRequestedConsumer> _logger;

    public NotificationRequestedConsumer(
        IIdempotencyService idempotency,
        ITemplateRepository templateRepo,
        INotificationRecordRepository recordRepo,
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        ISmsSender smsSender,
        IPushSender pushSender,
        INotificationHubService hubService,
        ILogger<NotificationRequestedConsumer> logger)
    {
        _idempotency = idempotency;
        _templateRepo = templateRepo;
        _recordRepo = recordRepo;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _pushSender = pushSender;
        _hubService = hubService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<NotificationRequestedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing notification request: EventId={EventId}, UserId={UserId}, Channels={Channels}",
            message.EventId,
            message.UserId,
            message.Channels);

        var template = await _templateRepo.GetByKeyAsync(message.TemplateKey, context.CancellationToken);
        if (template == null || !template.IsActive)
        {
            _logger.LogWarning(
                "Template not found or inactive: {TemplateKey}, EventId={EventId}",
                message.TemplateKey,
                message.EventId);
            return;
        }

        var tasks = new List<Task>();

        if (message.Channels.HasFlag(NotificationChannels.Email) && !string.IsNullOrEmpty(message.RecipientEmail))
        {
            tasks.Add(ProcessEmailAsync(message, template, context.CancellationToken));
        }

        if (message.Channels.HasFlag(NotificationChannels.Sms) && !string.IsNullOrEmpty(message.RecipientPhone))
        {
            tasks.Add(ProcessSmsAsync(message, template, context.CancellationToken));
        }

        if (message.Channels.HasFlag(NotificationChannels.Push))
        {
            tasks.Add(ProcessPushAsync(message, template, context.CancellationToken));
        }

        if (message.Channels.HasFlag(NotificationChannels.InApp))
        {
            tasks.Add(ProcessInAppAsync(message, template, context.CancellationToken));
        }

        await Task.WhenAll(tasks);
    }

    private async Task ProcessEmailAsync(
        NotificationRequestedEvent message,
        NotificationTemplate template,
        CancellationToken ct)
    {
        const string channel = "Email";

        if (await _idempotency.IsProcessedAsync(message.EventId, channel, ct))
        {
            _logger.LogDebug("Email already sent for EventId={EventId}", message.EventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(message.EventId, channel, ct: ct);
        if (lockHandle == null)
        {
            _logger.LogDebug("Could not acquire lock for email, EventId={EventId}", message.EventId);
            return;
        }

        if (await _idempotency.IsProcessedAsync(message.EventId, channel, ct))
            return;

        var subject = RenderTemplate(template.Subject, message.Data);
        var body = RenderTemplate(template.Body, message.Data);

        var record = NotificationRecord.Create(
            Guid.TryParse(message.UserId, out var uid) ? uid : Guid.Empty,
            message.TemplateKey,
            channel,
            subject,
            message.RecipientEmail);

        try
        {
            var result = await _emailSender.SendAsync(
                message.RecipientEmail!,
                subject,
                body,
                StripHtml(body),
                ct);

            if (result.Success)
            {
                record.MarkAsSent(result.MessageId);
                await _idempotency.MarkAsProcessedAsync(message.EventId, channel, result.MessageId, ct: ct);
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

    private async Task ProcessSmsAsync(
        NotificationRequestedEvent message,
        NotificationTemplate template,
        CancellationToken ct)
    {
        const string channel = "Sms";

        if (string.IsNullOrEmpty(template.SmsBody))
        {
            _logger.LogDebug("Template has no SMS body: {TemplateKey}", message.TemplateKey);
            return;
        }

        if (await _idempotency.IsProcessedAsync(message.EventId, channel, ct))
            return;

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(message.EventId, channel, ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(message.EventId, channel, ct))
            return;

        var smsBody = RenderTemplate(template.SmsBody, message.Data);

        var record = NotificationRecord.Create(
            Guid.TryParse(message.UserId, out var uid) ? uid : Guid.Empty,
            message.TemplateKey,
            channel,
            smsBody[..Math.Min(50, smsBody.Length)],
            message.RecipientPhone);

        try
        {
            var result = await _smsSender.SendAsync(message.RecipientPhone!, smsBody, ct);

            if (result.Success)
            {
                record.MarkAsSent(result.MessageId);
                await _idempotency.MarkAsProcessedAsync(message.EventId, channel, result.MessageId, ct: ct);
            }
            else
            {
                record.MarkAsFailed(result.Error ?? "Unknown error");
                throw new Exception($"SMS delivery failed: {result.Error}");
            }
        }
        finally
        {
            await _recordRepo.AddRecordAsync(record, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

    private async Task ProcessPushAsync(
        NotificationRequestedEvent message,
        NotificationTemplate template,
        CancellationToken ct)
    {
        const string channel = "Push";

        if (string.IsNullOrEmpty(template.PushBody))
        {
            _logger.LogDebug("Template has no Push body: {TemplateKey}", message.TemplateKey);
            return;
        }

        if (await _idempotency.IsProcessedAsync(message.EventId, channel, ct))
            return;

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(message.EventId, channel, ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(message.EventId, channel, ct))
            return;

        var pushTitle = RenderTemplate(template.PushTitle ?? template.Subject, message.Data);
        var pushBody = RenderTemplate(template.PushBody, message.Data);

        var record = NotificationRecord.Create(
            Guid.TryParse(message.UserId, out var uid) ? uid : Guid.Empty,
            message.TemplateKey,
            channel,
            pushTitle);

        try
        {
            var result = await _pushSender.SendAsync(message.UserId, pushTitle, pushBody, message.Data, ct);

            if (result.Success)
            {
                record.MarkAsSent(result.MessageId);
                await _idempotency.MarkAsProcessedAsync(message.EventId, channel, result.MessageId, ct: ct);
            }
            else
            {
                record.MarkAsFailed(result.Error ?? "Unknown error");
                throw new Exception($"Push delivery failed: {result.Error}");
            }
        }
        finally
        {
            await _recordRepo.AddRecordAsync(record, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

    private async Task ProcessInAppAsync(
        NotificationRequestedEvent message,
        NotificationTemplate template,
        CancellationToken ct)
    {
        const string channel = "InApp";

        if (await _idempotency.IsProcessedAsync(message.EventId, channel, ct))
            return;

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(message.EventId, channel, ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(message.EventId, channel, ct))
            return;

        var title = RenderTemplate(template.Subject, message.Data);
        var body = RenderTemplate(template.Body, message.Data);

        var notification = UserNotification.Create(
            message.UserId,
            title,
            StripHtml(body),
            message.InAppLink);

        await _recordRepo.AddUserNotificationAsync(notification, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _idempotency.MarkAsProcessedAsync(message.EventId, channel, notification.Id.ToString(), ct: ct);

        try
        {
            var dto = new Application.DTOs.NotificationDto
            {
                Id = notification.Id,
                UserId = message.UserId,
                Title = title,
                Message = StripHtml(body),
                Status = "Unread",
                CreatedAt = notification.CreatedAt
            };
            await _hubService.SendNotificationToUserAsync(message.UserId, dto);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send real-time notification");
        }
    }

    private static string RenderTemplate(string template, Dictionary<string, string> data)
        => TemplateHelper.RenderTemplate(template, data);

    private static string StripHtml(string html)
        => TemplateHelper.StripHtml(html);
}
