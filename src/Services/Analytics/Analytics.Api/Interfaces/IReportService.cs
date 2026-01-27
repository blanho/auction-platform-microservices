using Analytics.Api.Models;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Api.Interfaces;

public interface IReportService
{
    Task<Result<PaginatedResult<ReportDto>>> GetReportsAsync(ReportQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<Result<ReportDto>> GetReportByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ReportDto>> CreateReportAsync(string reporterUsername, CreateReportDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateReportStatusAsync(Guid id, UpdateReportStatusDto dto, string resolvedBy, CancellationToken cancellationToken = default);
    Task<Result> DeleteReportAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ReportStatsDto>> GetReportStatsAsync(CancellationToken cancellationToken = default);
}
