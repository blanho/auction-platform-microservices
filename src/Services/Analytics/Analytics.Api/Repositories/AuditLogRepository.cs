using System.Linq.Expressions;
using Common.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Analytics.Api.Data;
using Analytics.Api.Entities;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Filtering;
using BuildingBlocks.Application.Paging;

namespace Analytics.Api.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AnalyticsDbContext _context;

    private static readonly Dictionary<string, Expression<Func<AuditLog, object>>> SortMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["timestamp"] = x => x.Timestamp,
        ["entitytype"] = x => x.EntityType,
        ["action"] = x => x.Action,
        ["servicename"] = x => x.ServiceName
    };

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

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .AnyAsync(x => x.Id == id, cancellationToken);
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
        var filter = queryParams.Filter;
        
        var filterBuilder = FilterBuilder<AuditLog>.Create()
            .WhenHasValue(filter.EntityId, x => x.EntityId == filter.EntityId!.Value)
            .WhenNotEmpty(filter.EntityType, x => x.EntityType == filter.EntityType)
            .WhenHasValue(filter.UserId, x => x.UserId == filter.UserId!.Value)
            .WhenNotEmpty(filter.ServiceName, x => x.ServiceName == filter.ServiceName)
            .WhenHasValue(filter.Action, x => x.Action == filter.Action!.Value)
            .WhenHasValue(filter.FromDate, x => x.Timestamp >= filter.FromDate!.Value)
            .WhenHasValue(filter.ToDate, x => x.Timestamp <= filter.ToDate!.Value);

        var query = _context.AuditLogs
            .AsNoTracking()
            .ApplyFiltering(filterBuilder)
            .ApplySorting(queryParams, SortMap, x => x.Timestamp);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplyPaging(queryParams)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<AuditLog>(items, totalCount, queryParams.Page, queryParams.PageSize);
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
