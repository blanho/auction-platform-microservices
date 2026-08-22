using MediatR;
using Analytics.Application.Interfaces;
using Analytics.Application.Errors;
using Analytics.Domain.Enums;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Features.Reports.UpdateReportStatus;

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
