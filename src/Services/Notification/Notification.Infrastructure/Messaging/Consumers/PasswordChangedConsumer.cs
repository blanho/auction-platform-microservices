using IdentityService.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using IUnitOfWork = Notification.Application.Interfaces.IUnitOfWork;

namespace Notification.Infrastructure.Consumers;

public class PasswordChangedConsumer : IConsumer<PasswordChangedEvent>
{
    private readonly IIdempotencyService _idempotency;
    private readonly ITemplateRepository _templateRepo;
    private readonly INotificationRecordRepository _recordRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<PasswordChangedConsumer> _logger;

    public PasswordChangedConsumer(
        IIdempotencyService idempotency,
        ITemplateRepository templateRepo,
        INotificationRecordRepository recordRepo,
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        ILogger<PasswordChangedConsumer> logger)
    {
        _idempotency = idempotency;
        _templateRepo = templateRepo;
        _recordRepo = recordRepo;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PasswordChangedEvent> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"password-changed-{message.UserId}-{message.ChangedAt:yyyyMMddHHmmss}";

        _logger.LogInformation(
            "Processing PasswordChangedEvent: UserId={UserId}, Username={Username}",
            message.UserId,
            message.Username);

        if (await _idempotency.IsProcessedAsync(eventId, "Email", ct))
        {
            _logger.LogDebug("Email already sent for EventId={EventId}", eventId);
            return;
        }

        var template = await _templateRepo.GetByKeyAsync("password-changed", ct);
        if (template == null || !template.IsActive)
        {
            _logger.LogWarning("Template 'password-changed' not found or inactive");
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "Email", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "Email", ct))
            return;

        var data = new Dictionary<string, string>
        {
            ["username"] = message.Username,
            ["changedAt"] = message.ChangedAt.ToString("f")
        };

        var subject = TemplateHelper.RenderTemplate(template.Subject ?? "Your Password Has Been Changed", data);
        var body = TemplateHelper.RenderTemplate(template.Body, data);

        var record = Notification.Domain.Entities.NotificationRecord.Create(
            Guid.TryParse(message.UserId, out var uid) ? uid : Guid.Empty,
            "password-changed",
            "Email",
            subject,
            message.Email);

        try
        {
            var result = await _emailSender.SendAsync(
                message.Email,
                subject,
                body,
                TemplateHelper.StripHtml(body),
                ct);

            if (result.Success)
            {
                record.MarkAsSent(result.MessageId);
                await _idempotency.MarkAsProcessedAsync(eventId, "Email", result.MessageId, ct: ct);
                _logger.LogInformation("Password change notification sent to {Email}", message.Email);
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
