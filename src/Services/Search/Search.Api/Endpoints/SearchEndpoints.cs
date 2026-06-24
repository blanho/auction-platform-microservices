using Carter;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Web.Authorization;
using System.Security.Claims;
using MediatR;
using Search.Domain.Constants;
using Search.Domain.Documents;
using Search.Domain.Models;
using Search.Application.Features.Auctions.Queries.SearchAuctions;
using Search.Application.Features.Auctions.Queries.Autocomplete;
using Search.Application.Features.Auctions.Queries.GetAuction;
using Search.Application.Features.RecentSearches.Queries.GetRecentSearches;
using Search.Application.Features.RecentSearches.Commands.ClearRecentSearches;
using Search.Application.Features.RecentSearches.Queries.GetPopularSearches;

namespace Search.Api.Endpoints;

public class SearchEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/search")
            .WithTags("Search")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.View));

        group.MapGet("/auctions", SearchAuctions)
            .WithName("SearchAuctions")
            .WithSummary("Search auctions with filters")
            .Produces<AuctionSearchResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/auctions", SearchAuctionsAdvanced)
            .WithName("SearchAuctionsAdvanced")
            .WithSummary("Advanced search with complex filters")
            .Produces<AuctionSearchResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/autocomplete", Autocomplete)
            .WithName("SearchAutocomplete")
            .WithSummary("Get autocomplete suggestions")
            .Produces<List<string>>();

        group.MapGet("/auctions/{id:guid}", GetAuction)
            .WithName("GetAuctionFromSearch")
            .WithSummary("Get single auction from search index")
            .Produces<AuctionDocument>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        var publicGroup = app.MapGroup("/api/v1/search")
            .WithTags("Search");

        publicGroup.MapGet("/popular", GetPopularSearches)
            .WithName("GetPopularSearches")
            .WithSummary("Get popular search terms")
            .Produces<List<string>>()
            .AllowAnonymous();

        var authGroup = app.MapGroup("/api/v1/search")
            .WithTags("Search")
            .RequireAuthorization();

        authGroup.MapGet("/recent", GetRecentSearches)
            .WithName("GetRecentSearches")
            .WithSummary("Get user's recent searches")
            .Produces<List<string>>();

        authGroup.MapDelete("/recent", ClearRecentSearches)
            .WithName("ClearRecentSearches")
            .WithSummary("Clear user's recent searches")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> SearchAuctions(
        ISender sender,
        [AsParameters] AuctionSearchRequest request,
        CancellationToken ct = default) =>
        Results.Ok(await sender.Send(new SearchAuctionsQuery(request), ct));

    private static async Task<IResult> SearchAuctionsAdvanced(
        ISender sender,
        [FromBody] AuctionSearchRequest request,
        CancellationToken ct = default) =>
        Results.Ok(await sender.Send(new SearchAuctionsQuery(request), ct));

    private static async Task<IResult> Autocomplete(
        ISender sender,
        [FromQuery] string q,
        [FromQuery] int limit = SearchDefaults.DefaultAutocompleteLimit,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < SearchDefaults.MinAutocompleteLength)
        {
            return Results.Ok(new List<string>());
        }

        var suggestions = await sender.Send(new AutocompleteQuery(q, limit), ct);
        return Results.Ok(suggestions);
    }

    private static async Task<IResult> GetAuction(
        ISender sender,
        Guid id,
        CancellationToken ct = default)
    {
        var document = await sender.Send(new GetAuctionQuery(id), ct);

        if (document == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(document);
    }

    private static async Task<IResult> GetRecentSearches(
        ClaimsPrincipal user,
        ISender sender)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Ok(new List<string>());
        }

        var recentSearches = await sender.Send(new GetRecentSearchesQuery(userId));
        return Results.Ok(recentSearches);
    }

    private static async Task<IResult> ClearRecentSearches(
        ClaimsPrincipal user,
        ISender sender)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            await sender.Send(new ClearRecentSearchesCommand(userId));
        }

        return Results.NoContent();
    }

    private static async Task<IResult> GetPopularSearches(
        ISender sender)
    {
        var popularSearches = await sender.Send(new GetPopularSearchesQuery());
        return Results.Ok(popularSearches);
    }
}
