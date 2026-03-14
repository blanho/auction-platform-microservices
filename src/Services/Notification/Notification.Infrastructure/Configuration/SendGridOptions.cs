using System.ComponentModel.DataAnnotations;

namespace Notification.Infrastructure.Configuration;

public class SendGridOptions
{
    public const string SectionName = "SendGrid";

    [Required(ErrorMessage = "SendGrid ApiKey is required")]
    public string ApiKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "SendGrid FromEmail is required")]
    [EmailAddress(ErrorMessage = "FromEmail must be a valid email address")]
    public string FromEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "SendGrid FromName is required")]
    public string FromName { get; set; } = string.Empty;

    public bool EnableClickTracking { get; set; } = true;
    public bool EnableOpenTracking { get; set; } = true;
    public string? DefaultCategory { get; set; }
    public bool SandboxMode { get; set; } = false;
}
