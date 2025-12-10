using Common.Audit.Enums;
using Microsoft.EntityFrameworkCore;
using UtilityService.Data;
using UtilityService.Domain.Entities;
using UtilityService.Interfaces;

namespace UtilityService.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly UtilityDbContext _context;

    public AuditLogRepository(UtilityDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Guid? entityId = null,
        string? entityType = null,
        Guid? userId = null,
        string? serviceName = null,
        AuditAction? action = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (entityId.HasValue)
            query = query.Where(x => x.EntityId == entityId.Value);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(x => x.EntityType == entityType);

        if (userId.HasValue)
            query = query.Where(x => x.UserId == userId.Value);

        if (!string.IsNullOrEmpty(serviceName))
            query = query.Where(x => x.ServiceName == serviceName);

        if (action.HasValue)
            query = query.Where(x => x.Action == action.Value);

        if (fromDate.HasValue)
            query = query.Where(x => x.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(x => x.Timestamp <= toDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs.CountAsync(cancellationToken);
    }
}
