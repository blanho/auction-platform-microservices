using Notification.Domain.Enums;

namespace Notification.Application.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ReadAt { get; set; }
    public Guid? AuctionId { get; set; }
    public Guid? BidId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class CreateNotificationDto
{
    public string UserId { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public Guid? AuctionId { get; set; }
    public Guid? BidId { get; set; }
}

public class MarkAsReadDto
{
    public Guid NotificationId { get; set; }
}

public class NotificationSummaryDto
{
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
    public List<NotificationDto> RecentNotifications { get; set; } = new();
}

public class BroadcastNotificationDto
{
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TargetRole { get; set; }
}

public class NotificationStatsDto
{
    public int TotalNotifications { get; set; }
    public int UnreadNotifications { get; set; }
    public int TodayCount { get; set; }
    public Dictionary<string, int> ByType { get; set; } = new();
}

public class NotificationPreferenceDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool EmailEnabled { get; set; }
    public bool PushEnabled { get; set; }
    public bool BidUpdates { get; set; }
    public bool AuctionUpdates { get; set; }
    public bool PromotionalEmails { get; set; }
    public bool SystemAlerts { get; set; }
}

public class UpdateNotificationPreferenceDto
{
    public bool? EmailEnabled { get; set; }
    public bool? PushEnabled { get; set; }
    public bool? BidUpdates { get; set; }
    public bool? AuctionUpdates { get; set; }
    public bool? PromotionalEmails { get; set; }
    public bool? SystemAlerts { get; set; }
}
