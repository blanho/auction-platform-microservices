using Carter;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Web.Authorization;
using System.Security.Claims;
using Search.Api.Services;

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
        [FromServices] IAuctionSearchService searchService,
        [FromQuery] string? query = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? categorySlug = null,
        [FromQuery] Guid? brandId = null,
        [FromQuery] Guid? sellerId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? condition = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeFacets = false,
        CancellationToken ct = default)
    {
        var request = new AuctionSearchRequest
        {
            Query = query,
            CategoryId = categoryId,
            CategorySlug = categorySlug,
            BrandId = brandId,
            SellerId = sellerId,
            Statuses = string.IsNullOrEmpty(status) ? null : new List<string> { status },
            Conditions = string.IsNullOrEmpty(condition) ? null : new List<string> { condition },
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            SortBy = sortBy,
            Page = page,
            PageSize = pageSize,
            IncludeFacets = includeFacets
        };

        var response = await searchService.SearchAsync(request, ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> SearchAuctionsAdvanced(
        [FromServices] IAuctionSearchService searchService,
        [FromBody] AuctionSearchRequest request,
        CancellationToken ct = default)
    {
        var response = await searchService.SearchAsync(request, ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> Autocomplete(
        [FromServices] IAuctionSearchService searchService,
        [FromQuery] string q,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            return Results.Ok(new List<string>());
        }

        var suggestions = await searchService.AutocompleteAsync(q, limit, ct);
        return Results.Ok(suggestions);
    }

    private static async Task<IResult> GetAuction(
        [FromServices] IAuctionSearchService searchService,
        Guid id,
        CancellationToken ct = default)
    {
        var document = await searchService.GetByIdAsync(id, ct);

        if (document == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(document);
    }

    private static Task<IResult> GetRecentSearches(
        ClaimsPrincipal user,
        [FromServices] IRecentSearchService recentSearchService)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Task.FromResult(Results.Ok(new List<string>()));
        }

        var recentSearches = recentSearchService.GetRecentSearches(userId);
        return Task.FromResult(Results.Ok(recentSearches));
    }

    private static Task<IResult> ClearRecentSearches(
        ClaimsPrincipal user,
        [FromServices] IRecentSearchService recentSearchService)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            recentSearchService.ClearRecentSearches(userId);
        }

        return Task.FromResult(Results.NoContent());
    }

    private static Task<IResult> GetPopularSearches(
        [FromServices] IRecentSearchService recentSearchService)
    {
        var popularSearches = recentSearchService.GetPopularSearches();
        return Task.FromResult(Results.Ok(popularSearches));
    }
}
