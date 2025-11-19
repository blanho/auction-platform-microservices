using System.Text.Json;
using Analytics.Api.Entities;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using BuildingBlocks.Application.Abstractions;

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
        return entity is null ? null : MapToDto(entity);
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

        return entities.Select(MapToDto).ToList();
    }

    public async Task<PaginatedResult<AuditLogDto>> GetPagedAuditLogsAsync(
        AuditLogQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _unitOfWork.AuditLogs.GetPagedAsync(
            queryParams,
            cancellationToken);

        var dtos = pagedResult.Items.Select(MapToDto).ToList();

        return new PaginatedResult<AuditLogDto>(
            dtos,
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize);
    }

    private static AuditLogDto MapToDto(AuditLog entity)
    {
        return new AuditLogDto
        {
            Id = entity.Id,
            EntityId = entity.EntityId,
            EntityType = entity.EntityType,
            Action = entity.Action,
            OldValues = entity.OldValues,
            NewValues = entity.NewValues,
            ChangedProperties = !string.IsNullOrEmpty(entity.ChangedProperties)
                ? JsonSerializer.Deserialize<List<string>>(entity.ChangedProperties)
                : null,
            UserId = entity.UserId,
            Username = entity.Username,
            ServiceName = entity.ServiceName,
            CorrelationId = entity.CorrelationId,
            IpAddress = entity.IpAddress,
            Timestamp = entity.Timestamp
        };
    }
}
