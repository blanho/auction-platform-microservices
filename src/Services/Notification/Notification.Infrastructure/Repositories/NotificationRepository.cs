using BuildingBlocks.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications.FindAsync([id], cancellationToken);
    }

    public async Task<NotificationEntity> CreateAsync(NotificationEntity notification, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddAsync(notification, cancellationToken);
        return notification;
    }

    public async Task UpdateAsync(NotificationEntity notification, CancellationToken cancellationToken = default)
    {
        _context.Notifications.Update(notification);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications.FindAsync([id], cancellationToken);
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
        }
    }

    public async Task<List<NotificationEntity>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NotificationEntity>> GetUnreadByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && n.Status == NotificationStatus.Unread, cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications.FindAsync([id], cancellationToken);
        notification?.MarkAsRead();
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _context.Notifications
            .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
            .ExecuteUpdateAsync(
                s => s.SetProperty(n => n.Status, NotificationStatus.Read)
                      .SetProperty(n => n.ReadAt, DateTimeOffset.UtcNow),
                cancellationToken);
    }

    public async Task<List<NotificationEntity>> GetOldReadNotificationsAsync(int retentionDays, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-retentionDays);
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.Status == NotificationStatus.Read && n.ReadAt < cutoffDate)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteRangeAsync(List<NotificationEntity> notifications, CancellationToken cancellationToken = default)
    {
        _context.Notifications.RemoveRange(notifications);
        await Task.CompletedTask;
    }

    public async Task<PaginatedResult<NotificationEntity>> GetPaginatedAsync(
        int page,
        int pageSize,
        string? userId = null,
        NotificationType? type = null,
        NotificationStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(n => n.UserId == userId);

        if (type.HasValue)
            query = query.Where(n => n.Type == type.Value);

        if (status.HasValue)
            query = query.Where(n => n.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<NotificationEntity>(items, totalCount, page, pageSize);
    }

    public async Task<NotificationStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTimeOffset.UtcNow.Date;

        var totalCount = await _context.Notifications.AsNoTracking().CountAsync(cancellationToken);
        var unreadCount = await _context.Notifications.AsNoTracking().CountAsync(n => n.Status == NotificationStatus.Unread, cancellationToken);
        var todayCount = await _context.Notifications.AsNoTracking().CountAsync(n => n.CreatedAt.Date == today, cancellationToken);

        var byType = await _context.Notifications
            .AsNoTracking()
            .GroupBy(n => n.Type)
            .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);

        return new NotificationStats(totalCount, unreadCount, todayCount, byType);
    }
}
