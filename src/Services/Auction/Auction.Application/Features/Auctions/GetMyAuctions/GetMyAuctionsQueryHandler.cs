using Auctions.Application.DTOs;
using Auctions.Application.Services;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Queries.GetMyAuctions;

public class GetMyAuctionsQueryHandler : IQueryHandler<GetMyAuctionsQuery, PaginatedResult<AuctionDto>>
{
    private readonly IPaginatedAuctionQueryService _queryService;
    private readonly ILogger<GetMyAuctionsQueryHandler> _logger;

    public GetMyAuctionsQueryHandler(
        IPaginatedAuctionQueryService queryService,
        ILogger<GetMyAuctionsQueryHandler> logger)
    {
        _queryService = queryService;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<AuctionDto>>> Handle(GetMyAuctionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching user auctions - Status: {Status}, Page: {Page}",
            request.Status ?? "All", request.Page);

        var queryParams = new AuctionFilterDto
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.OrderBy,
            SortDescending = request.Descending,
            Filter = new AuctionFilter
            {
                Status = request.Status,
                Seller = request.Username,
                SearchTerm = request.SearchTerm
            }
        };

        var paginatedResult = await _queryService.GetPagedAuctionsAsync(queryParams, cancellationToken);

        _logger.LogDebug("Retrieved {Count} auctions out of {Total}",
            paginatedResult.Items.Count, paginatedResult.TotalCount);

        return Result.Success(paginatedResult);
    }
}

