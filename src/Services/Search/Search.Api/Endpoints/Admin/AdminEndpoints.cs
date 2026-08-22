using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Application.Localization;
using BuildingBlocks.Web.Helpers;
using BuildingBlocks.Web.Constants;
using Search.Application.Features.Admin.Commands.EnsureIndex;
using Search.Application.Features.Admin.Commands.RecreateIndex;
using Search.Application.Features.Admin.Queries.GetHealth;
using Search.Application.Features.Admin.Queries.GetIndexStats;

namespace Search.Api.Endpoints.Admin;

public class AdminEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/search")
            .WithTags("Search Admin")
            .WithMetadata(new RequireAdminAttribute());

        group.MapPost("/indices", EnsureIndex)
            .WithName("EnsureSearchIndex")
            .WithSummary("Ensure index exists");

        group.MapDelete("/indices", RecreateIndex)
            .WithName("RecreateSearchIndex")
            .WithSummary("Recreate index (deletes all data!)");

        group.MapGet("/indices/stats", GetIndexStats)
            .WithName("GetSearchIndexStats")
            .WithSummary("Get index statistics");

        group.MapGet("/health", GetHealth)
            .WithName("GetSearchHealth")
            .WithSummary("Get search service health");
    }

    private static async Task<IResult> EnsureIndex(
        ISender sender,
        ILocalizationService localizer,
        CancellationToken ct)
    {
        var result = await sender.Send(new EnsureIndexCommand(), ct);
        return result.IsSuccess
            ? Results.Ok(new { message = localizer[LocalizationKeys.Index.Ready] })
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> RecreateIndex(
        ISender sender,
        ILocalizationService localizer,
        CancellationToken ct)
    {
        var result = await sender.Send(new RecreateIndexCommand(), ct);
        return result.IsSuccess
            ? Results.Ok(new { message = localizer[LocalizationKeys.Index.Recreated] })
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetIndexStats(
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetIndexStatsQuery(), ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetHealth(
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetHealthQuery(), ct);
        return result.IsSuccess
            ? Results.Ok(new { status = "healthy", elasticsearch = "connected" })
            : Results.Json(
                ProblemDetailsHelper.FromError(result.Error!),
                statusCode: StatusCodes.Status503ServiceUnavailable,
                contentType: MediaTypeConstants.ProblemJson);
    }
}
