using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.Reports.GetReportStats;

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
