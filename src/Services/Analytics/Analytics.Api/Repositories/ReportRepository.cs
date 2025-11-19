using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Analytics.Api.Data;
using Analytics.Api.Entities;
using Analytics.Api.Enums;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using BuildingBlocks.Application.Abstractions;

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

    public ReportRepository(AnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reports.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<PaginatedResult<Report>> GetPagedAsync(
        ReportQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(queryParams).AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var reports = await query
            .OrderByDescending(r => r.Priority)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Report>(reports, totalCount, queryParams.Page, queryParams.PageSize);
    }

    private IQueryable<Report> BuildFilteredQuery(ReportQueryParams queryParams)
    {
        var query = _context.Reports.AsQueryable();

        if (queryParams.Status.HasValue)
            query = query.Where(r => r.Status == queryParams.Status.Value);

        if (queryParams.Type.HasValue)
            query = query.Where(r => r.Type == queryParams.Type.Value);

        if (queryParams.Priority.HasValue)
            query = query.Where(r => r.Priority == queryParams.Priority.Value);

        if (!string.IsNullOrWhiteSpace(queryParams.ReportedUsername))
            query = query.Where(r => r.ReportedUsername.ToLower().Contains(queryParams.ReportedUsername.ToLower()));

        return query;
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
            .CountAsync(r => r.Status == ReportStatus.Pending || r.Status == ReportStatus.UnderReview, cancellationToken);
    }
}
