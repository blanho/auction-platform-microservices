using Notification.Domain.Entities;

namespace Notification.Application.Interfaces;

public interface INotificationRecordRepository
{

    Task AddRecordAsync(NotificationRecord record, CancellationToken ct = default);

    Task<List<NotificationRecord>> GetRecordsByUserIdAsync(Guid userId, int skip = 0, int take = 50, CancellationToken ct = default);

    Task AddUserNotificationAsync(UserNotification notification, CancellationToken ct = default);

    Task<List<UserNotification>> GetUserNotificationsAsync(string userId, int skip = 0, int take = 20, CancellationToken ct = default);

    Task<UserNotification?> GetUserNotificationByIdAsync(Guid id, CancellationToken ct = default);

    Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);

    Task MarkAsReadAsync(Guid notificationId, CancellationToken ct = default);

    Task MarkAllAsReadAsync(string userId, CancellationToken ct = default);

    Task UpdateUserNotificationAsync(UserNotification notification, CancellationToken ct = default);
}
