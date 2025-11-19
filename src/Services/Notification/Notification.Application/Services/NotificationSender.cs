using Microsoft.Extensions.Logging;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;

namespace Notification.Application.Services;

public class NotificationSender : INotificationSender
{
    private readonly ITemplateRepository _templateRepo;
    private readonly INotificationRecordRepository _notificationRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly IPushSender _pushSender;
    private readonly INotificationHubService _hubService;
    private readonly ILogger<NotificationSender> _logger;

    public NotificationSender(
        ITemplateRepository templateRepo,
        INotificationRecordRepository notificationRepo,
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        ISmsSender smsSender,
        IPushSender pushSender,
        INotificationHubService hubService,
        ILogger<NotificationSender> logger)
    {
        _templateRepo = templateRepo;
        _notificationRepo = notificationRepo;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _pushSender = pushSender;
        _hubService = hubService;
        _logger = logger;
    }

    public async Task SendAsync(SendNotificationRequest request, CancellationToken ct = default)
    {
        var template = await _templateRepo.GetByKeyAsync(request.TemplateKey, ct);
        if (template == null)
        {
            _logger.LogWarning("Template '{Key}' not found", request.TemplateKey);
            return;
        }

        if (!template.IsActive)
        {
            _logger.LogWarning("Template '{Key}' is inactive", request.TemplateKey);
            return;
        }

        var subject = RenderTemplate(template.Subject, request.Data);
        var body = RenderTemplate(template.Body, request.Data);

        if (request.SendEmail && !string.IsNullOrEmpty(request.RecipientEmail))
        {
            await SendEmailInternalAsync(request.UserId, request.TemplateKey, request.RecipientEmail, subject, body, ct);
        }

        if (request.SendSms && !string.IsNullOrEmpty(request.RecipientPhone) && !string.IsNullOrEmpty(template.SmsBody))
        {
            var smsBody = RenderTemplate(template.SmsBody, request.Data);
            await SendSmsInternalAsync(request.UserId, request.TemplateKey, request.RecipientPhone, smsBody, ct);
        }

        if (request.SendPush && !string.IsNullOrEmpty(template.PushBody))
        {
            var pushTitle = RenderTemplate(template.PushTitle ?? subject, request.Data);
            var pushBody = RenderTemplate(template.PushBody, request.Data);
            await SendPushInternalAsync(request.UserId, request.TemplateKey, pushTitle, pushBody, request.Data, ct);
        }

        if (request.SendInApp)
        {
            await SendInAppAsync(
                request.UserId,
                subject,
                StripHtml(body),
                request.InAppLink,
                ct);
        }
    }

    public async Task SendEmailAsync(string userId, string templateKey, Dictionary<string, string> data, string recipientEmail, CancellationToken ct = default)
    {
        var template = await _templateRepo.GetByKeyAsync(templateKey, ct);
        if (template == null || !template.IsActive)
        {
            _logger.LogWarning("Template '{Key}' not found or inactive", templateKey);
            return;
        }

        var subject = RenderTemplate(template.Subject, data);
        var body = RenderTemplate(template.Body, data);
        await SendEmailInternalAsync(userId, templateKey, recipientEmail, subject, body, ct);
    }

    public async Task SendSmsAsync(string userId, string templateKey, Dictionary<string, string> data, string phoneNumber, CancellationToken ct = default)
    {
        var template = await _templateRepo.GetByKeyAsync(templateKey, ct);
        if (template == null || !template.IsActive || string.IsNullOrEmpty(template.SmsBody))
        {
            _logger.LogWarning("Template '{Key}' not found, inactive, or has no SMS body", templateKey);
            return;
        }

        var smsBody = RenderTemplate(template.SmsBody, data);
        await SendSmsInternalAsync(userId, templateKey, phoneNumber, smsBody, ct);
    }

    public async Task SendPushAsync(string userId, string templateKey, Dictionary<string, string> data, CancellationToken ct = default)
    {
        var template = await _templateRepo.GetByKeyAsync(templateKey, ct);
        if (template == null || !template.IsActive || string.IsNullOrEmpty(template.PushBody))
        {
            _logger.LogWarning("Template '{Key}' not found, inactive, or has no push body", templateKey);
            return;
        }

        var pushTitle = RenderTemplate(template.PushTitle ?? template.Subject, data);
        var pushBody = RenderTemplate(template.PushBody, data);
        await SendPushInternalAsync(userId, templateKey, pushTitle, pushBody, data, ct);
    }

    public async Task SendInAppAsync(string userId, string title, string message, string? link = null, CancellationToken ct = default)
    {
        var notification = UserNotification.Create(userId, title, message, link);
        await _notificationRepo.AddUserNotificationAsync(notification, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        try
        {
            var dto = new NotificationDto
            {
                Id = notification.Id,
                UserId = userId,
                Title = title,
                Message = message,
                Status = "Unread",
                CreatedAt = notification.CreatedAt
            };
            await _hubService.SendNotificationToUserAsync(userId, dto);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send real-time notification to user {UserId}", userId);
        }
    }

    public async Task<List<UserNotification>> GetUserNotificationsAsync(string userId, int skip = 0, int take = 20, CancellationToken ct = default)
    {
        return await _notificationRepo.GetUserNotificationsAsync(userId, skip, take, ct);
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default)
    {
        return await _notificationRepo.GetUnreadCountAsync(userId, ct);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken ct = default)
    {
        await _notificationRepo.MarkAsReadAsync(notificationId, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken ct = default)
    {
        await _notificationRepo.MarkAllAsReadAsync(userId, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    #region Private Methods

    private async Task SendEmailInternalAsync(string userId, string templateKey, string recipientEmail, string subject, string body, CancellationToken ct)
    {
        var record = NotificationRecord.Create(
            Guid.TryParse(userId, out var uid) ? uid : Guid.Empty,
            templateKey,
            "Email",
            subject,
            recipientEmail);

        try
        {
            var result = await _emailSender.SendAsync(recipientEmail, subject, body, StripHtml(body), ct);
            if (result.Success)
            {
                record.MarkAsSent(result.MessageId);
                _logger.LogInformation("Email sent to {Email} for template {Template}", recipientEmail, templateKey);
            }
            else
            {
                record.MarkAsFailed(result.Error ?? "Unknown error");
                _logger.LogWarning("Failed to send email to {Email}: {Error}", recipientEmail, result.Error);
            }
        }
        catch (Exception ex)
        {
            record.MarkAsFailed(ex.Message);
            _logger.LogError(ex, "Exception sending email to {Email}", recipientEmail);
        }

        await _notificationRepo.AddRecordAsync(record, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task SendSmsInternalAsync(string userId, string templateKey, string phoneNumber, string message, CancellationToken ct)
    {
        var record = NotificationRecord.Create(
            Guid.TryParse(userId, out var uid) ? uid : Guid.Empty,
            templateKey,
            "Sms",
            message.Length > 50 ? message[..50] + "..." : message,
            phoneNumber);

        try
        {
            var result = await _smsSender.SendAsync(phoneNumber, message, ct);
            if (result.Success)
            {
                record.MarkAsSent(result.MessageId);
                _logger.LogInformation("SMS sent to {Phone} for template {Template}", MaskPhone(phoneNumber), templateKey);
            }
            else
            {
                record.MarkAsFailed(result.Error ?? "Unknown error");
                _logger.LogWarning("Failed to send SMS to {Phone}: {Error}", MaskPhone(phoneNumber), result.Error);
            }
        }
        catch (Exception ex)
        {
            record.MarkAsFailed(ex.Message);
            _logger.LogError(ex, "Exception sending SMS to {Phone}", MaskPhone(phoneNumber));
        }

        await _notificationRepo.AddRecordAsync(record, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task SendPushInternalAsync(string userId, string templateKey, string title, string body, Dictionary<string, string> data, CancellationToken ct)
    {
        var record = NotificationRecord.Create(
            Guid.TryParse(userId, out var uid) ? uid : Guid.Empty,
            templateKey,
            "Push",
            title,
            userId);

        try
        {
            var result = await _pushSender.SendAsync(userId, title, body, data, ct);
            if (result.Success)
            {
                record.MarkAsSent(result.MessageId);
                _logger.LogInformation("Push notification sent to user {UserId} for template {Template}", userId, templateKey);
            }
            else
            {
                record.MarkAsFailed(result.Error ?? "Unknown error");
                _logger.LogWarning("Failed to send push to user {UserId}: {Error}", userId, result.Error);
            }
        }
        catch (Exception ex)
        {
            record.MarkAsFailed(ex.Message);
            _logger.LogError(ex, "Exception sending push to user {UserId}", userId);
        }

        await _notificationRepo.AddRecordAsync(record, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static string RenderTemplate(string template, Dictionary<string, string> data)
        => TemplateHelper.RenderTemplate(template, data);

    private static string StripHtml(string html)
        => TemplateHelper.StripHtml(html);

    private static string MaskPhone(string phone)
        => TemplateHelper.MaskPhone(phone);

    #endregion
}
