using MediatR;
using Analytics.Application.DTOs;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.Reports.UpdateReportStatus;

public record UpdateReportStatusCommand(Guid Id, UpdateReportStatusDto Dto, string ResolvedBy) : IRequest<Result>;
