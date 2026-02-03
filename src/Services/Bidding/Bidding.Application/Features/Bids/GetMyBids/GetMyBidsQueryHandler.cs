using Bidding.Application.DTOs;
using Bidding.Application.Filtering;
using Bidding.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS;

namespace Bidding.Application.Features.Bids.Queries.GetMyBids;

public class GetMyBidsQueryHandler : IQueryHandler<GetMyBidsQuery, PaginatedResult<BidDto>>
{
    private readonly IBidService _bidService;

    public GetMyBidsQueryHandler(IBidService bidService)
    {
        _bidService = bidService;
    }

    public async Task<Result<PaginatedResult<BidDto>>> Handle(GetMyBidsQuery request, CancellationToken cancellationToken)
    {
        var queryParams = new BidQueryParams
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Filter = new BidFilter
            {
                BidderUsername = request.Username,
                AuctionId = request.AuctionId,
                Status = request.Status,
                MinAmount = request.MinAmount,
                MaxAmount = request.MaxAmount,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            }
        };

        var result = await _bidService.GetBidsForBidderAsync(queryParams, cancellationToken);
        return Result<PaginatedResult<BidDto>>.Success(result);
    }
}
