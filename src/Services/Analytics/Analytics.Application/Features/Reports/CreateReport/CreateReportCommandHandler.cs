using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Application.Mappers;
using Analytics.Domain.Entities;
using Analytics.Domain.Enums;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Features.Reports.CreateReport;

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
