using Common.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty; // JSON data for additional context
        public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
        public DateTime? ReadAt { get; set; }
        public Guid? AuctionId { get; set; }
        public Guid? BidId { get; set; }
    }
}
