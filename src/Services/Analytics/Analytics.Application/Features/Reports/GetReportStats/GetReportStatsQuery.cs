using MediatR;
using Analytics.Application.DTOs;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.Reports.GetReportStats;

public record GetReportStatsQuery() : IRequest<Result<ReportStatsDto>>;
