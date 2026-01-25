using Analytics.Api.Entities;
using Analytics.Api.Enums;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Exceptions;

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

    public async Task<PaginatedResult<ReportDto>> GetReportsAsync(
        ReportQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _reportRepository.GetPagedAsync(
            queryParams,
            cancellationToken);

        var dtos = pagedResult.Items.ToDtoList();
        
        return new PaginatedResult<ReportDto>(
            dtos,
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize
        );
    }

    public async Task<ReportDto?> GetReportByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _reportRepository.GetByIdAsync(id, cancellationToken);
        return report?.ToDto();
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

        return report.ToDto();
    }

    public async Task UpdateReportStatusAsync(
        Guid id,
        UpdateReportStatusDto dto,
        string resolvedBy,
        CancellationToken cancellationToken = default)
    {
        var report = await _reportRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Report not found");

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
            ?? throw new NotFoundException("Report not found");

        _reportRepository.Delete(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Report {ReportId} deleted", id);
    }

    public async Task<ReportStatsDto> GetReportStatsAsync(CancellationToken cancellationToken = default)
    {
        return await _reportRepository.GetStatsAsync(cancellationToken);
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
