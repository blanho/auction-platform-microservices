using IdentityService.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using IUnitOfWork = Notification.Application.Interfaces.IUnitOfWork;

namespace Notification.Infrastructure.Consumers;

public class UserLoginConsumer : IConsumer<UserLoginEvent>
{
    private readonly IIdempotencyService _idempotency;
    private readonly ITemplateRepository _templateRepo;
    private readonly INotificationRecordRepository _recordRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<UserLoginConsumer> _logger;

    public UserLoginConsumer(
        IIdempotencyService idempotency,
        ITemplateRepository templateRepo,
        INotificationRecordRepository recordRepo,
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        ILogger<UserLoginConsumer> logger)
    {
        _idempotency = idempotency;
        _templateRepo = templateRepo;
        _recordRepo = recordRepo;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserLoginEvent> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"user-login-{message.UserId}-{message.LoginAt:yyyyMMddHHmmss}";

        _logger.LogInformation(
            "Processing UserLoginEvent: UserId={UserId}, Username={Username}, IP={IpAddress}",
            message.UserId,
            message.Username,
            message.IpAddress);

        if (await _idempotency.IsProcessedAsync(eventId, "Email", ct))
            return;

        var template = await _templateRepo.GetByKeyAsync("login-notification", ct);
        if (template == null || !template.IsActive)
        {
            _logger.LogDebug("Template 'login-notification' not found or inactive - skipping login notification");
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "Email", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "Email", ct))
            return;

        var data = new Dictionary<string, string>
        {
            ["username"] = message.Username,
            ["ipAddress"] = message.IpAddress,
            ["loginAt"] = message.LoginAt.ToString("f")
        };

        var subject = TemplateHelper.RenderTemplate(template.Subject ?? "New Login to Your Account", data);
        var body = TemplateHelper.RenderTemplate(template.Body, data);

        var record = Notification.Domain.Entities.NotificationRecord.Create(
            Guid.TryParse(message.UserId, out var uid) ? uid : Guid.Empty,
            "login-notification",
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
                _logger.LogInformation("Login notification sent to {Email}", message.Email);
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
}
