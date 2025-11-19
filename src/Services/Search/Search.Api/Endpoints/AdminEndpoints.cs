using Carter;
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
        [FromServices] IIndexManagementService indexService,
        CancellationToken ct)
    {
        var success = await indexService.EnsureIndexExistsAsync(ct);
        return success
            ? Results.Ok(new { message = "Index ready" })
            : Results.Problem("Failed to create index");
    }

    private static async Task<IResult> RecreateIndex(
        [FromServices] IIndexManagementService indexService,
        CancellationToken ct)
    {
        await indexService.RecreateIndexAsync(ct);
        return Results.Ok(new { message = "Index recreated" });
    }

    private static async Task<IResult> GetIndexStats(
        [FromServices] IIndexManagementService indexService,
        CancellationToken ct)
    {
        var stats = await indexService.GetIndexStatsAsync(ct);
        return stats != null
            ? Results.Ok(stats)
            : Results.NotFound();
    }

    private static async Task<IResult> GetHealth(
        [FromServices] Elastic.Clients.Elasticsearch.ElasticsearchClient client,
        CancellationToken ct)
    {
        try
        {
            var response = await client.PingAsync(ct);
            return response.IsValidResponse
                ? Results.Ok(new { status = "healthy", elasticsearch = "connected" })
                : Results.Problem("Elasticsearch not responding");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Elasticsearch error: {ex.Message}");
        }
    }
}
