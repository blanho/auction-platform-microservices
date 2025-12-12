using Common.Core.Interfaces;
using Common.Core.Constants;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
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
    }
}
