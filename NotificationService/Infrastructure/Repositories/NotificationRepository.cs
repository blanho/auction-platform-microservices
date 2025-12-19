using Common.Core.Interfaces;
using Common.Core.Constants;
using Microsoft.EntityFrameworkCore;
using LegacyRepo = NotificationService.Application.Interfaces;
using PortRepo = NotificationService.Application.Ports;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Repositories
{
    public class NotificationRepository : LegacyRepo.INotificationRepository, PortRepo.INotificationRepository
    {
        private readonly NotificationDbContext _context;
        private readonly IDateTimeProvider _dateTime;

        public NotificationRepository(NotificationDbContext context, IDateTimeProvider dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<List<Notification>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Notification> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(x => !x.IsDeleted)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<Notification> CreateAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            notification.CreatedAt = _dateTime.UtcNow;
            notification.CreatedBy = SystemGuids.System;
            notification.IsDeleted = false;

            await _context.Notifications.AddAsync(notification, cancellationToken);
            return notification;
        }

        public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            notification.UpdatedAt = _dateTime.UtcNow;
            notification.UpdatedBy = SystemGuids.System;
            _context.Notifications.Update(notification);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var notification = await GetByIdAsync(id, cancellationToken);
            if (notification != null)
            {
                notification.IsDeleted = true;
                notification.DeletedAt = _dateTime.UtcNow;
                notification.DeletedBy = SystemGuids.System;
                _context.Notifications.Update(notification);
            }
        }

        public async Task<List<Notification>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(x => !x.IsDeleted && x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Notification>> GetUnreadByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(x => !x.IsDeleted && x.UserId == userId && x.Status == NotificationStatus.Unread)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetUnreadCountByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(x => !x.IsDeleted && x.UserId == userId && x.Status == NotificationStatus.Unread)
                .CountAsync(cancellationToken);
        }

        public async Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var notification = await GetByIdAsync(id, cancellationToken);
            if (notification != null && notification.Status == NotificationStatus.Unread)
            {
                notification.Status = NotificationStatus.Read;
                notification.ReadAt = _dateTime.UtcNow;
                notification.UpdatedAt = _dateTime.UtcNow;
                notification.UpdatedBy = SystemGuids.System;
                _context.Notifications.Update(notification);
            }
        }

        public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
        {
            var unreadNotifications = await GetUnreadByUserIdAsync(userId, cancellationToken);
            var now = _dateTime.UtcNow;

            foreach (var notification in unreadNotifications)
            {
                notification.Status = NotificationStatus.Read;
                notification.ReadAt = now;
                notification.UpdatedAt = now;
                notification.UpdatedBy = SystemGuids.System;
            }

            _context.Notifications.UpdateRange(unreadNotifications);
        }

        public async Task<List<Notification>> GetOldReadNotificationsAsync(int retentionDays, CancellationToken cancellationToken = default)
        {
            var cutoffDate = _dateTime.UtcNow.AddDays(-retentionDays);
            
            return await _context.Notifications
                .Where(x => !x.IsDeleted && 
                           x.Status == NotificationStatus.Read && 
                           x.ReadAt.HasValue && 
                           x.ReadAt.Value < cutoffDate)
                .OrderBy(x => x.ReadAt)
                .ToListAsync(cancellationToken);
        }

        public async Task DeleteRangeAsync(List<Notification> notifications, CancellationToken cancellationToken = default)
        {
            var now = _dateTime.UtcNow;
            
            foreach (var notification in notifications)
            {
                notification.IsDeleted = true;
                notification.DeletedAt = now;
                notification.DeletedBy = SystemGuids.System;
            }
            
            _context.Notifications.UpdateRange(notifications);
        }

        public async Task<Notification?> GetByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(x => !x.IsDeleted && x.IdempotencyKey == key)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<(List<Notification> Items, int TotalCount)> GetPagedAsync(
            string userId,
            NotificationStatus? status,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Notifications
                .Where(n => !n.IsDeleted && n.UserId == userId && n.Status != NotificationStatus.Dismissed);

            if (status.HasValue)
            {
                query = query.Where(n => n.Status == status.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<List<Notification>> GetPendingAsync(int limit, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(n => !n.IsDeleted && n.Status == NotificationStatus.Pending)
                .OrderBy(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .CountAsync(n => !n.IsDeleted && n.UserId == userId && n.Status == NotificationStatus.Unread, cancellationToken);
        }

        async Task Application.Ports.INotificationRepository.CreateAsync(Notification notification, CancellationToken cancellationToken)
        {
            notification.CreatedAt = _dateTime.UtcNow;
            notification.CreatedBy = SystemGuids.System;
            notification.IsDeleted = false;
            await _context.Notifications.AddAsync(notification, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        async Task Application.Ports.INotificationRepository.UpdateAsync(Notification notification, CancellationToken cancellationToken)
        {
            notification.UpdatedAt = _dateTime.UtcNow;
            notification.UpdatedBy = SystemGuids.System;
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync(cancellationToken);
        }

        async Task Application.Ports.INotificationRepository.DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var notification = await GetByIdAsync(id, cancellationToken);
            if (notification != null)
            {
                notification.IsDeleted = true;
                notification.DeletedAt = _dateTime.UtcNow;
                notification.DeletedBy = SystemGuids.System;
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        async Task<int> Application.Ports.INotificationRepository.MarkAllAsReadAsync(string userId, CancellationToken cancellationToken)
        {
            return await _context.Notifications
                .Where(n => !n.IsDeleted && n.UserId == userId && n.Status == NotificationStatus.Unread)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(n => n.Status, NotificationStatus.Read)
                          .SetProperty(n => n.ReadAt, _dateTime.UtcNow),
                    cancellationToken);
        }
    }
}
