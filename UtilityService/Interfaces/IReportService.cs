using UtilityService.DTOs;

namespace UtilityService.Interfaces;

public interface IReportService
{
    Task<PagedReportsDto> GetReportsAsync(ReportQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<ReportDto> GetReportByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ReportDto> CreateReportAsync(string reporterUsername, CreateReportDto dto, CancellationToken cancellationToken = default);
    Task UpdateReportStatusAsync(Guid id, UpdateReportStatusDto dto, string resolvedBy, CancellationToken cancellationToken = default);
    Task DeleteReportAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ReportStatsDto> GetReportStatsAsync(CancellationToken cancellationToken = default);
}
