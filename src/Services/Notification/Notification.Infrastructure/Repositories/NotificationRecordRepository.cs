using BuildingBlocks.Application.Abstractions;
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

    public async Task<NotificationRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Records
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<PaginatedResult<NotificationRecord>> GetPagedAsync(
        Guid? userId = null,
        string? channel = null,
        string? status = null,
        string? templateKey = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _context.Records.AsNoTracking().AsQueryable();

        if (userId.HasValue)
            query = query.Where(r => r.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(channel))
            query = query.Where(r => r.Channel == channel);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<NotificationRecordStatus>(status, true, out var statusEnum))
            query = query.Where(r => r.Status == statusEnum);

        if (!string.IsNullOrWhiteSpace(templateKey))
            query = query.Where(r => r.TemplateKey == templateKey);

        if (fromDate.HasValue)
            query = query.Where(r => r.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(r => r.CreatedAt <= toDate.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedResult<NotificationRecord>(items, totalCount, page, pageSize);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken ct = default)
    {
        return await _context.Records.CountAsync(ct);
    }

    public async Task<int> GetCountByStatusAsync(NotificationRecordStatus status, CancellationToken ct = default)
    {
        return await _context.Records.CountAsync(r => r.Status == status, ct);
    }

    public async Task<Dictionary<string, int>> GetCountByChannelAsync(CancellationToken ct = default)
    {
        return await _context.Records
            .GroupBy(r => r.Channel)
            .Select(g => new { Channel = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Channel, x => x.Count, ct);
    }

    public async Task<Dictionary<string, int>> GetCountByTemplateAsync(CancellationToken ct = default)
    {
        return await _context.Records
            .GroupBy(r => r.TemplateKey)
            .Select(g => new { TemplateKey = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TemplateKey, x => x.Count, ct);
    }
}
