using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Application.Mappers;
using Analytics.Application.Errors;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.Reports.GetReportById;

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
