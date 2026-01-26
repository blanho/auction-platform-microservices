using BuildingBlocks.Application.Abstractions;
using Notification.Domain.Entities;

namespace Notification.Application.Interfaces;

public interface INotificationRecordRepository
{

    Task AddRecordAsync(NotificationRecord record, CancellationToken ct = default);

    Task<List<NotificationRecord>> GetRecordsByUserIdAsync(Guid userId, int skip = 0, int take = 50, CancellationToken ct = default);

    Task<NotificationRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<NotificationRecord>> GetPagedAsync(
        Guid? userId = null,
        string? channel = null,
        string? status = null,
        string? templateKey = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    Task<int> GetTotalCountAsync(CancellationToken ct = default);
    Task<int> GetCountByStatusAsync(NotificationRecordStatus status, CancellationToken ct = default);
    Task<Dictionary<string, int>> GetCountByChannelAsync(CancellationToken ct = default);
    Task<Dictionary<string, int>> GetCountByTemplateAsync(CancellationToken ct = default);

    Task AddUserNotificationAsync(UserNotification notification, CancellationToken ct = default);

    Task<List<UserNotification>> GetUserNotificationsAsync(string userId, int skip = 0, int take = 20, CancellationToken ct = default);

    Task<UserNotification?> GetUserNotificationByIdAsync(Guid id, CancellationToken ct = default);

    Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);

    Task MarkAsReadAsync(Guid notificationId, CancellationToken ct = default);

    Task MarkAllAsReadAsync(string userId, CancellationToken ct = default);

    Task UpdateUserNotificationAsync(UserNotification notification, CancellationToken ct = default);
}
