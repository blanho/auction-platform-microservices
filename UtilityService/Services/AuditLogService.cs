using System.Text.Json;
using UtilityService.Domain.Entities;
using UtilityService.DTOs;
using UtilityService.Interfaces;

namespace UtilityService.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AuditLogDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.AuditLogs.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
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

    public async Task<PagedAuditLogsDto> GetPagedAuditLogsAsync(
        AuditLogQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.AuditLogs.GetPagedAsync(
            queryParams.Page,
            queryParams.PageSize,
            queryParams.EntityId,
            queryParams.EntityType,
            queryParams.UserId,
            queryParams.ServiceName,
            queryParams.Action,
            queryParams.FromDate,
            queryParams.ToDate,
            cancellationToken);

        var dtos = items.Select(MapToDto).ToList();

        return new PagedAuditLogsDto
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        };
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
