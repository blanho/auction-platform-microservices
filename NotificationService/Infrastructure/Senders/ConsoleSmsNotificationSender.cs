using Microsoft.Extensions.Logging;
using NotificationService.Application.Ports;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Senders;

public class ConsoleSmsOptions
{
    public bool Enabled { get; set; } = true;
}

public class ConsoleSmsNotificationSender : INotificationSender
{
    private readonly ILogger<ConsoleSmsNotificationSender> _logger;

    public ChannelType Channel => ChannelType.Sms;

    public ConsoleSmsNotificationSender(ILogger<ConsoleSmsNotificationSender> logger)
    {
        _logger = logger;
    }

    public Task<SendResult> SendAsync(SendRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.RecipientPhone))
        {
            return Task.FromResult(SendResult.Failed(ChannelType.Sms, "No phone number provided"));
        }

        _logger.LogInformation(
            "[SMS SIMULATION] To: {Phone}, Message: {Message}",
            request.RecipientPhone,
            request.Body?.Length > 160 ? request.Body[..160] + "..." : request.Body);

        return Task.FromResult(SendResult.Succeeded(ChannelType.Sms));
    }
}
