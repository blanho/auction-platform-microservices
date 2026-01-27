namespace Notification.Domain.Entities;

public class NotificationPreference : BaseEntity
{
    public string UserId { get; private set; } = string.Empty;
    public bool EmailEnabled { get; private set; } = true;
    public bool PushEnabled { get; private set; } = true;
    public bool BidUpdates { get; private set; } = true;
    public bool AuctionUpdates { get; private set; } = true;
    public bool PromotionalEmails { get; private set; } = false;
    public bool SystemAlerts { get; private set; } = true;

    private NotificationPreference() { }

    public static NotificationPreference CreateDefault(string userId)
    {
        return new NotificationPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EmailEnabled = true,
            PushEnabled = true,
            BidUpdates = true,
            AuctionUpdates = true,
            PromotionalEmails = false,
            SystemAlerts = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(
        bool? emailEnabled = null,
        bool? pushEnabled = null,
        bool? bidUpdates = null,
        bool? auctionUpdates = null,
        bool? promotionalEmails = null,
        bool? systemAlerts = null)
    {
        if (emailEnabled.HasValue) EmailEnabled = emailEnabled.Value;
        if (pushEnabled.HasValue) PushEnabled = pushEnabled.Value;
        if (bidUpdates.HasValue) BidUpdates = bidUpdates.Value;
        if (auctionUpdates.HasValue) AuctionUpdates = auctionUpdates.Value;
        if (promotionalEmails.HasValue) PromotionalEmails = promotionalEmails.Value;
        if (systemAlerts.HasValue) SystemAlerts = systemAlerts.Value;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
