using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Persistence.Repositories;

public class NotificationRepository : EfRepository<NotificationEntity, NotificationDbContext>, INotificationRepository
{
    public NotificationRepository(
        NotificationDbContext context, IDateTimeProvider clock, IAuditContext auditContext)
        : base(context, clock, auditContext)
    {
    }

    public override async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await GetByIdForUpdateAsync(id, cancellationToken);
        if (notification != null)
        {
            Context.Notifications.Remove(notification);
        }
    }

    public async Task<List<NotificationEntity>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await Query
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NotificationEntity>> GetUnreadByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await Query
            .AsNoTracking()
            .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<int> GetUnreadCountByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        CountAsync(notification => notification.UserId == userId && notification.Status == NotificationStatus.Unread,
            cancellationToken);

    public async Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await GetByIdForUpdateAsync(id, cancellationToken);
        notification?.MarkAsRead();
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
    {
        await Query
            .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
            .ExecuteUpdateAsync(
                s => s.SetProperty(n => n.Status, NotificationStatus.Read)
                      .SetProperty(n => n.ReadAt, DateTimeOffset.UtcNow),
                cancellationToken);
    }

    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await GetByIdForUpdateAsync(id, cancellationToken);
        notification?.Archive();
    }

    public async Task<List<NotificationEntity>> GetOldReadNotificationsAsync(int retentionDays, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-retentionDays);
        return await Query
            .AsNoTracking()
            .Where(n => n.Status == NotificationStatus.Read && n.ReadAt < cutoffDate)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteRangeAsync(List<NotificationEntity> notifications, CancellationToken cancellationToken = default)
    {
        Context.Notifications.RemoveRange(notifications);
        await Task.CompletedTask;
    }

    public Task<PaginatedResult<NotificationEntity>> GetPaginatedAsync(
        int page,
        int pageSize,
        string? userId = null,
        NotificationType? type = null,
        NotificationStatus? status = null,
        CancellationToken cancellationToken = default) =>
        GetPagedAsync(notification => (string.IsNullOrEmpty(userId) || notification.UserId == userId)
            && (!type.HasValue || notification.Type == type)
            && (!status.HasValue || notification.Status == status), page, pageSize, cancellationToken);

    public async Task<NotificationStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTimeOffset.UtcNow.Date;

        var totalCount = await CountAsync(cancellationToken);
        var unreadCount = await CountAsync(n => n.Status == NotificationStatus.Unread, cancellationToken);
        var todayCount = await CountAsync(n => n.CreatedAt.Date == today, cancellationToken);

        var byType = await Query
            .AsNoTracking()
            .GroupBy(n => n.Type)
            .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);

        return new NotificationStats(totalCount, unreadCount, todayCount, byType);
    }
}
