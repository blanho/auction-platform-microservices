using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Features.AuditLogs.GetAuditLogById;

public record GetAuditLogByIdQuery(Guid Id) : IRequest<AuditLogDto?>;
