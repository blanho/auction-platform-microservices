using MediatR;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.Reports.DeleteReport;

public record DeleteReportCommand(Guid Id) : IRequest<Result>;
