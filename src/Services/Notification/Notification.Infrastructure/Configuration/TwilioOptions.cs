using System.ComponentModel.DataAnnotations;

namespace Notification.Infrastructure.Configuration;

public class TwilioOptions
{
    public const string SectionName = "Twilio";

    [Required(ErrorMessage = "Twilio AccountSid is required")]
    public string AccountSid { get; set; } = string.Empty;

    [Required(ErrorMessage = "Twilio AuthToken is required")]
    public string AuthToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "Twilio FromNumber is required")]
    [Phone(ErrorMessage = "FromNumber must be a valid phone number")]
    public string FromNumber { get; set; } = string.Empty;

    public string? MessagingServiceSid { get; set; }
    public string? StatusCallbackUrl { get; set; }

    [Range(1, 10000, ErrorMessage = "MaxMessageLength must be between 1 and 10000")]
    public int MaxMessageLength { get; set; } = 1600;

    public string DefaultCountryCode { get; set; } = "1";
}
