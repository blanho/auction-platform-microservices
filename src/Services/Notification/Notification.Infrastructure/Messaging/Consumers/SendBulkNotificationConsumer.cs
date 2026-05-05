using System.Diagnostics;
using System.Text.Json;
using JobService.Contracts.Commands;
using JobService.Contracts.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using NotificationService.Contracts.Commands;
using NotificationService.Contracts.Enums;
using NotificationService.Contracts.Events;
using IUnitOfWork = Notification.Application.Interfaces.IUnitOfWork;

namespace Notification.Infrastructure.Messaging.Consumers;

public class SendBulkNotificationConsumer : IConsumer<SendBulkNotificationCommand>
{
    private readonly IIdempotencyService _idempotency;
    private readonly ITemplateRepository _templateRepo;
    private readonly INotificationRecordRepository _recordRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly IPushSender _pushSender;
    private readonly INotificationHubService _hubService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SendBulkNotificationConsumer> _logger;

    public SendBulkNotificationConsumer(
        IIdempotencyService idempotency,
        ITemplateRepository templateRepo,
        INotificationRecordRepository recordRepo,
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        ISmsSender smsSender,
        IPushSender pushSender,
        INotificationHubService hubService,
        IPublishEndpoint publishEndpoint,
        ILogger<SendBulkNotificationConsumer> logger)
    {
        _idempotency = idempotency;
        _templateRepo = templateRepo;
        _recordRepo = recordRepo;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _pushSender = pushSender;
        _hubService = hubService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendBulkNotificationCommand> context)
    {
        var message = context.Message;
        var stopwatch = Stopwatch.StartNew();
        var correlationId = message.CorrelationId.ToString();

        _logger.LogInformation(
            "Processing bulk notification {CorrelationId} - Template: {TemplateKey}, Recipients: {RecipientCount}",
            correlationId, message.TemplateKey, message.Recipients.Count);

        await PublishJobStarted(context, message);

        var template = await _templateRepo.GetByKeyAsync(message.TemplateKey, context.CancellationToken);

        int successCount = 0;
        int failureCount = 0;
        int processedCount = 0;

        var batches = message.Recipients
            .Select((recipient, index) => new { recipient, index })
            .GroupBy(x => x.index / message.BatchSize)
            .Select(g => g.Select(x => x.recipient).ToList())
            .ToList();

        foreach (var batch in batches)
        {
            var batchTasks = batch.Select(async recipient =>
            {
                try
                {
                    await ProcessRecipientAsync(
                        message,
                        recipient,
                        template,
                        context.CancellationToken);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to send notification to {UserId} in bulk job {CorrelationId}",
                        recipient.UserId, correlationId);
                    return false;
                }
            });

            var results = await Task.WhenAll(batchTasks);
            successCount += results.Count(r => r);
            failureCount += results.Count(r => !r);
            processedCount += batch.Count;

            await _publishEndpoint.Publish(new ReportJobBatchProgressCommand
            {
                CorrelationId = correlationId,
                CompletedCount = successCount,
                FailedCount = failureCount
            }, context.CancellationToken);

            _logger.LogInformation(
                "Bulk notification progress {CorrelationId}: {ProcessedCount}/{TotalCount}",
                correlationId, processedCount, message.Recipients.Count);
        }

        stopwatch.Stop();

        await _publishEndpoint.Publish(new BulkNotificationCompletedEvent
        {
            CorrelationId = message.CorrelationId,
            RequestedBy = message.RequestedBy,
            TemplateKey = message.TemplateKey,
            TotalRecipients = message.Recipients.Count,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Duration = stopwatch.Elapsed,
            CompletedAt = DateTimeOffset.UtcNow
        }, context.CancellationToken);

        _logger.LogInformation(
            "Bulk notification completed {CorrelationId} - Success: {SuccessCount}, Failed: {FailureCount}, Duration: {Duration}ms",
            correlationId, successCount, failureCount, stopwatch.ElapsedMilliseconds);
    }

    private async Task ProcessRecipientAsync(
        SendBulkNotificationCommand message,
        BulkNotificationRecipient recipient,
        NotificationTemplate? template,
        CancellationToken cancellationToken)
    {
        var idempotencyKey = $"{message.CorrelationId}:{recipient.UserId}";

        if (await _idempotency.IsProcessedAsync(idempotencyKey, "bulk", cancellationToken))
        {
            _logger.LogDebug(
                "Notification already sent for {UserId} in bulk job {CorrelationId}",
                recipient.UserId, message.CorrelationId);
            return;
        }

        var parameters = new Dictionary<string, string>(message.GlobalParameters);
        foreach (var param in recipient.Parameters)
        {
            parameters[param.Key] = param.Value;
        }

        var tasks = new List<Task>();

        if (message.Channels.Contains("Email") && !string.IsNullOrEmpty(recipient.Email))
        {
            tasks.Add(SendEmailAsync(message, recipient, template, parameters, cancellationToken));
        }

        if (message.Channels.Contains("Sms") && !string.IsNullOrEmpty(recipient.PhoneNumber))
        {
            tasks.Add(SendSmsAsync(message, recipient, template, parameters, cancellationToken));
        }

        if (message.Channels.Contains("Push"))
        {
            tasks.Add(SendPushAsync(message, recipient, template, parameters, cancellationToken));
        }

        if (message.Channels.Contains("InApp"))
        {
            tasks.Add(SendInAppAsync(message, recipient, parameters, cancellationToken));
        }

        await Task.WhenAll(tasks);
        await _idempotency.MarkAsProcessedAsync(idempotencyKey, "bulk", message.CorrelationId.ToString(), ct: cancellationToken);
    }

    private async Task SendEmailAsync(
        SendBulkNotificationCommand message,
        BulkNotificationRecipient recipient,
        NotificationTemplate? template,
        Dictionary<string, string> parameters,
        CancellationToken cancellationToken)
    {
        var subject = RenderTemplate(template?.Subject ?? message.Title, parameters);
        var body = RenderTemplate(template?.Body ?? message.Message, parameters);

        var record = NotificationRecord.Create(
            recipient.UserId,
            message.TemplateKey,
            "Email",
            subject,
            recipient.Email);

        var result = await _emailSender.SendAsync(
            recipient.Email,
            subject,
            body,
            StripHtml(body),
            cancellationToken);

        ApplySendResult(record, result.Success, result.MessageId, result.Error);
        await PersistNotificationRecordAsync(record, cancellationToken);
    }

    private async Task SendSmsAsync(
        SendBulkNotificationCommand message,
        BulkNotificationRecipient recipient,
        NotificationTemplate? template,
        Dictionary<string, string> parameters,
        CancellationToken cancellationToken)
    {
        var body = RenderTemplate(template?.SmsBody ?? message.Message, parameters);

        var record = NotificationRecord.Create(
            recipient.UserId,
            message.TemplateKey,
            "Sms",
            message.Title,
            recipient.PhoneNumber ?? string.Empty);

        var result = await _smsSender.SendAsync(
            recipient.PhoneNumber!,
            body,
            cancellationToken);

        ApplySendResult(record, result.Success, result.MessageId, result.Error);
        await PersistNotificationRecordAsync(record, cancellationToken);
    }

    private async Task SendPushAsync(
        SendBulkNotificationCommand message,
        BulkNotificationRecipient recipient,
        NotificationTemplate? template,
        Dictionary<string, string> parameters,
        CancellationToken cancellationToken)
    {
        var title = RenderTemplate(template?.Subject ?? message.Title, parameters);
        var body = RenderTemplate(template?.PushBody ?? message.Message, parameters);

        var record = NotificationRecord.Create(
            recipient.UserId,
            message.TemplateKey,
            "Push",
            title,
            recipient.UserId.ToString());

        var result = await _pushSender.SendAsync(
            recipient.UserId.ToString(),
            title,
            body,
            null,
            cancellationToken);

        ApplySendResult(record, result.Success, result.MessageId, result.Error);
        await PersistNotificationRecordAsync(record, cancellationToken);
    }

    private async Task SendInAppAsync(
        SendBulkNotificationCommand message,
        BulkNotificationRecipient recipient,
        Dictionary<string, string> parameters,
        CancellationToken cancellationToken)
    {
        var title = RenderTemplate(message.Title, parameters);
        var body = RenderTemplate(message.Message, parameters);

        var record = NotificationRecord.Create(
            recipient.UserId,
            message.TemplateKey,
            "InApp",
            title,
            recipient.UserId.ToString());

        try
        {
            var notification = new NotificationDto
            {
                Id = Guid.NewGuid(),
                UserId = recipient.UserId.ToString(),
                Type = "BulkNotification",
                Title = title,
                Message = body,
                Status = "Unread",
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _hubService.SendNotificationToUserAsync(
                recipient.UserId.ToString(),
                notification);

            record.MarkAsSent("inapp");
        }
        catch (Exception ex)
        {
            record.MarkAsFailed(ex.Message);
        }

        await PersistNotificationRecordAsync(record, cancellationToken);
    }

    private async Task PublishJobStarted(
        ConsumeContext<SendBulkNotificationCommand> context,
        SendBulkNotificationCommand message)
    {
        await context.Publish(new RequestJobCommand
        {
            JobType = nameof(JobType.BulkNotification),
            CorrelationId = message.CorrelationId.ToString(),
            RequestedBy = message.RequestedBy,
            PayloadJson = JsonSerializer.Serialize(new
            {
                message.TemplateKey,
                message.Title,
                RecipientCount = message.Recipients.Count,
                message.Channels,
                message.ScheduledAt
            }),
            TotalItems = message.Recipients.Count,
            MaxRetryCount = 0
        });
    }

    private static void ApplySendResult(NotificationRecord record, bool success, string? messageId, string? error)
    {
        if (success)
            record.MarkAsSent(messageId);
        else
            record.MarkAsFailed(error ?? "Unknown error");
    }

    private async Task PersistNotificationRecordAsync(NotificationRecord record, CancellationToken cancellationToken)
    {
        await _recordRepo.AddRecordAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string RenderTemplate(string? template, Dictionary<string, string> parameters)
    {
        if (string.IsNullOrEmpty(template))
            return string.Empty;

        var result = template;
        foreach (var param in parameters)
        {
            result = result.Replace($"{{{{{param.Key}}}}}", param.Value);
            result = result.Replace($"{{{param.Key}}}", param.Value);
        }
        return result;
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", string.Empty);
    }
}
