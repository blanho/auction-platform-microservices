#nullable enable
using NotificationService.Application.DTOs;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);
    Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, CancellationToken cancellationToken = default);
    Task<NotificationSummaryDto> GetNotificationSummaryAsync(string userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
    Task DeleteNotificationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedNotificationsDto> GetAllNotificationsAsync(int pageNumber, int pageSize, string? userId, string? type, string? status, CancellationToken cancellationToken = default);
    Task BroadcastNotificationAsync(BroadcastNotificationDto dto, CancellationToken cancellationToken = default);
    Task<NotificationStatsDto> GetNotificationStatsAsync(CancellationToken cancellationToken = default);
}
