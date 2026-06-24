using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Web.Authorization;

namespace Search.Api.Endpoints;

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
        CancellationToken ct)
    {
        var result = await sender.Send(new Search.Application.Features.Admin.Commands.EnsureIndex.EnsureIndexCommand(), ct);
        return result.IsSuccess
            ? Results.Ok(new { message = "Index ready" })
            : Results.Problem($"Failed to create index: {result.Error}");
    }

    private static async Task<IResult> RecreateIndex(
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new Search.Application.Features.Admin.Commands.RecreateIndex.RecreateIndexCommand(), ct);
        return result.IsSuccess
            ? Results.Ok(new { message = "Index recreated" })
            : Results.Problem($"Failed to recreate index: {result.Error}");
    }

    private static async Task<IResult> GetIndexStats(
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new Search.Application.Features.Admin.Queries.GetIndexStats.GetIndexStatsQuery(), ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem($"Failed to get index stats: {result.Error}");
    }

    private static async Task<IResult> GetHealth(
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new Search.Application.Features.Admin.Queries.GetHealth.GetHealthQuery(), ct);
        return result.IsSuccess
            ? Results.Ok(new { status = "healthy", elasticsearch = "connected" })
            : Results.Problem("Elasticsearch not responding");
    }
}
