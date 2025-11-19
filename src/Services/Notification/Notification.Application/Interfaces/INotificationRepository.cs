#nullable enable
using BuildingBlocks.Application.Abstractions;
using Notification.Domain.Enums;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Application.Interfaces;

public interface INotificationRepository
{
    Task<List<NotificationEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<NotificationEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NotificationEntity> CreateAsync(NotificationEntity notification, CancellationToken cancellationToken = default);
    Task UpdateAsync(NotificationEntity notification, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<NotificationEntity>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<NotificationEntity>> GetUnreadByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<NotificationEntity>> GetOldReadNotificationsAsync(int retentionDays, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(List<NotificationEntity> notifications, CancellationToken cancellationToken = default);

    Task<PaginatedResult<NotificationEntity>> GetPaginatedAsync(
        int page,
        int pageSize,
        string? userId = null,
        NotificationType? type = null,
        NotificationStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<NotificationStats> GetStatsAsync(CancellationToken cancellationToken = default);
}

public record NotificationStats(
    int TotalCount,
    int UnreadCount,
    int TodayCount,
    Dictionary<string, int> ByType);
