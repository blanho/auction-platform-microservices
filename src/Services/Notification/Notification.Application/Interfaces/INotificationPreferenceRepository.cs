#nullable enable
using Notification.Domain.Entities;

namespace Notification.Application.Interfaces;

public interface INotificationPreferenceRepository
{
    Task<NotificationPreference?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<NotificationPreference> CreateAsync(NotificationPreference preference, CancellationToken cancellationToken = default);
    Task UpdateAsync(NotificationPreference preference, CancellationToken cancellationToken = default);
}
