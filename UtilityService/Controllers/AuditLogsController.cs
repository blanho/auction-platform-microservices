using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UtilityService.Data;
using UtilityService.DTOs;

namespace UtilityService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly UtilityDbContext _context;

    public AuditLogsController(UtilityDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedAuditLogsDto>> GetAuditLogs(
        [FromQuery] AuditLogQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (queryParams.EntityId.HasValue)
            query = query.Where(x => x.EntityId == queryParams.EntityId.Value);

        if (!string.IsNullOrEmpty(queryParams.EntityType))
            query = query.Where(x => x.EntityType == queryParams.EntityType);

        if (queryParams.UserId.HasValue)
            query = query.Where(x => x.UserId == queryParams.UserId.Value);

        if (!string.IsNullOrEmpty(queryParams.ServiceName))
            query = query.Where(x => x.ServiceName == queryParams.ServiceName);

        if (queryParams.Action.HasValue)
            query = query.Where(x => x.Action == queryParams.Action.Value);

        if (queryParams.FromDate.HasValue)
            query = query.Where(x => x.Timestamp >= queryParams.FromDate.Value);

        if (queryParams.ToDate.HasValue)
            query = query.Where(x => x.Timestamp <= queryParams.ToDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var entities = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync(cancellationToken);

        var items = entities.Select(MapToDto).ToList();

        return Ok(new PagedAuditLogsDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<List<AuditLogDto>>> GetEntityAuditHistory(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var entities = await _context.AuditLogs
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync(cancellationToken);

        var logs = entities.Select(MapToDto).ToList();

        return Ok(logs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuditLogDto>> GetAuditLog(
        Guid id,
        CancellationToken cancellationToken)
    {
        var entity = await _context.AuditLogs
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
            return NotFound();

        return Ok(MapToDto(entity));
    }

    private static AuditLogDto MapToDto(Domain.Entities.AuditLog entity)
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
