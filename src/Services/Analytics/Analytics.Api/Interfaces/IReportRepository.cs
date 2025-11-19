using Analytics.Api.Entities;
using Analytics.Api.Models;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Api.Interfaces;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Report>> GetPagedAsync(
        ReportQueryParams queryParams,
        CancellationToken cancellationToken = default);
    Task<Report> AddAsync(Report report, CancellationToken cancellationToken = default);
    void Update(Report report);
    void Delete(Report report);
    Task<ReportStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
    Task<List<Report>> GetReportsForEscalationAsync(
        TimeSpan unreviewedThreshold,
        CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(List<Report> reports, CancellationToken cancellationToken = default);
    Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default);
}
