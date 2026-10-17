using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Text.Json;
using JobService.Contracts.Commands;
using JobService.Contracts.Enums;
using Microsoft.Extensions.Logging;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using NotificationService.Contracts.Commands;
using NotificationService.Contracts.Enums;
using NotificationService.Contracts.Events;

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
    private readonly SemaphoreSlim _persistenceLock = new(1, 1);

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
            correlationId, message.TemplateKey, message.Recipients?.Count ?? 0);

        if (message.ParentJobId.HasValue)
        {
            var validationError = ValidateJobItem(message);
            if (validationError is not null)
            {
                await PublishItemResult(context, validationError);
                return;
            }
        }
        else
            await PublishJobStarted(context, message);

        var template = await _templateRepo.GetByKeyAsync(message.TemplateKey, context.CancellationToken);

        int successCount = 0;
        int failureCount = 0;
        int processedCount = 0;

        var recipients = message.Recipients ?? throw new ArgumentException("Recipients are required.");
        var batches = recipients
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
                    return await ProcessRecipientAsync(
                        message,
                        recipient,
                        template,
                        context.CancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to send notification to {UserId} in bulk job {CorrelationId}",
                        recipient.UserId, correlationId);
                    return false;
                }
            });

            var results = await Task.WhenAll(batchTasks);
            context.CancellationToken.ThrowIfCancellationRequested();

            var batchSuccessCount = results.Count(result => result);
            var batchFailureCount = results.Length - batchSuccessCount;
            successCount += batchSuccessCount;
            failureCount += batchFailureCount;
            processedCount += batch.Count;

            if (!message.ParentJobId.HasValue)
                await _publishEndpoint.Publish(new ReportJobBatchProgressCommand
                {
                    CorrelationId = correlationId,
                    BatchId = $"notification:{processedCount}",
                    CompletedCount = batchSuccessCount,
                    FailedCount = batchFailureCount
                }, context.CancellationToken);

            _logger.LogInformation(
                "Bulk notification progress {CorrelationId}: {ProcessedCount}/{TotalCount}",
                correlationId, processedCount, message.Recipients.Count);
        }

        stopwatch.Stop();

        if (message.ParentJobId.HasValue)
            await PublishItemResult(context, failureCount == 0 ? null :
                $"Delivery failed for {failureCount} of {message.Recipients.Count} recipients.");

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

    private async Task<bool> ProcessRecipientAsync(
        SendBulkNotificationCommand message,
        BulkNotificationRecipient recipient,
        NotificationTemplate? template,
        CancellationToken cancellationToken)
    {
        // An operator retry changes the attempt correlation, but successful deliveries
        // belong to the same persisted job item and must not be sent again.
        var operationId = message.ParentJobItemId ?? message.CorrelationId;
        var idempotencyKey = $"{operationId}:{recipient.UserId}";

        if (await _idempotency.IsProcessedAsync(idempotencyKey, "bulk", cancellationToken))
        {
            _logger.LogDebug(
                "Notification already sent for {UserId} in bulk job {CorrelationId}",
                recipient.UserId, message.CorrelationId);
            return true;
        }

        var parameters = new Dictionary<string, string>(message.GlobalParameters);
        foreach (var param in recipient.Parameters)
        {
            parameters[param.Key] = param.Value;
        }

        var tasks = new List<Task<bool>>();

        if (message.Channels.Contains(NotificationChannelNames.Email) && !string.IsNullOrEmpty(recipient.Email))
        {
            tasks.Add(SendChannelAsync(NotificationChannelNames.Email,
                () => SendEmailAsync(message, recipient, template, parameters, cancellationToken)));
        }

        if (message.Channels.Contains(NotificationChannelNames.Sms) && !string.IsNullOrEmpty(recipient.PhoneNumber))
        {
            tasks.Add(SendChannelAsync(NotificationChannelNames.Sms,
                () => SendSmsAsync(message, recipient, template, parameters, cancellationToken)));
        }

        if (message.Channels.Contains(NotificationChannelNames.Push))
        {
            tasks.Add(SendChannelAsync(NotificationChannelNames.Push,
                () => SendPushAsync(message, recipient, template, parameters, cancellationToken)));
        }

        if (message.Channels.Contains(NotificationChannelNames.InApp))
        {
            tasks.Add(SendChannelAsync(NotificationChannelNames.InApp,
                () => SendInAppAsync(message, recipient, parameters, cancellationToken)));
        }

        var results = await Task.WhenAll(tasks);
        var succeeded = results.All(success => success);
        if (succeeded)
            await _idempotency.MarkAsProcessedAsync(idempotencyKey, "bulk", operationId.ToString(), ct: cancellationToken);
        return succeeded;

        async Task<bool> SendChannelAsync(string channel, Func<Task<bool>> send)
        {
            if (await _idempotency.IsProcessedAsync(idempotencyKey, channel, cancellationToken))
                return true;

            var sent = await send();
            if (sent)
                await _idempotency.MarkAsProcessedAsync(idempotencyKey, channel, operationId.ToString(), ct: cancellationToken);
            return sent;
        }
    }

    private static string? ValidateJobItem(SendBulkNotificationCommand message)
    {
        if (!message.ParentJobItemId.HasValue || message.ParentJobItemId == Guid.Empty ||
            message.ParentJobId == Guid.Empty || message.RequestedBy == Guid.Empty)
            return "A job item and requester are required.";
        if (string.IsNullOrWhiteSpace(message.TemplateKey) || string.IsNullOrWhiteSpace(message.Title) ||
            string.IsNullOrWhiteSpace(message.Message) || message.GlobalParameters is null)
            return "TemplateKey, Title, Message, and GlobalParameters are required.";
        if (message.BatchSize < NotificationDefaults.Bulk.MinBatchSize ||
            message.BatchSize > NotificationDefaults.Bulk.MaxBatchSize)
            return $"BatchSize must be between {NotificationDefaults.Bulk.MinBatchSize} and {NotificationDefaults.Bulk.MaxBatchSize}.";
        if (message.Recipients is null || message.Recipients.Count == 0 ||
            message.Recipients.Count > NotificationDefaults.Bulk.MaxRecipients ||
            message.Recipients.Any(r => r is null || r.UserId == Guid.Empty || r.Parameters is null))
            return "A nonempty, bounded recipient list with valid user IDs is required.";
        if (message.Recipients.Select(r => r.UserId).Distinct().Count() != message.Recipients.Count)
            return "Recipient user IDs must be unique within a job item.";
        if (message.Channels is null || message.Channels.Count == 0 || message.Channels.Any(c =>
                c is not (NotificationChannelNames.Email or NotificationChannelNames.Sms or
                    NotificationChannelNames.Push or NotificationChannelNames.InApp)))
            return "Specify supported notification channels.";
        if (message.Channels.Contains(NotificationChannelNames.Email) &&
            message.Recipients.Any(r => string.IsNullOrWhiteSpace(r.Email)))
            return "Every email recipient must have an email address.";
        if (message.Channels.Contains(NotificationChannelNames.Sms) &&
            message.Recipients.Any(r => string.IsNullOrWhiteSpace(r.PhoneNumber)))
            return "Every SMS recipient must have a phone number.";
        if (message.ScheduledAt.HasValue)
            return "ScheduledAt is not supported for Jobs-dispatched notifications; submit the job when it is due.";
        return null;
    }

    private static Task PublishItemResult(ConsumeContext<SendBulkNotificationCommand> context, string? error)
    {
        var message = context.Message;
        return context.Publish(new ReportJobItemResultCommand
        {
            JobId = message.ParentJobId!.Value,
            JobItemId = message.ParentJobItemId.GetValueOrDefault(),
            Attempt = message.Attempt,
            IsSuccess = error is null,
            IsFinalFailure = error is not null,
            ErrorMessage = error
        }, context.CancellationToken);
    }

    private async Task<bool> SendEmailAsync(
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
            NotificationChannelNames.Email,
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
        return result.Success;
    }

    private async Task<bool> SendSmsAsync(
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
            NotificationChannelNames.Sms,
            message.Title,
            recipient.PhoneNumber ?? string.Empty);

        var result = await _smsSender.SendAsync(
            recipient.PhoneNumber!,
            body,
            cancellationToken);

        ApplySendResult(record, result.Success, result.MessageId, result.Error);
        await PersistNotificationRecordAsync(record, cancellationToken);
        return result.Success;
    }

    private async Task<bool> SendPushAsync(
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
            NotificationChannelNames.Push,
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
        return result.Success;
    }

    private async Task<bool> SendInAppAsync(
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
            NotificationChannelNames.InApp,
            title,
            recipient.UserId.ToString());

        var sent = false;
        try
        {
            var notification = new NotificationDto
            {
                Id = Guid.NewGuid(),
                UserId = recipient.UserId.ToString(),
                Type = NotificationDefaults.PayloadType.BulkNotification,
                Title = title,
                Message = body,
                Status = nameof(NotificationStatus.Unread),
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _hubService.SendNotificationToUserAsync(
                recipient.UserId.ToString(),
                notification);

            record.MarkAsSent("inapp");
            sent = true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            record.MarkAsFailed(ex.Message);
        }

        await PersistNotificationRecordAsync(record, cancellationToken);
        return sent;
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
            record.MarkAsFailed(error ?? NotificationDefaults.Fallback.UnknownError);
    }

    private async Task PersistNotificationRecordAsync(NotificationRecord record, CancellationToken cancellationToken)
    {
        await _persistenceLock.WaitAsync(cancellationToken);
        try
        {
            await _recordRepo.AddRecordAsync(record, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _persistenceLock.Release();
        }
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

        return Regex.Replace(html, "<[^>]*>", string.Empty);
    }
}
