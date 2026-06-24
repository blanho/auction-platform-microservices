using MediatR;
using Analytics.Domain.Entities;
using Analytics.Domain.Enums;
using Analytics.Domain.Errors;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Application.Mappers;

using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Features.Reports;

public record GetReportsQuery(ReportQueryParams QueryParams) : IRequest<Result<PaginatedResult<ReportDto>>>;
public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, Result<PaginatedResult<ReportDto>>>
{
    private readonly IReportRepository _reportRepository;

    public GetReportsQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<PaginatedResult<ReportDto>>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await _reportRepository.GetPagedAsync(request.QueryParams, cancellationToken);
        var dtos = pagedResult.Items.ToDtoList();

        return Result<PaginatedResult<ReportDto>>.Success(new PaginatedResult<ReportDto>(
            dtos,
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize
        ));
    }
}

public record GetReportByIdQuery(Guid Id) : IRequest<Result<ReportDto>>;
public class GetReportByIdQueryHandler : IRequestHandler<GetReportByIdQuery, Result<ReportDto>>
{
    private readonly IReportRepository _reportRepository;

    public GetReportByIdQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<ReportDto>> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdAsync(request.Id, cancellationToken);
        if (report == null)
            return Result.Failure<ReportDto>(AnalyticsErrors.Report.NotFound);

        return Result<ReportDto>.Success(report.ToDto());
    }
}

public record CreateReportCommand(string ReporterUsername, CreateReportDto Dto) : IRequest<Result<ReportDto>>;
public class CreateReportCommandHandler : IRequestHandler<CreateReportCommand, Result<ReportDto>>
{
    private readonly IReportRepository _reportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateReportCommandHandler> _logger;

    public CreateReportCommandHandler(IReportRepository reportRepository, IUnitOfWork unitOfWork, ILogger<CreateReportCommandHandler> logger)
    {
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ReportDto>> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var report = new Report
        {
            Id = Guid.NewGuid(),
            ReporterUsername = request.ReporterUsername,
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

        _logger.LogInformation("Report {ReportId} created by {Reporter} against {Reported}", report.Id, request.ReporterUsername, dto.ReportedUsername);

        return Result<ReportDto>.Success(report.ToDto());
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

public record UpdateReportStatusCommand(Guid Id, UpdateReportStatusDto Dto, string ResolvedBy) : IRequest<Result>;
public class UpdateReportStatusCommandHandler : IRequestHandler<UpdateReportStatusCommand, Result>
{
    private readonly IReportRepository _reportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateReportStatusCommandHandler> _logger;

    public UpdateReportStatusCommandHandler(IReportRepository reportRepository, IUnitOfWork unitOfWork, ILogger<UpdateReportStatusCommandHandler> logger)
    {
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateReportStatusCommand request, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdAsync(request.Id, cancellationToken);
        if (report == null)
            return Result.Failure(AnalyticsErrors.Report.NotFound);

        report.Status = request.Dto.Status;
        report.Resolution = request.Dto.Resolution;
        report.UpdatedAt = DateTimeOffset.UtcNow;

        if (request.Dto.Status is ReportStatus.Resolved or ReportStatus.Dismissed)
        {
            report.ResolvedBy = request.ResolvedBy;
            report.ResolvedAt = DateTimeOffset.UtcNow;
        }

        _reportRepository.Update(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Report {ReportId} status updated to {Status} by {ResolvedBy}", request.Id, request.Dto.Status, request.ResolvedBy);

        return Result.Success();
    }
}

public record DeleteReportCommand(Guid Id) : IRequest<Result>;
public class DeleteReportCommandHandler : IRequestHandler<DeleteReportCommand, Result>
{
    private readonly IReportRepository _reportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteReportCommandHandler> _logger;

    public DeleteReportCommandHandler(IReportRepository reportRepository, IUnitOfWork unitOfWork, ILogger<DeleteReportCommandHandler> logger)
    {
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdAsync(request.Id, cancellationToken);
        if (report == null)
            return Result.Failure(AnalyticsErrors.Report.NotFound);

        _reportRepository.Delete(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Report {ReportId} deleted", request.Id);

        return Result.Success();
    }
}

public record GetReportStatsQuery() : IRequest<Result<ReportStatsDto>>;
public class GetReportStatsQueryHandler : IRequestHandler<GetReportStatsQuery, Result<ReportStatsDto>>
{
    private readonly IReportRepository _reportRepository;

    public GetReportStatsQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<ReportStatsDto>> Handle(GetReportStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _reportRepository.GetStatsAsync(cancellationToken);
        return Result<ReportStatsDto>.Success(stats);
    }
}
