using Common.Contracts.Events;
using Analytics.Api.Entities;
using Analytics.Api.Models;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Api.Interfaces;

public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<AuditLog>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);
    
    Task<PaginatedResult<AuditLog>> GetPagedAsync(
        AuditLogQueryParams queryParams,
        CancellationToken cancellationToken = default);
    
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    
    Task<List<AuditLog>> GetLogsOlderThanAsync(DateTimeOffset cutoffDate, CancellationToken cancellationToken = default);
    
    Task<List<AuditLog>> GetRecentAsync(int limit, CancellationToken cancellationToken = default);
}
