using Microsoft.Extensions.Logging;
using NotificationService.Application.Ports;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Senders;

public class ConsolePushNotificationSender : INotificationSender
{
    private readonly ILogger<ConsolePushNotificationSender> _logger;

    public ChannelType Channel => ChannelType.Push;

    public ConsolePushNotificationSender(ILogger<ConsolePushNotificationSender> logger)
    {
        _logger = logger;
    }

    public Task<SendResult> SendAsync(SendRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.DeviceToken))
        {
            return Task.FromResult(SendResult.Failed(ChannelType.Push, "No device token provided"));
        }

        _logger.LogInformation(
            "[PUSH SIMULATION] Token: {Token}, Title: {Title}, Body: {Body}",
            request.DeviceToken[..Math.Min(20, request.DeviceToken.Length)] + "...",
            request.Subject,
            request.Body?.Length > 100 ? request.Body[..100] + "..." : request.Body);

        return Task.FromResult(SendResult.Succeeded(ChannelType.Push));
    }
}
