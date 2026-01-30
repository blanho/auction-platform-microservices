using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Analytics.Api.Data;
using Analytics.Api.Entities;
using Analytics.Api.Enums;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Filtering;
using BuildingBlocks.Application.Paging;

namespace Analytics.Api.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AnalyticsDbContext _context;

    private static readonly Expression<Func<Report, bool>> IsUnresolvedCritical =
        r => r.Priority == ReportPriority.Critical && r.Status != ReportStatus.Resolved;

    private static readonly Expression<Func<Report, bool>> IsUnresolvedHigh =
        r => r.Priority == ReportPriority.High && r.Status != ReportStatus.Resolved;

    private static readonly Expression<Func<Report, bool>> IsPendingForEscalation =
        r => r.Status == ReportStatus.Pending;

    private static readonly Dictionary<string, Expression<Func<Report, object>>> SortMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["priority"] = r => r.Priority,
        ["createdat"] = r => r.CreatedAt,
        ["status"] = r => r.Status,
        ["type"] = r => r.Type,
        ["reportedusername"] = r => r.ReportedUsername
    };

    public ReportRepository(AnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<PaginatedResult<Report>> GetPagedAsync(
        ReportQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var filter = queryParams.Filter;
        
        var filterBuilder = FilterBuilder<Report>.Create()
            .WhenHasValue(filter.Status, r => r.Status == filter.Status!.Value)
            .WhenHasValue(filter.Type, r => r.Type == filter.Type!.Value)
            .WhenHasValue(filter.Priority, r => r.Priority == filter.Priority!.Value)
            .WhenNotEmpty(filter.ReportedUsername, r => r.ReportedUsername.ToLower().Contains(filter.ReportedUsername!.ToLower()));

        var query = _context.Reports
            .AsNoTracking()
            .ApplyFiltering(filterBuilder)
            .ApplySorting(queryParams, SortMap, r => r.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplyPaging(queryParams)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Report>(items, totalCount, queryParams.Page, queryParams.PageSize);
    }

    public async Task<Report> AddAsync(Report report, CancellationToken cancellationToken = default)
    {
        await _context.Reports.AddAsync(report, cancellationToken);
        return report;
    }

    public void Update(Report report)
    {
        _context.Reports.Update(report);
    }

    public void Delete(Report report)
    {
        _context.Reports.Remove(report);
    }

    public async Task<ReportStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var statusCounts = await _context.Reports
            .AsNoTracking()
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var criticalCount = await _context.Reports
            .AsNoTracking()
            .Where(IsUnresolvedCritical)
            .CountAsync(cancellationToken);

        var highPriorityCount = await _context.Reports
            .AsNoTracking()
            .Where(IsUnresolvedHigh)
            .CountAsync(cancellationToken);

        return new ReportStatsDto
        {
            TotalReports = statusCounts.Sum(x => x.Count),
            PendingReports = statusCounts.FirstOrDefault(x => x.Status == ReportStatus.Pending)?.Count ?? 0,
            UnderReviewReports = statusCounts.FirstOrDefault(x => x.Status == ReportStatus.UnderReview)?.Count ?? 0,
            ResolvedReports = statusCounts.FirstOrDefault(x => x.Status == ReportStatus.Resolved)?.Count ?? 0,
            DismissedReports = statusCounts.FirstOrDefault(x => x.Status == ReportStatus.Dismissed)?.Count ?? 0,
            CriticalReports = criticalCount,
            HighPriorityReports = highPriorityCount
        };
    }

    public async Task<List<Report>> GetReportsForEscalationAsync(
        TimeSpan unreviewedThreshold,
        CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTimeOffset.UtcNow - unreviewedThreshold;

        return await _context.Reports
            .AsNoTracking()
            .Where(IsPendingForEscalation)
            .Where(r => r.CreatedAt <= cutoffTime)
            .Where(r => r.EscalatedAt == null || r.EscalatedAt <= cutoffTime)
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(List<Report> reports, CancellationToken cancellationToken = default)
    {
        _context.Reports.UpdateRange(reports);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .AsNoTracking()
            .CountAsync(r => r.Status == ReportStatus.Pending || r.Status == ReportStatus.UnderReview, cancellationToken);
    }
}
