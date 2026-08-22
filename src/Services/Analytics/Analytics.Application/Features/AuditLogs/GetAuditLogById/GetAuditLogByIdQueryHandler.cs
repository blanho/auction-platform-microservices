using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Application.Mappers;

namespace Analytics.Application.Features.AuditLogs.GetAuditLogById;

public class GetAuditLogByIdQueryHandler : IRequestHandler<GetAuditLogByIdQuery, AuditLogDto?>
{
    private readonly IAuditLogRepository _repository;

    public GetAuditLogByIdQueryHandler(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<AuditLogDto?> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return entity?.ToDto();
    }
}
