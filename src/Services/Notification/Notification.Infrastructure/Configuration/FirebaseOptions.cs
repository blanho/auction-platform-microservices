using System.ComponentModel.DataAnnotations;

namespace Notification.Infrastructure.Configuration;

public class FirebaseOptions
{
    public const string SectionName = "Firebase";

    public string ServiceAccountPath { get; set; } = string.Empty;
    public string? ServiceAccountJson { get; set; }

    [Required(ErrorMessage = "Firebase ProjectId is required")]
    public string ProjectId { get; set; } = string.Empty;

    public string DefaultAndroidChannel { get; set; } = "default_channel";
    public string? WebPushIcon { get; set; }

    [Range(1, 168, ErrorMessage = "TimeToLiveHours must be between 1 and 168")]
    public int TimeToLiveHours { get; set; } = 24;
}
