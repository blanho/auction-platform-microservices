using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;

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
            .Produces<PaginatedResult<ReportDto>>()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetReport)
            .WithName("GetReport")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewPlatform))
            .Produces<ReportDto>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("", CreateReport)
            .WithName("CreateReport")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Reports.Create))
            .Produces<ReportDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}/status", UpdateReportStatus)
            .WithName("UpdateReportStatus")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Reports.Manage))
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteReport)
            .WithName("DeleteReport")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Reports.Delete))
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/stats", GetStats)
            .WithName("GetReportStats")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewPlatform))
            .Produces<ReportStatsDto>();
    }

    private static async Task<IResult> GetReports(
        [AsParameters] ReportQueryParams queryParams,
        IReportService reportService,
        CancellationToken cancellationToken)
    {
        var result = await reportService.GetReportsAsync(queryParams, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetReport(
        Guid id,
        IReportService reportService,
        CancellationToken cancellationToken)
    {
        var result = await reportService.GetReportByIdAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(ProblemDetailsHelper.NotFound("Report", id));
    }

    private static async Task<IResult> CreateReport(
        CreateReportDto dto,
        HttpContext httpContext,
        IReportService reportService,
        CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(httpContext.User);
        var result = await reportService.CreateReportAsync(username, dto, cancellationToken);
        
        return result.IsSuccess
            ? Results.Created($"/api/v1/reports/{result.Value!.Id}", result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> UpdateReportStatus(
        Guid id,
        UpdateReportStatusDto dto,
        HttpContext httpContext,
        IReportService reportService,
        CancellationToken cancellationToken)
    {
        var adminUsername = UserHelper.GetUsername(httpContext.User);
        var result = await reportService.UpdateReportStatusAsync(id, dto, adminUsername, cancellationToken);
        
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.NotFound("Report", id));
    }

    private static async Task<IResult> DeleteReport(
        Guid id,
        IReportService reportService,
        CancellationToken cancellationToken)
    {
        var result = await reportService.DeleteReportAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.NotFound("Report", id));
    }

    private static async Task<IResult> GetStats(
        IReportService reportService,
        CancellationToken cancellationToken)
    {
        var result = await reportService.GetReportStatsAsync(cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }
}
