using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Ports;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Notification?> GetByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<List<Notification>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<Notification>> GetPendingAsync(int limit, CancellationToken cancellationToken = default);
    
    Task CreateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<int> MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);

    Task<(List<Notification> Items, int TotalCount)> GetPagedAsync(
        string userId,
        NotificationStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
