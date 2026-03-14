using Analytics.Api.Entities;
using Analytics.Api.Enums;
using Analytics.Api.Errors;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using BuildingBlocks.Application.Abstractions;
using IUnitOfWork = Analytics.Api.Interfaces.IUnitOfWork;

namespace Analytics.Api.Services;

public sealed class ReportService : IReportService
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

    public async Task<Result<PaginatedResult<ReportDto>>> GetReportsAsync(
        ReportQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _reportRepository.GetPagedAsync(queryParams, cancellationToken);
        var dtos = pagedResult.Items.ToDtoList();
        
        return Result<PaginatedResult<ReportDto>>.Success(new PaginatedResult<ReportDto>(
            dtos,
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize
        ));
    }

    public async Task<Result<ReportDto>> GetReportByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _reportRepository.GetByIdAsync(id, cancellationToken);
        if (report == null)
            return Result.Failure<ReportDto>(AnalyticsErrors.Report.NotFound);

        return Result<ReportDto>.Success(report.ToDto());
    }

    public async Task<Result<ReportDto>> CreateReportAsync(
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

        return Result<ReportDto>.Success(report.ToDto());
    }

    public async Task<Result> UpdateReportStatusAsync(
        Guid id,
        UpdateReportStatusDto dto,
        string resolvedBy,
        CancellationToken cancellationToken = default)
    {
        var report = await _reportRepository.GetByIdAsync(id, cancellationToken);
        if (report == null)
            return Result.Failure(AnalyticsErrors.Report.NotFound);

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

        return Result.Success();
    }

    public async Task<Result> DeleteReportAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _reportRepository.GetByIdAsync(id, cancellationToken);
        if (report == null)
            return Result.Failure(AnalyticsErrors.Report.NotFound);

        _reportRepository.Delete(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Report {ReportId} deleted", id);

        return Result.Success();
    }

    public async Task<Result<ReportStatsDto>> GetReportStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await _reportRepository.GetStatsAsync(cancellationToken);
        return Result<ReportStatsDto>.Success(stats);
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
}
