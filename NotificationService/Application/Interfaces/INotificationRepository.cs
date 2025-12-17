#nullable enable
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces;

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
