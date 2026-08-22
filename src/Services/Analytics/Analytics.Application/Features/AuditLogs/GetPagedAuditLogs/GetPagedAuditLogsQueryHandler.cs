using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Application.Mappers;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.AuditLogs.GetPagedAuditLogs;

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
