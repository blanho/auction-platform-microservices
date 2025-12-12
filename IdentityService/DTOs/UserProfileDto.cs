namespace IdentityService.DTOs;

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}

public class UpdateProfileDto
{
    public string? FullName { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? PhoneNumber { get; set; }
}

public class NotificationPreferencesDto
{
    public bool EmailBidUpdates { get; set; }
    public bool EmailOutbid { get; set; }
    public bool EmailAuctionEnd { get; set; }
    public bool EmailNewsletter { get; set; }
    public bool PushNotifications { get; set; }
    public bool SmsNotifications { get; set; }
}

public class ChangePasswordDto
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}
