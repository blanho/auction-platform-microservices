using Common.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Analytics.Api.Data;
using Analytics.Api.Entities;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Api.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AnalyticsDbContext _context;

    public AuditLogRepository(AnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResult<AuditLog>> GetPagedAsync(
        AuditLogQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(queryParams).AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<AuditLog>(items, totalCount, queryParams.Page, queryParams.PageSize);
    }

    private IQueryable<AuditLog> BuildFilteredQuery(AuditLogQueryParams queryParams)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (queryParams.EntityId.HasValue)
            query = query.Where(x => x.EntityId == queryParams.EntityId.Value);

        if (!string.IsNullOrEmpty(queryParams.EntityType))
            query = query.Where(x => x.EntityType == queryParams.EntityType);

        if (queryParams.UserId.HasValue)
            query = query.Where(x => x.UserId == queryParams.UserId.Value);

        if (!string.IsNullOrEmpty(queryParams.ServiceName))
            query = query.Where(x => x.ServiceName == queryParams.ServiceName);

        if (queryParams.Action.HasValue)
            query = query.Where(x => x.Action == queryParams.Action.Value);

        if (queryParams.FromDate.HasValue)
            query = query.Where(x => x.Timestamp >= queryParams.FromDate.Value);

        if (queryParams.ToDate.HasValue)
            query = query.Where(x => x.Timestamp <= queryParams.ToDate.Value);

        return query;
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .CountAsync(cancellationToken);
    }

    public async Task<List<AuditLog>> GetLogsOlderThanAsync(DateTimeOffset cutoffDate, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(x => x.Timestamp < cutoffDate)
            .OrderBy(x => x.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AuditLog>> GetRecentAsync(int limit, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.Timestamp)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
