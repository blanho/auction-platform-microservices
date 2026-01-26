using BuildingBlocks.Application.Abstractions;
using Notification.Application.DTOs;

namespace Notification.Application.Interfaces;

public interface INotificationRecordService
{
    Task<NotificationRecordDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PaginatedResult<NotificationRecordDto>> GetPagedAsync(NotificationRecordFilterDto filter, CancellationToken ct = default);
    Task<List<NotificationRecordDto>> GetByUserIdAsync(Guid userId, int limit = 50, CancellationToken ct = default);
    Task<NotificationRecordStatsDto> GetStatsAsync(CancellationToken ct = default);
}
