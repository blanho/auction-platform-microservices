using Carter;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Analytics.Application.DTOs;
using Analytics.Application.Features.AuditLogs.GetAuditLogById;
using Analytics.Application.Features.AuditLogs.GetEntityAuditHistory;
using Analytics.Application.Features.AuditLogs.GetPagedAuditLogs;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Authorization;

namespace Analytics.Api.Endpoints.AuditLogs;

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
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPagedAuditLogsQuery(queryParams), cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<List<AuditLogDto>>> GetEntityAuditHistory(
        string entityType,
        Guid entityId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var logs = await sender.Send(new GetEntityAuditHistoryQuery(entityType, entityId), cancellationToken);
        return TypedResults.Ok(logs);
    }

    private static async Task<Results<Ok<AuditLogDto>, NotFound>> GetAuditLog(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var dto = await sender.Send(new GetAuditLogByIdQuery(id), cancellationToken);

        if (dto == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(dto);
    }
}
