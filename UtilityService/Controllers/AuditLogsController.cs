using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityService.DTOs;
using UtilityService.Interfaces;

namespace UtilityService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedAuditLogsDto>> GetAuditLogs(
        [FromQuery] AuditLogQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        var result = await _auditLogService.GetPagedAuditLogsAsync(queryParams, cancellationToken);
        return Ok(result);
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<List<AuditLogDto>>> GetEntityAuditHistory(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var logs = await _auditLogService.GetEntityAuditHistoryAsync(entityType, entityId, cancellationToken);
        return Ok(logs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuditLogDto>> GetAuditLog(
        Guid id,
        CancellationToken cancellationToken)
    {
        var dto = await _auditLogService.GetByIdAsync(id, cancellationToken);

        if (dto == null)
            return NotFound();

        return Ok(dto);
    }

}
