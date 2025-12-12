using UtilityService.Domain.Entities;
using UtilityService.DTOs;
using UtilityService.Interfaces;

namespace UtilityService.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IReportRepository reportRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReportService> logger)
    {
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedReportsDto> GetReportsAsync(
        ReportQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var (reports, totalCount) = await _reportRepository.GetPagedAsync(
            queryParams.Status,
            queryParams.Type,
            queryParams.Priority,
            queryParams.ReportedUsername,
            queryParams.PageNumber,
            queryParams.PageSize,
            cancellationToken);

        return new PagedReportsDto
        {
            Reports = reports.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize
        };
    }

    public async Task<ReportDto> GetReportByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _reportRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Report not found");

        return MapToDto(report);
    }

    public async Task<ReportDto> CreateReportAsync(
        string reporterUsername,
        CreateReportDto dto,
        CancellationToken cancellationToken = default)
    {
        var report = new Report
        {
            Id = Guid.NewGuid(),
            ReporterUsername = reporterUsername,
            ReportedUsername = dto.ReportedUsername,
            AuctionId = dto.AuctionId,
            Type = dto.Type,
            Priority = DeterminePriority(dto.Type),
            Reason = dto.Reason,
            Description = dto.Description,
            Status = ReportStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _reportRepository.AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Report {ReportId} created by {Reporter} against {Reported}",
            report.Id, reporterUsername, dto.ReportedUsername);

        return MapToDto(report);
    }

    public async Task UpdateReportStatusAsync(
        Guid id,
        UpdateReportStatusDto dto,
        string resolvedBy,
        CancellationToken cancellationToken = default)
    {
        var report = await _reportRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Report not found");

        report.Status = dto.Status;
        report.Resolution = dto.Resolution;
        report.UpdatedAt = DateTimeOffset.UtcNow;

        if (dto.Status is ReportStatus.Resolved or ReportStatus.Dismissed)
        {
            report.ResolvedBy = resolvedBy;
            report.ResolvedAt = DateTimeOffset.UtcNow;
        }

        _reportRepository.Update(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Report {ReportId} status updated to {Status} by {ResolvedBy}",
            id, dto.Status, resolvedBy);
    }

    public async Task DeleteReportAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _reportRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Report not found");

        _reportRepository.Delete(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Report {ReportId} deleted", id);
    }

    public async Task<ReportStatsDto> GetReportStatsAsync(CancellationToken cancellationToken = default)
    {
        var allReports = await _reportRepository.GetAllAsync(cancellationToken);

        return new ReportStatsDto
        {
            TotalReports = allReports.Count,
            PendingReports = allReports.Count(r => r.Status == ReportStatus.Pending),
            UnderReviewReports = allReports.Count(r => r.Status == ReportStatus.UnderReview),
            ResolvedReports = allReports.Count(r => r.Status == ReportStatus.Resolved),
            DismissedReports = allReports.Count(r => r.Status == ReportStatus.Dismissed),
            CriticalReports = allReports.Count(r => r.Priority == ReportPriority.Critical && r.Status != ReportStatus.Resolved),
            HighPriorityReports = allReports.Count(r => r.Priority == ReportPriority.High && r.Status != ReportStatus.Resolved)
        };
    }

    private static ReportPriority DeterminePriority(ReportType type)
    {
        return type switch
        {
            ReportType.Fraud => ReportPriority.Critical,
            ReportType.FakeItem => ReportPriority.High,
            ReportType.NonPayment => ReportPriority.High,
            ReportType.SuspiciousActivity => ReportPriority.High,
            ReportType.Harassment => ReportPriority.Medium,
            ReportType.InappropriateContent => ReportPriority.Medium,
            _ => ReportPriority.Low
        };
    }

    private static ReportDto MapToDto(Report report)
    {
        return new ReportDto
        {
            Id = report.Id,
            ReporterUsername = report.ReporterUsername,
            ReportedUsername = report.ReportedUsername,
            AuctionId = report.AuctionId,
            Type = report.Type.ToString(),
            Priority = report.Priority.ToString(),
            Reason = report.Reason,
            Description = report.Description,
            Status = report.Status.ToString(),
            Resolution = report.Resolution,
            ResolvedBy = report.ResolvedBy,
            ResolvedAt = report.ResolvedAt,
            CreatedAt = report.CreatedAt
        };
    }
}
