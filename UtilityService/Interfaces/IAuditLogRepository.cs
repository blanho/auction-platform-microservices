using Common.Audit.Enums;
using UtilityService.Domain.Entities;

namespace UtilityService.Interfaces;

public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<AuditLog>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);
    
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Guid? entityId = null,
        string? entityType = null,
        Guid? userId = null,
        string? serviceName = null,
        AuditAction? action = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default);
    
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    
    Task<List<AuditLog>> GetLogsOlderThanAsync(DateTimeOffset cutoffDate, CancellationToken cancellationToken = default);
}
