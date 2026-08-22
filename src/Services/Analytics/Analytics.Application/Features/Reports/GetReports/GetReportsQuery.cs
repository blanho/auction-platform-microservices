using MediatR;
using Analytics.Application.DTOs;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.Reports.GetReports;

public record GetReportsQuery(ReportQueryParams QueryParams) : IRequest<Result<PaginatedResult<ReportDto>>>;
