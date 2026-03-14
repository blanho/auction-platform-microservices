using Microsoft.Extensions.Logging;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;

namespace Notification.Infrastructure.Senders;

public class SmsNotificationSender : ISmsSender
{
    private readonly ILogger<SmsNotificationSender> _logger;

    public SmsNotificationSender(ILogger<SmsNotificationSender> logger)
    {
        _logger = logger;
    }

    public async Task<SmsSendResult> SendAsync(
        string phoneNumber,
        string message,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(phoneNumber))
        {
            return new SmsSendResult(false, Error: "Phone number is required");
        }

        if (string.IsNullOrEmpty(message))
        {
            return new SmsSendResult(false, Error: "Message is required");
        }

        try
        {

            _logger.LogInformation(
                "Sending SMS to {PhoneNumber}, Length: {Length}",
                PhoneNumberHelper.MaskPhoneNumber(phoneNumber),
                message.Length);

            await Task.Delay(10, ct);

            var messageId = Guid.NewGuid().ToString();

            _logger.LogInformation(
                "SMS sent successfully to {PhoneNumber}. MessageId: {MessageId}",
                PhoneNumberHelper.MaskPhoneNumber(phoneNumber),
                messageId);

            return new SmsSendResult(true, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", PhoneNumberHelper.MaskPhoneNumber(phoneNumber));
            return new SmsSendResult(false, Error: ex.Message);
        }
    }
}
