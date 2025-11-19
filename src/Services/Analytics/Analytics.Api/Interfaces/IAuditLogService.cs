using Analytics.Api.Models;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Api.Interfaces;

public interface IAuditLogService
{
    Task<AuditLogDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<List<AuditLogDto>> GetEntityAuditHistoryAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);
    
    Task<PaginatedResult<AuditLogDto>> GetPagedAuditLogsAsync(
        AuditLogQueryParams queryParams,
        CancellationToken cancellationToken = default);
}
