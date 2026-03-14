using System.Text.Json;
using Analytics.Api.Entities;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using BuildingBlocks.Application.Abstractions;
using IUnitOfWork = Analytics.Api.Interfaces.IUnitOfWork;

namespace Analytics.Api.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AuditLogDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.AuditLogs.GetByIdAsync(id, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<List<AuditLogDto>> GetEntityAuditHistoryAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.AuditLogs.GetByEntityAsync(
            entityType,
            entityId,
            cancellationToken);

        return entities.ToDtoList();
    }

    public async Task<PaginatedResult<AuditLogDto>> GetPagedAuditLogsAsync(
        AuditLogQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _unitOfWork.AuditLogs.GetPagedAsync(
            queryParams,
            cancellationToken);

        var dtos = pagedResult.Items.ToDtoList();

        return new PaginatedResult<AuditLogDto>(
            dtos,
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize);
    }
}
