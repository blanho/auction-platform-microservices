using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Authorization;

namespace Analytics.Api.Endpoints;

public class AuditLogsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auditlogs")
            .WithTags("AuditLogs")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.AuditLogs.View));

        group.MapGet("", GetAuditLogs)
            .WithName("GetAuditLogs")
            .Produces<PaginatedResult<AuditLogDto>>();

        group.MapGet("/entity/{entityType}/{entityId:guid}", GetEntityAuditHistory)
            .WithName("GetEntityAuditHistory")
            .Produces<List<AuditLogDto>>();

        group.MapGet("/{id:guid}", GetAuditLog)
            .WithName("GetAuditLog")
            .Produces<AuditLogDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Ok<PaginatedResult<AuditLogDto>>> GetAuditLogs(
        [AsParameters] AuditLogQueryParams queryParams,
        IAuditLogService auditLogService,
        CancellationToken cancellationToken)
    {
        var result = await auditLogService.GetPagedAuditLogsAsync(queryParams, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<List<AuditLogDto>>> GetEntityAuditHistory(
        string entityType,
        Guid entityId,
        IAuditLogService auditLogService,
        CancellationToken cancellationToken)
    {
        var logs = await auditLogService.GetEntityAuditHistoryAsync(entityType, entityId, cancellationToken);
        return TypedResults.Ok(logs);
    }

    private static async Task<Results<Ok<AuditLogDto>, NotFound>> GetAuditLog(
        Guid id,
        IAuditLogService auditLogService,
        CancellationToken cancellationToken)
    {
        var dto = await auditLogService.GetByIdAsync(id, cancellationToken);

        if (dto == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(dto);
    }
}
