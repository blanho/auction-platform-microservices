using BuildingBlocks.Application.Abstractions;
using Notification.Application.DTOs;
using Notification.Application.Filtering;

namespace Notification.Application.Interfaces;

public interface INotificationRecordService
{
    Task<NotificationRecordDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PaginatedResult<NotificationRecordDto>> GetPagedAsync(NotificationRecordFilterDto filter, CancellationToken ct = default);
    Task<PaginatedResult<NotificationRecordDto>> GetByUserIdAsync(NotificationRecordQueryParams queryParams, CancellationToken ct = default);
    Task<NotificationRecordStatsDto> GetStatsAsync(CancellationToken ct = default);
}
