using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Notification.Infrastructure.Senders;

public class TwilioSmsSender : ISmsSender
{
    private readonly TwilioOptions _options;
    private readonly ILogger<TwilioSmsSender> _logger;
    private readonly bool _isInitialized;

    public TwilioSmsSender(
        IOptions<TwilioOptions> options,
        ILogger<TwilioSmsSender> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (!string.IsNullOrEmpty(_options.AccountSid) && !string.IsNullOrEmpty(_options.AuthToken))
        {
            TwilioClient.Init(_options.AccountSid, _options.AuthToken);
            _isInitialized = true;
        }
        else
        {
            _logger.LogWarning("Twilio credentials not configured. SMS sending will be disabled.");
            _isInitialized = false;
        }
    }

    public async Task<SmsSendResult> SendAsync(
        string phoneNumber,
        string message,
        CancellationToken ct = default)
    {
        if (!_isInitialized)
            return new SmsSendResult(false, Error: "Twilio not configured");

        if (string.IsNullOrEmpty(phoneNumber))
            return new SmsSendResult(false, Error: "Phone number is required");

        if (string.IsNullOrEmpty(message))
            return new SmsSendResult(false, Error: "Message is required");

        var formattedPhone = PhoneNumberHelper.FormatPhoneNumber(phoneNumber);
        if (formattedPhone == null)
            return new SmsSendResult(false, Error: "Invalid phone number format");

        if (message.Length > _options.MaxMessageLength)
        {
            _logger.LogWarning(
                "Message truncated from {Original} to {Max} characters",
                message.Length,
                _options.MaxMessageLength);
            message = message[.._options.MaxMessageLength];
        }

        try
        {
            _logger.LogDebug(
                "Sending SMS via Twilio to {Phone}, Length: {Length}",
                PhoneNumberHelper.MaskPhoneNumber(formattedPhone),
                message.Length);

            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_options.FromNumber),
                to: new PhoneNumber(formattedPhone),

                messagingServiceSid: string.IsNullOrEmpty(_options.MessagingServiceSid)
                    ? null
                    : _options.MessagingServiceSid,

                statusCallback: string.IsNullOrEmpty(_options.StatusCallbackUrl)
                    ? null
                    : new Uri(_options.StatusCallbackUrl)
            );

            if (messageResource.Status == MessageResource.StatusEnum.Failed ||
                messageResource.Status == MessageResource.StatusEnum.Undelivered)
            {
                _logger.LogWarning(
                    "Twilio SMS failed: Status={Status}, ErrorCode={ErrorCode}, ErrorMessage={ErrorMessage}",
                    messageResource.Status,
                    messageResource.ErrorCode,
                    messageResource.ErrorMessage);

                return new SmsSendResult(
                    false,
                    messageResource.Sid,
                    Error: $"{messageResource.ErrorCode}: {messageResource.ErrorMessage}");
            }

            _logger.LogInformation(
                "SMS sent successfully via Twilio. SID: {Sid}, Status: {Status}, To: {To}",
                messageResource.Sid,
                messageResource.Status,
                PhoneNumberHelper.MaskPhoneNumber(formattedPhone));

            return new SmsSendResult(true, messageResource.Sid);
        }
        catch (Twilio.Exceptions.ApiException ex)
        {
            _logger.LogError(ex,
                "Twilio API error: Code={Code}, Message={Message}, MoreInfo={MoreInfo}",
                ex.Code,
                ex.Message,
                ex.MoreInfo);

            return new SmsSendResult(false, Error: $"Twilio error {ex.Code}: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {Phone}", PhoneNumberHelper.MaskPhoneNumber(phoneNumber));
            return new SmsSendResult(false, Error: ex.Message);
        }
    }
}
