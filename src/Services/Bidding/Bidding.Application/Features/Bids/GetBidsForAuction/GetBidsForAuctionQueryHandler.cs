using Bidding.Application.DTOs;
using Bidding.Application.Filtering;
using Bidding.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS;

namespace Bidding.Application.Features.Bids.Queries.GetBidsForAuction;

public class GetBidsForAuctionQueryHandler : IQueryHandler<GetBidsForAuctionQuery, PaginatedResult<BidDto>>
{
    private readonly IBidService _bidService;

    public GetBidsForAuctionQueryHandler(IBidService bidService)
    {
        _bidService = bidService;
    }

    public async Task<Result<PaginatedResult<BidDto>>> Handle(GetBidsForAuctionQuery request, CancellationToken cancellationToken)
    {
        var queryParams = new BidQueryParams
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Filter = new BidFilter
            {
                AuctionId = request.AuctionId,
                Status = request.Status,
                MinAmount = request.MinAmount,
                MaxAmount = request.MaxAmount,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            }
        };

        var result = await _bidService.GetBidsForAuctionAsync(queryParams, cancellationToken);
        return Result<PaginatedResult<BidDto>>.Success(result);
    }
}
