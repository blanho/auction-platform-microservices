using MediatR;
using Analytics.Application.Interfaces;
using Analytics.Application.Errors;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Features.Reports.DeleteReport;

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
