using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Application.Interfaces;
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

        var formattedPhone = FormatPhoneNumber(phoneNumber);
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
                MaskPhoneNumber(formattedPhone),
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
                MaskPhoneNumber(formattedPhone));

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
            _logger.LogError(ex, "Failed to send SMS to {Phone}", MaskPhoneNumber(phoneNumber));
            return new SmsSendResult(false, Error: ex.Message);
        }
    }

    private static string? FormatPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone))
            return null;

        var cleaned = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());

        if (cleaned.StartsWith('+'))
            return cleaned;

        if (cleaned.Length >= 10)
        {

            if (cleaned.Length == 10)
                return $"+1{cleaned}";

            if (cleaned.Length == 11 && cleaned.StartsWith('1'))
                return $"+{cleaned}";

            return $"+{cleaned}";
        }

        return null;
    }

    private static string MaskPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 6)
            return "***";

        return phone[..3] + new string('*', phone.Length - 6) + phone[^3..];
    }
}

public class TwilioOptions
{
    public const string SectionName = "Twilio";

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Twilio AccountSid is required")]
    public string AccountSid { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Twilio AuthToken is required")]
    public string AuthToken { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Twilio FromNumber is required")]
    [System.ComponentModel.DataAnnotations.Phone(ErrorMessage = "FromNumber must be a valid phone number")]
    public string FromNumber { get; set; } = string.Empty;

    public string? MessagingServiceSid { get; set; }

    public string? StatusCallbackUrl { get; set; }

    [System.ComponentModel.DataAnnotations.Range(1, 10000, ErrorMessage = "MaxMessageLength must be between 1 and 10000")]
    public int MaxMessageLength { get; set; } = 1600;

    public string DefaultCountryCode { get; set; } = "1";
}
