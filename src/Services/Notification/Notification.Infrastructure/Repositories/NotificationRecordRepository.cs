using Microsoft.EntityFrameworkCore;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Repositories;

public class NotificationRecordRepository : INotificationRecordRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRecordRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task AddRecordAsync(NotificationRecord record, CancellationToken ct = default)
    {
        await _context.Records.AddAsync(record, ct);
    }

    public async Task<List<NotificationRecord>> GetRecordsByUserIdAsync(Guid userId, int skip = 0, int take = 50, CancellationToken ct = default)
    {
        return await _context.Records
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task AddUserNotificationAsync(UserNotification notification, CancellationToken ct = default)
    {
        await _context.UserNotifications.AddAsync(notification, ct);
    }

    public async Task<List<UserNotification>> GetUserNotificationsAsync(string userId, int skip = 0, int take = 20, CancellationToken ct = default)
    {
        return await _context.UserNotifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<UserNotification?> GetUserNotificationByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.UserNotifications.FindAsync(new object[] { id }, ct);
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default)
    {
        return await _context.UserNotifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken ct = default)
    {
        var notification = await _context.UserNotifications.FindAsync(new object[] { notificationId }, ct);
        notification?.MarkAsRead();
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken ct = default)
    {
        var unreadNotifications = await _context.UserNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
        }
    }

    public async Task UpdateUserNotificationAsync(UserNotification notification, CancellationToken ct = default)
    {
        _context.UserNotifications.Update(notification);
        await Task.CompletedTask;
    }
}
