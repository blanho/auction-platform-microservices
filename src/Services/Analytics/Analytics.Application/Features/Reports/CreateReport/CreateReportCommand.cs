using MediatR;
using Analytics.Application.DTOs;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.Reports.CreateReport;

public record CreateReportCommand(string ReporterUsername, CreateReportDto Dto) : IRequest<Result<ReportDto>>;
