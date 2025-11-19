using Microsoft.Extensions.Logging;
using Notification.Application.Interfaces;

namespace Notification.Infrastructure.Senders;

public class PushSender : IPushSender
{
    private readonly ILogger<PushSender> _logger;

    public PushSender(ILogger<PushSender> logger)
    {
        _logger = logger;
    }

    public async Task<PushSendResult> SendAsync(
        string userId,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return new PushSendResult(false, Error: "User ID is required");
        }

        if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(body))
        {
            return new PushSendResult(false, Error: "Title or body is required");
        }

        try
        {

            _logger.LogInformation(
                "Sending push notification to user {UserId}, Title: {Title}",
                userId,
                title);

            await Task.Delay(10, ct);

            var messageId = Guid.NewGuid().ToString();

            _logger.LogInformation(
                "Push notification sent successfully to user {UserId}. MessageId: {MessageId}",
                userId,
                messageId);

            return new PushSendResult(true, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to user {UserId}", userId);
            return new PushSendResult(false, Error: ex.Message);
        }
    }
}
