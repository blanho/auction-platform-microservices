using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Application.Mappers;

namespace Analytics.Application.Features.AuditLogs.GetEntityAuditHistory;

public class GetEntityAuditHistoryQueryHandler : IRequestHandler<GetEntityAuditHistoryQuery, List<AuditLogDto>>
{
    private readonly IAuditLogRepository _repository;
    public GetEntityAuditHistoryQueryHandler(IAuditLogRepository repository) => _repository = repository;

    public async Task<List<AuditLogDto>> Handle(GetEntityAuditHistoryQuery request, CancellationToken cancellationToken)
    {
        var entities = await _repository.GetByEntityAsync(request.EntityType, request.EntityId, cancellationToken);
        return entities.ToDtoList();
    }
}
