#nullable enable
using NotificationService.Application.DTOs;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Notification> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Notification> CreateAsync(Notification notification, CancellationToken cancellationToken = default);
        Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Notification>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<List<Notification>> GetUnreadByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<int> GetUnreadCountByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
        Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
        Task<List<Notification>> GetOldReadNotificationsAsync(int retentionDays, CancellationToken cancellationToken = default);
        Task DeleteRangeAsync(List<Notification> notifications, CancellationToken cancellationToken = default);
    }

    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public interface INotificationService
    {
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);
        Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, CancellationToken cancellationToken = default);
        Task<NotificationSummaryDto> GetNotificationSummaryAsync(string userId, CancellationToken cancellationToken = default);
        Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
        Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
        Task DeleteNotificationAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedNotificationsDto> GetAllNotificationsAsync(int pageNumber, int pageSize, string? userId, string? type, string? status, CancellationToken cancellationToken = default);
        Task BroadcastNotificationAsync(BroadcastNotificationDto dto, CancellationToken cancellationToken = default);
        Task<NotificationStatsDto> GetNotificationStatsAsync(CancellationToken cancellationToken = default);
    }

    public interface INotificationHubService
    {
        Task SendNotificationToUserAsync(string userId, NotificationDto notification);
        Task SendNotificationToAllAsync(NotificationDto notification);
    }

    public interface IEmailService
    {
        Task<SendEmailResultDto> SendEmailAsync(EmailDto email, CancellationToken cancellationToken = default);
        Task<SendEmailResultDto> SendTemplatedEmailAsync(string to, string templateName, Dictionary<string, string> data, CancellationToken cancellationToken = default);
        Task SendAuctionWonEmailAsync(string to, string auctionTitle, int winningBid, Guid auctionId, CancellationToken cancellationToken = default);
        Task SendOutbidEmailAsync(string to, string auctionTitle, int newBid, Guid auctionId, CancellationToken cancellationToken = default);
        Task SendBuyNowConfirmationEmailAsync(string to, string auctionTitle, int price, Guid auctionId, CancellationToken cancellationToken = default);
        Task SendOrderShippedEmailAsync(string to, string itemTitle, string trackingNumber, string? carrier, CancellationToken cancellationToken = default);
        Task SendAuctionEndingSoonEmailAsync(string to, string auctionTitle, TimeSpan timeRemaining, Guid auctionId, CancellationToken cancellationToken = default);
    }
}
