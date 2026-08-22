using MediatR;
using Analytics.Application.DTOs;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.AuditLogs.GetPagedAuditLogs;

public record GetPagedAuditLogsQuery(AuditLogQueryParams QueryParams) : IRequest<PaginatedResult<AuditLogDto>>;
