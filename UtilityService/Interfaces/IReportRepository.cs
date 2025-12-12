using UtilityService.Domain.Entities;

namespace UtilityService.Interfaces;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Report>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(List<Report> Reports, int TotalCount)> GetPagedAsync(
        ReportStatus? status,
        ReportType? type,
        ReportPriority? priority,
        string? reportedUsername,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<Report> AddAsync(Report report, CancellationToken cancellationToken = default);
    void Update(Report report);
    void Delete(Report report);
    Task<int> GetCountByStatusAsync(ReportStatus status, CancellationToken cancellationToken = default);
    Task<int> GetCountByPriorityAsync(ReportPriority priority, CancellationToken cancellationToken = default);
    Task<List<Report>> GetReportsForEscalationAsync(
        TimeSpan unreviewedThreshold,
        int maxEscalations,
        CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(List<Report> reports, CancellationToken cancellationToken = default);
}
