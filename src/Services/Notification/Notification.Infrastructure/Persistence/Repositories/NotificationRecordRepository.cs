using System.Linq.Expressions;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Filtering;
using BuildingBlocks.Application.Paging;
using Microsoft.EntityFrameworkCore;
using Notification.Application.DTOs;
using Notification.Application.Filtering;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Persistence.Repositories;

public class NotificationRecordRepository : INotificationRecordRepository
{
    private readonly NotificationDbContext _context;

    private static readonly Dictionary<string, Expression<Func<NotificationRecord, object>>> SortMap = 
        new(StringComparer.OrdinalIgnoreCase)
    {
        ["createdat"] = r => r.CreatedAt,
        ["sentat"] = r => r.SentAt!,
        ["channel"] = r => r.Channel,
        ["status"] = r => r.Status,
        ["templatekey"] = r => r.TemplateKey
    };

    public NotificationRecordRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task AddRecordAsync(NotificationRecord record, CancellationToken ct = default)
    {
        await _context.Records.AddAsync(record, ct);
    }

    public async Task<PaginatedResult<NotificationRecord>> GetRecordsByUserIdAsync(NotificationRecordQueryParams queryParams, CancellationToken ct = default)
    {
        var query = _context.Records.AsNoTracking();
        
        if (queryParams.Filter != null)
        {
            query = queryParams.Filter.Apply(query);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .ApplySorting(queryParams, SortMap, r => r.CreatedAt)
            .ApplyPaging(queryParams)
            .ToListAsync(ct);

        return new PaginatedResult<NotificationRecord>(items, totalCount, queryParams.Page, queryParams.PageSize);
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
        NotificationRecordFilterDto queryParams,
        CancellationToken ct = default)
    {
        var filter = queryParams.Filter;
        
        var filterBuilder = FilterBuilder<NotificationRecord>.Create()
            .WhenHasValue(filter.UserId, r => r.UserId == filter.UserId!.Value)
            .WhenNotEmpty(filter.Channel, r => r.Channel == filter.Channel)
            .WhenNotEmpty(filter.Status, r => r.Status.ToString() == filter.Status)
            .WhenNotEmpty(filter.TemplateKey, r => r.TemplateKey == filter.TemplateKey)
            .WhenHasValue(filter.FromDate, r => r.CreatedAt >= filter.FromDate!.Value)
            .WhenHasValue(filter.ToDate, r => r.CreatedAt <= filter.ToDate!.Value);

        var query = _context.Records
            .AsNoTracking()
            .ApplyFiltering(filterBuilder)
            .ApplySorting(queryParams, SortMap, r => r.CreatedAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .ApplyPaging(queryParams)
            .ToListAsync(ct);

        return new PaginatedResult<NotificationRecord>(items, totalCount, queryParams.Page, queryParams.PageSize);
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
