using Notification.Domain.Entities;
using IdentityService.Contracts.Events;
using Microsoft.Extensions.Logging;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;

namespace Notification.Infrastructure.Messaging.Consumers;

public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly IIdempotencyService _idempotency;
    private readonly ITemplateRepository _templateRepo;
    private readonly INotificationRecordRepository _recordRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<UserCreatedConsumer> _logger;

    public UserCreatedConsumer(
        IIdempotencyService idempotency,
        ITemplateRepository templateRepo,
        INotificationRecordRepository recordRepo,
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        ILogger<UserCreatedConsumer> logger)
    {
        _idempotency = idempotency;
        _templateRepo = templateRepo;
        _recordRepo = recordRepo;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        _logger.LogInformation(
            "Processing UserCreatedEvent: UserId={UserId}, Username={Username}, EmailConfirmed={EmailConfirmed}",
            message.UserId,
            message.Username,
            message.EmailConfirmed);

        string templateKey;
        string subject;
        Dictionary<string, string> data;
        string eventId;

        if (message.EmailConfirmed)
        {
            templateKey = NotificationTemplateKeys.Welcome;
            subject = "Welcome to Auction Platform";
            eventId = $"user-created-welcome-{message.UserId}";
            data = new Dictionary<string, string>
            {
                [NotificationTemplateDataKeys.Username] = message.Username,
                [NotificationTemplateDataKeys.FullName] = message.FullName ?? message.Username
            };
        }
        else if (!string.IsNullOrEmpty(message.ConfirmationLink))
        {
            templateKey = NotificationTemplateKeys.EmailConfirmation;
            subject = "Confirm Your Email";
            eventId = $"user-created-confirmation-{message.UserId}";
            data = new Dictionary<string, string>
            {
                [NotificationTemplateDataKeys.Username] = message.Username,
                [NotificationTemplateDataKeys.ConfirmationLink] = message.ConfirmationLink
            };
        }
        else
        {
            _logger.LogWarning("UserCreatedEvent has no confirmation link and email not confirmed");
            return;
        }

        if (await _idempotency.IsProcessedAsync(eventId, NotificationChannelNames.Email, ct))
        {
            _logger.LogDebug("Email already sent for EventId={EventId}", eventId);
            return;
        }

        var template = await _templateRepo.GetByKeyAsync(templateKey, ct);
        if (template == null || !template.IsActive)
        {
            _logger.LogWarning("Template not found or inactive: {TemplateKey}", templateKey);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, NotificationChannelNames.Email, ct: ct);
        if (lockHandle == null)
        {
            _logger.LogDebug("Could not acquire lock for email, EventId={EventId}", eventId);
            return;
        }

        if (await _idempotency.IsProcessedAsync(eventId, NotificationChannelNames.Email, ct))
            return;

        var renderedSubject = TemplateHelper.RenderTemplate(template.Subject ?? subject, data);
        var body = TemplateHelper.RenderTemplate(template.Body, data);

        var record = NotificationRecord.Create(
            Guid.TryParse(message.UserId, out var uid) ? uid : Guid.Empty,
            templateKey,
            NotificationChannelNames.Email,
            renderedSubject,
            message.Email);

        try
        {
            var result = await _emailSender.SendAsync(
                message.Email,
                renderedSubject,
                body,
                TemplateHelper.StripHtml(body),
                ct);

            if (result.Success)
            {
                record.MarkAsSent(result.MessageId);
                await _idempotency.MarkAsProcessedAsync(eventId, NotificationChannelNames.Email, result.MessageId, ct: ct);
                _logger.LogInformation(
                    "Email sent for UserCreatedEvent: UserId={UserId}, MessageId={MessageId}",
                    message.UserId,
                    result.MessageId);
            }
            else
            {
                record.MarkAsFailed(result.Error ?? NotificationDefaults.Fallback.UnknownError);
                _logger.LogWarning(
                    "Email failed for UserCreatedEvent: UserId={UserId}, Error={Error}",
                    message.UserId,
                    result.Error);
                throw new InvalidOperationException($"Email delivery failed: {result.Error}");
            }
        }
        finally
        {
            await _recordRepo.AddRecordAsync(record, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
