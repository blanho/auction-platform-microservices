using IdentityService.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using IUnitOfWork = Notification.Application.Interfaces.IUnitOfWork;

namespace Notification.Infrastructure.Consumers;

public class SecurityAlertConsumer : IConsumer<SecurityAlertEvent>
{
    private readonly IIdempotencyService _idempotency;
    private readonly ITemplateRepository _templateRepo;
    private readonly INotificationRecordRepository _recordRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<SecurityAlertConsumer> _logger;

    public SecurityAlertConsumer(
        IIdempotencyService idempotency,
        ITemplateRepository templateRepo,
        INotificationRecordRepository recordRepo,
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        ILogger<SecurityAlertConsumer> logger)
    {
        _idempotency = idempotency;
        _templateRepo = templateRepo;
        _recordRepo = recordRepo;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SecurityAlertEvent> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"security-alert-{message.UserId}-{message.AlertType}-{message.OccurredAt:yyyyMMddHHmmss}";

        _logger.LogWarning(
            "Processing SecurityAlertEvent: UserId={UserId}, AlertType={AlertType}, IP={IpAddress}",
            message.UserId,
            message.AlertType,
            message.IpAddress ?? "unknown");

        if (await _idempotency.IsProcessedAsync(eventId, "Email", ct))
            return;

        var templateKey = GetTemplateKey(message.AlertType);
        var template = await _templateRepo.GetByKeyAsync(templateKey, ct);
        if (template == null || !template.IsActive)
        {
            template = await _templateRepo.GetByKeyAsync("security-alert-general", ct);
        }

        if (template == null || !template.IsActive)
        {
            _logger.LogWarning("No security alert template found - skipping notification");
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "Email", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "Email", ct))
            return;

        var data = new Dictionary<string, string>
        {
            ["username"] = message.Username,
            ["alertType"] = message.AlertType,
            ["description"] = message.Description,
            ["ipAddress"] = message.IpAddress ?? "Unknown",
            ["occurredAt"] = message.OccurredAt.ToString("f")
        };

        var subject = GetSubject(message.AlertType);
        var body = TemplateHelper.RenderTemplate(template.Body, data);

        var record = Notification.Domain.Entities.NotificationRecord.Create(
            Guid.TryParse(message.UserId, out var uid) ? uid : Guid.Empty,
            templateKey,
            "Email",
            subject,
            message.Email);

        try
        {
            var result = await _emailSender.SendAsync(message.Email, subject, body, TemplateHelper.StripHtml(body), ct);

            if (result.Success)
            {
                record.MarkAsSent(result.MessageId);
                await _idempotency.MarkAsProcessedAsync(eventId, "Email", result.MessageId, ct: ct);
                _logger.LogInformation("Security alert email sent to {Email} for {AlertType}", message.Email, message.AlertType);
            }
            else
            {
                record.MarkAsFailed(result.Error ?? "Unknown error");
                throw new Exception($"Email delivery failed: {result.Error}");
            }
        }
        finally
        {
            await _recordRepo.AddRecordAsync(record, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

    private static string GetTemplateKey(string alertType) => alertType switch
    {
        SecurityAlertTypes.TokenTheftDetected => "security-alert-token-theft",
        SecurityAlertTypes.SuspiciousLogin => "security-alert-suspicious-login",
        SecurityAlertTypes.MultipleFailedLogins => "security-alert-failed-logins",
        SecurityAlertTypes.SessionsRevoked => "security-alert-sessions-revoked",
        _ => "security-alert-general"
    };

    private static string GetSubject(string alertType) => alertType switch
    {
        SecurityAlertTypes.TokenTheftDetected => "Security Alert: Suspicious Activity Detected on Your Account",
        SecurityAlertTypes.SuspiciousLogin => "Security Alert: New Login from Unrecognized Device",
        SecurityAlertTypes.MultipleFailedLogins => "Security Alert: Multiple Failed Login Attempts",
        SecurityAlertTypes.SessionsRevoked => "Security Alert: All Sessions Have Been Logged Out",
        _ => "Security Alert: Important Account Activity"
    };
}
