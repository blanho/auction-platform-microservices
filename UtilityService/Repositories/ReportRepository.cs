using Microsoft.EntityFrameworkCore;
using UtilityService.Data;
using UtilityService.Domain.Entities;
using UtilityService.Interfaces;

namespace UtilityService.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly UtilityDbContext _context;

    public ReportRepository(UtilityDbContext context)
    {
        _context = context;
    }

    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reports.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<List<Report>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reports.ToListAsync(cancellationToken);
    }

    public async Task<(List<Report> Reports, int TotalCount)> GetPagedAsync(
        ReportStatus? status,
        ReportType? type,
        ReportPriority? priority,
        string? reportedUsername,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Reports.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(r => r.Type == type.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(r => r.Priority == priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(reportedUsername))
        {
            query = query.Where(r => r.ReportedUsername.ToLower().Contains(reportedUsername.ToLower()));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var reports = await query
            .OrderByDescending(r => r.Priority)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (reports, totalCount);
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

    public async Task<int> GetCountByStatusAsync(ReportStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Reports.CountAsync(r => r.Status == status, cancellationToken);
    }

    public async Task<int> GetCountByPriorityAsync(ReportPriority priority, CancellationToken cancellationToken = default)
    {
        return await _context.Reports.CountAsync(r => r.Priority == priority && r.Status != ReportStatus.Resolved, cancellationToken);
    }
}
