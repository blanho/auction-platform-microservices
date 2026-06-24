using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Application.Mappers;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.AuditLogs;

public record GetAuditLogByIdQuery(Guid Id) : IRequest<AuditLogDto?>;

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

public record GetEntityAuditHistoryQuery(string EntityType, Guid EntityId) : IRequest<List<AuditLogDto>>;

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

public record GetPagedAuditLogsQuery(AuditLogQueryParams QueryParams) : IRequest<PaginatedResult<AuditLogDto>>;

public class GetPagedAuditLogsQueryHandler : IRequestHandler<GetPagedAuditLogsQuery, PaginatedResult<AuditLogDto>>
{
    private readonly IAuditLogRepository _repository;
    public GetPagedAuditLogsQueryHandler(IAuditLogRepository repository) => _repository = repository;

    public async Task<PaginatedResult<AuditLogDto>> Handle(GetPagedAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await _repository.GetPagedAsync(request.QueryParams, cancellationToken);
        var dtos = pagedResult.Items.ToDtoList();

        return new PaginatedResult<AuditLogDto>(
            dtos,
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize);
    }
}
