using MediatR;
using Analytics.Application.DTOs;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.Reports.GetReportById;

public record GetReportByIdQuery(Guid Id) : IRequest<Result<ReportDto>>;
