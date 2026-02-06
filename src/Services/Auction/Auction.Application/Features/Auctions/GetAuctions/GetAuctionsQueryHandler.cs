using Auctions.Application.DTOs;
using Auctions.Application.Services;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Queries.GetAuctions;

public class GetAuctionsQueryHandler : IQueryHandler<GetAuctionsQuery, PaginatedResult<AuctionDto>>
{
    private readonly IPaginatedAuctionQueryService _queryService;
    private readonly ILogger<GetAuctionsQueryHandler> _logger;

    public GetAuctionsQueryHandler(
        IPaginatedAuctionQueryService queryService,
        ILogger<GetAuctionsQueryHandler> logger)
    {
        _queryService = queryService;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<AuctionDto>>> Handle(GetAuctionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching auctions with filters - Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            request.Status ?? "All", request.Page, request.PageSize);

        var queryParams = new AuctionFilterDto
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.OrderBy,
            SortDescending = request.Descending,
            Filter = new AuctionFilter
            {
                Status = request.Status,
                Seller = request.Seller,
                Winner = request.Winner,
                SearchTerm = request.SearchTerm,
                Category = request.Category,
                IsFeatured = request.IsFeatured
            }
        };

        var paginatedResult = await _queryService.GetPagedAuctionsAsync(queryParams, cancellationToken);

        _logger.LogDebug("Retrieved {Count} auctions out of {Total}", paginatedResult.Items.Count, paginatedResult.TotalCount);

        return Result.Success(paginatedResult);
    }
}

