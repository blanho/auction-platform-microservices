using Common.Audit.Enums;
using AnalyticsService.Domain.Entities;
using AnalyticsService.DTOs;

namespace AnalyticsService.Interfaces;

public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<AuditLog>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);
    
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(
        AuditLogQueryParams queryParams,
        CancellationToken cancellationToken = default);
    
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    
    Task<List<AuditLog>> GetLogsOlderThanAsync(DateTimeOffset cutoffDate, CancellationToken cancellationToken = default);
    
    Task<List<AuditLog>> GetRecentAsync(int limit, CancellationToken cancellationToken = default);
}
