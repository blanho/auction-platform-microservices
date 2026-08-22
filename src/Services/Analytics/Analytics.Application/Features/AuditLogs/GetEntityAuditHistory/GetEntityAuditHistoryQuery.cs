using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Features.AuditLogs.GetEntityAuditHistory;

public record GetEntityAuditHistoryQuery(string EntityType, Guid EntityId) : IRequest<List<AuditLogDto>>;
