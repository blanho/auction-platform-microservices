using Notification.Application.Interfaces;

namespace Notification.Application.Services;

public sealed class NotificationDelivery(INotificationRecordRepository records, IUnitOfWork unitOfWork,
    IEmailSender email, ISmsSender sms, IPushSender push, ILogger<NotificationDelivery> logger) : INotificationDelivery
{
    private const int SmsSubjectPreviewLength = 50;

    public async Task SendEmailAsync(string userId, string templateKey, string recipientEmail, string subject,
        string body, CancellationToken ct = default)
    {
        var record = NotificationRecord.Create(Guid.TryParse(userId, out var id) ? id : Guid.Empty,
            templateKey, NotificationChannelNames.Email, subject, recipientEmail);
        try
        {
            var result = await email.SendAsync(recipientEmail, subject, body, TemplateHelper.StripHtml(body), ct);
            if (result.Success)
            {
                record.MarkAsSent(result.MessageId);
                logger.LogInformation("Email sent to {Email} for template {Template}", recipientEmail, templateKey);
            }
            else
            {
                record.MarkAsFailed(result.Error ?? NotificationDefaults.Fallback.UnknownError);
                logger.LogWarning("Failed to send email to {Email}: {Error}", recipientEmail, result.Error);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            record.MarkAsFailed(ex.Message);
            logger.LogError(ex, "Exception sending email to {Email}", recipientEmail);
        }
        await PersistAsync(record, ct);
    }

    public async Task SendSmsAsync(string userId, string templateKey, string phoneNumber, string message,
        CancellationToken ct = default)
    {
        var preview = message.Length > SmsSubjectPreviewLength ? message[..SmsSubjectPreviewLength] + "..." : message;
        var record = NotificationRecord.Create(Guid.TryParse(userId, out var id) ? id : Guid.Empty,
            templateKey, NotificationChannelNames.Sms, preview, phoneNumber);
        try
        {
            var result = await sms.SendAsync(phoneNumber, message, ct);
            if (result.Success)
            {
                record.MarkAsSent(result.MessageId);
                logger.LogInformation("SMS sent to {Phone} for template {Template}", TemplateHelper.MaskPhone(phoneNumber), templateKey);
            }
            else
            {
                record.MarkAsFailed(result.Error ?? NotificationDefaults.Fallback.UnknownError);
                logger.LogWarning("Failed to send SMS to {Phone}: {Error}", TemplateHelper.MaskPhone(phoneNumber), result.Error);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            record.MarkAsFailed(ex.Message);
            logger.LogError(ex, "Exception sending SMS to {Phone}", TemplateHelper.MaskPhone(phoneNumber));
        }
        await PersistAsync(record, ct);
    }

    public async Task SendPushAsync(string userId, string templateKey, string title, string body,
        Dictionary<string, string> data, CancellationToken ct = default)
    {
        var record = NotificationRecord.Create(Guid.TryParse(userId, out var id) ? id : Guid.Empty,
            templateKey, NotificationChannelNames.Push, title, userId);
        try
        {
            var result = await push.SendAsync(userId, title, body, data, ct);
            if (result.Success)
            {
                record.MarkAsSent(result.MessageId);
                logger.LogInformation("Push notification sent to user {UserId} for template {Template}", userId, templateKey);
            }
            else
            {
                record.MarkAsFailed(result.Error ?? NotificationDefaults.Fallback.UnknownError);
                logger.LogWarning("Failed to send push to user {UserId}: {Error}", userId, result.Error);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            record.MarkAsFailed(ex.Message);
            logger.LogError(ex, "Exception sending push to user {UserId}", userId);
        }
        await PersistAsync(record, ct);
    }

    private async Task PersistAsync(NotificationRecord record, CancellationToken ct)
    {
        await records.AddRecordAsync(record, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
