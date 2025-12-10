using Common.Audit.Enums;
using UtilityService.DTOs;

namespace UtilityService.Interfaces;

public interface IAuditLogService
{
    Task<AuditLogDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<List<AuditLogDto>> GetEntityAuditHistoryAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);
    
    Task<PagedAuditLogsDto> GetPagedAuditLogsAsync(
        AuditLogQueryParams queryParams,
        CancellationToken cancellationToken = default);
}
