using Microsoft.AspNetCore.Identity;

namespace IdentityService.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSuspended { get; set; }
    public string? SuspensionReason { get; set; }
    public DateTimeOffset? SuspendedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAt { get; set; }
    
    public bool EmailBidUpdates { get; set; } = true;
    public bool EmailOutbid { get; set; } = true;
    public bool EmailAuctionEnd { get; set; } = true;
    public bool EmailNewsletter { get; set; } = false;
    public bool PushNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
}
