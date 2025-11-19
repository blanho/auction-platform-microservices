using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Authorization;

namespace Analytics.Api.Endpoints;

public class ReportsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/reports")
            .WithTags("Reports");

        group.MapGet("", GetReports)
            .WithName("GetReports")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewPlatform))
            .Produces<PaginatedResult<ReportDto>>();

        group.MapGet("/{id:guid}", GetReport)
            .WithName("GetReport")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewPlatform))
            .Produces<ReportDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("", CreateReport)
            .WithName("CreateReport")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Reports.Create))
            .Produces<ReportDto>(StatusCodes.Status201Created);

        group.MapPut("/{id:guid}/status", UpdateReportStatus)
            .WithName("UpdateReportStatus")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Reports.Manage))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteReport)
            .WithName("DeleteReport")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Reports.Delete))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/stats", GetStats)
            .WithName("GetReportStats")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewPlatform))
            .Produces<ReportStatsDto>();
    }

    private static async Task<Ok<PaginatedResult<ReportDto>>> GetReports(
        [AsParameters] ReportQueryParams queryParams,
        IReportService reportService,
        CancellationToken cancellationToken)
    {
        var result = await reportService.GetReportsAsync(queryParams, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<ReportDto>, NotFound<string>>> GetReport(
        Guid id,
        IReportService reportService,
        CancellationToken cancellationToken)
    {
        var report = await reportService.GetReportByIdAsync(id, cancellationToken);
        return report is not null
            ? TypedResults.Ok(report)
            : TypedResults.NotFound("Report not found");
    }

    private static async Task<Created<ReportDto>> CreateReport(
        CreateReportDto dto,
        HttpContext httpContext,
        IReportService reportService,
        CancellationToken cancellationToken)
    {
        var username = httpContext.User.Identity?.Name
            ?? httpContext.User.FindFirst("username")?.Value
            ?? "Anonymous";

        var report = await reportService.CreateReportAsync(username, dto, cancellationToken);
        return TypedResults.Created($"/api/reports/{report.Id}", report);
    }

    private static async Task<NoContent> UpdateReportStatus(
        Guid id,
        UpdateReportStatusDto dto,
        HttpContext httpContext,
        IReportService reportService,
        CancellationToken cancellationToken)
    {
        var adminUsername = httpContext.User.Identity?.Name
            ?? httpContext.User.FindFirst("username")?.Value
            ?? "Anonymous";
        await reportService.UpdateReportStatusAsync(id, dto, adminUsername, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteReport(
        Guid id,
        IReportService reportService,
        CancellationToken cancellationToken)
    {
        await reportService.DeleteReportAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<ReportStatsDto>> GetStats(
        IReportService reportService,
        CancellationToken cancellationToken)
    {
        var stats = await reportService.GetReportStatsAsync(cancellationToken);
        return TypedResults.Ok(stats);
    }
}
