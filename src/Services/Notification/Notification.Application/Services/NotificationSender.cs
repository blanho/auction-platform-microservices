using Microsoft.Extensions.Logging;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Constants;
using Notification.Domain.Entities;

namespace Notification.Application.Services;

public class NotificationSender : INotificationSender
{
    private readonly ITemplateRepository _templateRepo;
    private readonly INotificationRecordRepository _notificationRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationDelivery _delivery;
    private readonly INotificationHubService _hubService;
    private readonly ILogger<NotificationSender> _logger;

    public NotificationSender(
        ITemplateRepository templateRepo,
        INotificationRecordRepository notificationRepo,
        IUnitOfWork unitOfWork,
        INotificationDelivery delivery,
        INotificationHubService hubService,
        ILogger<NotificationSender> logger)
    {
        _templateRepo = templateRepo;
        _notificationRepo = notificationRepo;
        _unitOfWork = unitOfWork;
        _delivery = delivery;
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

        var subject = TemplateHelper.RenderTemplate(template.Subject, request.Data);
        var body = TemplateHelper.RenderTemplate(template.Body, request.Data);

        if (request.SendEmail && !string.IsNullOrEmpty(request.RecipientEmail))
        {
            await _delivery.SendEmailAsync(request.UserId, request.TemplateKey, request.RecipientEmail, subject, body, ct);
        }

        if (request.SendSms && !string.IsNullOrEmpty(request.RecipientPhone) && !string.IsNullOrEmpty(template.SmsBody))
        {
            var smsBody = TemplateHelper.RenderTemplate(template.SmsBody, request.Data);
            await _delivery.SendSmsAsync(request.UserId, request.TemplateKey, request.RecipientPhone, smsBody, ct);
        }

        if (request.SendPush && !string.IsNullOrEmpty(template.PushBody))
        {
            var pushTitle = TemplateHelper.RenderTemplate(template.PushTitle ?? subject, request.Data);
            var pushBody = TemplateHelper.RenderTemplate(template.PushBody, request.Data);
            await _delivery.SendPushAsync(request.UserId, request.TemplateKey, pushTitle, pushBody, request.Data, ct);
        }

        if (request.SendInApp)
        {
            await SendInAppAsync(
                request.UserId,
                subject,
                TemplateHelper.StripHtml(body),
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

        var subject = TemplateHelper.RenderTemplate(template.Subject, data);
        var body = TemplateHelper.RenderTemplate(template.Body, data);
        await _delivery.SendEmailAsync(userId, templateKey, recipientEmail, subject, body, ct);
    }

    public async Task SendSmsAsync(string userId, string templateKey, Dictionary<string, string> data, string phoneNumber, CancellationToken ct = default)
    {
        var template = await _templateRepo.GetByKeyAsync(templateKey, ct);
        if (template == null || !template.IsActive || string.IsNullOrEmpty(template.SmsBody))
        {
            _logger.LogWarning("Template '{Key}' not found, inactive, or has no SMS body", templateKey);
            return;
        }

        var smsBody = TemplateHelper.RenderTemplate(template.SmsBody, data);
        await _delivery.SendSmsAsync(userId, templateKey, phoneNumber, smsBody, ct);
    }

    public async Task SendPushAsync(string userId, string templateKey, Dictionary<string, string> data, CancellationToken ct = default)
    {
        var template = await _templateRepo.GetByKeyAsync(templateKey, ct);
        if (template == null || !template.IsActive || string.IsNullOrEmpty(template.PushBody))
        {
            _logger.LogWarning("Template '{Key}' not found, inactive, or has no push body", templateKey);
            return;
        }

        var pushTitle = TemplateHelper.RenderTemplate(template.PushTitle ?? template.Subject, data);
        var pushBody = TemplateHelper.RenderTemplate(template.PushBody, data);
        await _delivery.SendPushAsync(userId, templateKey, pushTitle, pushBody, data, ct);
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
                Status = nameof(NotificationStatus.Unread),
                CreatedAt = notification.CreatedAt
            };
            await _hubService.SendNotificationToUserAsync(userId, dto);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to send real-time notification to user {UserId}", userId);
        }
    }

    public async Task<List<UserNotification>> GetUserNotificationsAsync(string userId, int skip = 0, int take = NotificationDefaults.Pagination.DefaultPageSize, CancellationToken ct = default)
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

}
