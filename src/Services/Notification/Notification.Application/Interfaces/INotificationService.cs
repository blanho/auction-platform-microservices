#nullable enable
using BuildingBlocks.Application.Abstractions;
using Notification.Application.DTOs;

namespace Notification.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);
    Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, CancellationToken cancellationToken = default);
    Task<NotificationSummaryDto> GetNotificationSummaryAsync(string userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
    Task ArchiveNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task DeleteNotificationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<NotificationDto>> GetAllNotificationsAsync(int page, int pageSize, string? userId, string? type, string? status, CancellationToken cancellationToken = default);
    Task BroadcastNotificationAsync(BroadcastNotificationDto dto, CancellationToken cancellationToken = default);
    Task<NotificationStatsDto> GetNotificationStatsAsync(CancellationToken cancellationToken = default);
    Task<NotificationPreferenceDto> GetPreferencesAsync(string userId, CancellationToken cancellationToken = default);
    Task<NotificationPreferenceDto> UpdatePreferencesAsync(string userId, UpdateNotificationPreferenceDto dto, CancellationToken cancellationToken = default);
}
