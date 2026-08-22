using Carter;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Analytics.Application.DTOs;
using Analytics.Application.Features.Reports.CreateReport;
using Analytics.Application.Features.Reports.DeleteReport;
using Analytics.Application.Features.Reports.GetReportById;
using Analytics.Application.Features.Reports.GetReports;
using Analytics.Application.Features.Reports.GetReportStats;
using Analytics.Application.Features.Reports.UpdateReportStatus;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Helpers;

namespace Analytics.Api.Endpoints.Reports;

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
            .WithValidation<CreateReportDto>()
            .Produces<ReportDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}/status", UpdateReportStatus)
            .WithName("UpdateReportStatus")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Reports.Manage))
            .WithValidation<UpdateReportStatusDto>()
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
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetReportsQuery(queryParams), cancellationToken);
        return result.ToOkResult();
    }

    private static async Task<IResult> GetReport(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetReportByIdQuery(id), cancellationToken);
        return result.ToOkResult();
    }

    private static async Task<IResult> CreateReport(
        CreateReportDto dto,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(httpContext.User);
        var result = await sender.Send(new CreateReportCommand(username, dto), cancellationToken);
        return result.ToApiResult(value => Results.Created($"/api/v1/reports/{value!.Id}", value));
    }

    private static async Task<IResult> UpdateReportStatus(
        Guid id,
        UpdateReportStatusDto dto,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var adminUsername = UserHelper.GetUsername(httpContext.User);
        var result = await sender.Send(new UpdateReportStatusCommand(id, dto, adminUsername), cancellationToken);
        return result.ToNoContentResult();
    }

    private static async Task<IResult> DeleteReport(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteReportCommand(id), cancellationToken);
        return result.ToNoContentResult();
    }

    private static async Task<IResult> GetStats(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetReportStatsQuery(), cancellationToken);
        return result.ToOkResult();
    }
}
