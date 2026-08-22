using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Application.Mappers;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.Reports.GetReports;

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
